using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using chatrealtime.Models;
using Microsoft.Extensions.Options;
using chatrealtime.Configuration;

namespace chatrealtime.Services;

public class RealtimeWebSocketHandler
{
    private readonly ILogger<RealtimeWebSocketHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RealtimeWebSocketHandler(
        ILogger<RealtimeWebSocketHandler> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleWebSocketAsync(HttpContext context, WebSocket clientWebSocket)
    {
        _logger.LogInformation("New WebSocket connection from client");

        // Create a new OpenAI service instance for this client
        using var scope = _serviceProvider.CreateScope();
        var openAIService = scope.ServiceProvider.GetRequiredService<OpenAIRealtimeService>();

        var clientSendLock = new SemaphoreSlim(1, 1);
        var cancellationTokenSource = new CancellationTokenSource();

        try
        {
            // Set up event handlers for OpenAI responses
            openAIService.OnAudioReceived += async (audioData) =>
            {
                _logger.LogDebug("[Handler] Sending audio to client, size: {Size}", audioData?.Length ?? 0);
                await SendToClientAsync(clientWebSocket, clientSendLock, new ServerMessage
                {
                    Type = "audio",
                    Audio = audioData
                }, cancellationTokenSource.Token);
            };

            openAIService.OnTranscriptReceived += async (role, transcript) =>
            {
                _logger.LogDebug("[Handler] Sending transcript to client - Role: {Role}, Text: {Text}", role, transcript);
                await SendToClientAsync(clientWebSocket, clientSendLock, new ServerMessage
                {
                    Type = "transcript",
                    Role = role,
                    Transcript = transcript
                }, cancellationTokenSource.Token);
            };

            openAIService.OnError += async (error) =>
            {
                await SendToClientAsync(clientWebSocket, clientSendLock, new ServerMessage
                {
                    Type = "error",
                    Error = error
                }, cancellationTokenSource.Token);
            };

            openAIService.OnStatusChanged += async (status) =>
            {
                await SendToClientAsync(clientWebSocket, clientSendLock, new ServerMessage
                {
                    Type = "status",
                    Status = status
                }, cancellationTokenSource.Token);
            };

            // Connect to OpenAI
            var connected = await openAIService.ConnectAsync(cancellationTokenSource.Token);
            if (!connected)
            {
                _logger.LogError("Failed to connect to OpenAI");
                await clientWebSocket.CloseAsync(
                    WebSocketCloseStatus.InternalServerError,
                    "Failed to connect to OpenAI",
                    CancellationToken.None);
                return;
            }

            // Notify client that we're ready
            await SendToClientAsync(clientWebSocket, clientSendLock, new ServerMessage
            {
                Type = "ready",
                Status = "Connected to OpenAI"
            }, cancellationTokenSource.Token);

            // Receive messages from client
            var buffer = new byte[1024 * 16]; // 16KB buffer
            var messageBuilder = new StringBuilder();

            while (clientWebSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await clientWebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Client requested close");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuilder.Append(message);

                if (result.EndOfMessage)
                {
                    var completeMessage = messageBuilder.ToString();
                    messageBuilder.Clear();

                    await ProcessClientMessageAsync(completeMessage, openAIService, cancellationTokenSource.Token);
                }
            }

            // Clean up
            await openAIService.DisconnectAsync();
            await clientWebSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Connection closed",
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("WebSocket operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WebSocket connection");
            try
            {
                if (clientWebSocket.State == WebSocketState.Open)
                {
                    await clientWebSocket.CloseAsync(
                        WebSocketCloseStatus.InternalServerError,
                        "Internal server error",
                        CancellationToken.None);
                }
            }
            catch { }
        }
        finally
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            clientSendLock.Dispose();
        }
    }

    private async Task ProcessClientMessageAsync(
        string message,
        OpenAIRealtimeService openAIService,
        CancellationToken cancellationToken)
    {
        try
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessage>(message);
            if (clientMessage == null)
            {
                _logger.LogWarning("Failed to deserialize client message");
                return;
            }

            _logger.LogDebug("Client message type: {Type}", clientMessage.Type);

            switch (clientMessage.Type)
            {
                case "audio":
                    if (!string.IsNullOrEmpty(clientMessage.Audio))
                    {
                        await openAIService.SendAudioAsync(clientMessage.Audio, cancellationToken);
                    }
                    break;

                case "text":
                    if (!string.IsNullOrEmpty(clientMessage.Text))
                    {
                        await openAIService.SendTextAsync(clientMessage.Text, cancellationToken);
                    }
                    break;

                case "audio_commit":
                    await openAIService.CommitAudioAsync(cancellationToken);
                    break;

                case "create_response":
                    await openAIService.CreateResponseAsync(cancellationToken);
                    break;

                case "cancel_response":
                    await openAIService.CancelResponseAsync(cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unknown client message type: {Type}", clientMessage.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client message: {Message}", message);
        }
    }

    private async Task SendToClientAsync(
        WebSocket clientWebSocket,
        SemaphoreSlim sendLock,
        ServerMessage message,
        CancellationToken cancellationToken)
    {
        if (clientWebSocket.State != WebSocketState.Open)
        {
            return;
        }

        await sendLock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await clientWebSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to client");
        }
        finally
        {
            sendLock.Release();
        }
    }
}
