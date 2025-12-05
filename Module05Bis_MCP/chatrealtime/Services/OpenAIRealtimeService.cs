using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using chatrealtime.Configuration;
using chatrealtime.Models;
using chatrealtime.Services.Tools;
using Microsoft.Extensions.Options;

namespace chatrealtime.Services;

public class OpenAIRealtimeService : IDisposable
{
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIRealtimeService> _logger;
    private readonly IToolExecutor _toolExecutor;
    private readonly McpDiscoveryService _mcpDiscovery;
    private ClientWebSocket? _openAIWebSocket;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private CancellationTokenSource? _receiveCts;
    private string _systemInstructions = string.Empty;
    private bool _isResponseActive = false; // Track if assistant is currently responding
    private string _mcpDiscoveryInfo = string.Empty; // MCP capabilities discovered at startup
    private List<ToolConfig> _discoveredMcpTools = new(); // Dynamically discovered MCP tools

    public event Func<string, Task>? OnAudioReceived;
    public event Func<string, string, Task>? OnTranscriptReceived; // role, transcript
    public event Func<string, Task>? OnError;
    public event Func<string, Task>? OnStatusChanged;

    public OpenAIRealtimeService(
        IOptions<OpenAISettings> settings,
        ILogger<OpenAIRealtimeService> logger,
        IToolExecutor toolExecutor,
        McpDiscoveryService mcpDiscovery)
    {
        _settings = settings.Value;
        _logger = logger;
        _toolExecutor = toolExecutor;
        _mcpDiscovery = mcpDiscovery;
        LoadSystemInstructions();
    }

    private void LoadSystemInstructions()
    {
        try
        {
            // Use the configured Instructions if SystemPromptFile is not set
            if (string.IsNullOrEmpty(_settings.SystemPromptFile))
            {
                _systemInstructions = _settings.Instructions;
                _logger.LogInformation("Using inline instructions from configuration");
                return;
            }

            // Try to load the system prompt from file
            if (File.Exists(_settings.SystemPromptFile))
            {
                _systemInstructions = File.ReadAllText(_settings.SystemPromptFile);
                _logger.LogInformation("Loaded system instructions from: {File}", _settings.SystemPromptFile);
            }
            else
            {
                _logger.LogWarning("System prompt file not found: {File}. Using inline instructions.", _settings.SystemPromptFile);
                _systemInstructions = _settings.Instructions;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system prompt file. Using inline instructions.");
            _systemInstructions = _settings.Instructions;
        }
    }

    private async Task DiscoverMcpCapabilitiesAsync(CancellationToken cancellationToken)
    {
        if (_settings.McpServers == null || !_settings.McpServers.Any())
        {
            _logger.LogInformation("No MCP servers configured, skipping discovery");
            return;
        }

        _logger.LogInformation("Starting MCP discovery process...");

        try
        {
            // Discover all MCP tools dynamically
            _discoveredMcpTools = await _mcpDiscovery.DiscoverAllServersAsync(cancellationToken);
            
            // Add discovered tools to settings so ToolExecutor can find them
            _settings.Tools ??= new List<ToolConfig>();
            _settings.Tools.AddRange(_discoveredMcpTools);
            
            // Generate a summary for the system prompt
            _mcpDiscoveryInfo = await _mcpDiscovery.GenerateCapabilitiesSummaryAsync(cancellationToken);
            
            _logger.LogInformation("MCP discovery completed. Discovered {Count} dynamic tools", _discoveredMcpTools.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MCP discovery");
            _mcpDiscoveryInfo = "\n## ⚠️ Erreur lors de la découverte MCP\n\nCertaines capacités MCP peuvent ne pas être disponibles.";
        }
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.ApiKey) || _settings.ApiKey == "YOUR_OPENAI_API_KEY_HERE")
            {
                _logger.LogError("OpenAI API Key is not configured");
                await NotifyError("OpenAI API Key is not configured in appsettings.json");
                return false;
            }

            _openAIWebSocket = new ClientWebSocket();
            _openAIWebSocket.Options.SetRequestHeader("Authorization", $"Bearer {_settings.ApiKey}");
            _openAIWebSocket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");

            var uri = new Uri($"{_settings.RealtimeUrl}?model={_settings.Model}");
            _logger.LogInformation("Connecting to OpenAI Realtime API: {Uri}", uri);

            await _openAIWebSocket.ConnectAsync(uri, cancellationToken);
            _logger.LogInformation("Connected to OpenAI Realtime API");

            await NotifyStatus("Connected to OpenAI");

            // Discover MCP capabilities before configuring session
            await DiscoverMcpCapabilitiesAsync(cancellationToken);

            // Configure session
            await ConfigureSessionAsync(cancellationToken);

            // Start receiving messages
            _receiveCts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveMessagesAsync(_receiveCts.Token), _receiveCts.Token);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to OpenAI");
            await NotifyError($"Connection error: {ex.Message}");
            return false;
        }
    }

    private async Task ConfigureSessionAsync(CancellationToken cancellationToken)
    {
        // Combine configured tools with discovered MCP tools
        var allToolConfigs = new List<ToolConfig>();
        
        // Add manually configured tools (builtin, http, etc.)
        if (_settings.Tools != null && _settings.Tools.Any())
        {
            allToolConfigs.AddRange(_settings.Tools);
        }
        
        // Add dynamically discovered MCP tools
        if (_discoveredMcpTools != null && _discoveredMcpTools.Any())
        {
            allToolConfigs.AddRange(_discoveredMcpTools);
            _logger.LogInformation("Adding {Count} dynamically discovered MCP tools", _discoveredMcpTools.Count);
        }

        // Convert all tools to API format
        List<Tool>? tools = null;
        if (allToolConfigs.Any())
        {
            tools = new List<Tool>();
            foreach (var t in allToolConfigs)
            {
                try
                {
                    // Convert JsonElement to object for serialization
                    object parameters;
                    
                    if (t.Parameters.ValueKind == System.Text.Json.JsonValueKind.Undefined || 
                        t.Parameters.ValueKind == System.Text.Json.JsonValueKind.Null)
                    {
                        // Default empty parameters schema
                        parameters = new 
                        { 
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        };
                        _logger.LogWarning("Tool {ToolName} has no parameters defined, using empty schema", t.Name);
                    }
                    else
                    {
                        var rawJson = t.Parameters.GetRawText();
                        parameters = System.Text.Json.JsonSerializer.Deserialize<object>(rawJson) ?? new { };
                        _logger.LogDebug("Added tool {ToolName} (type: {Type})", t.Name, t.Type);
                    }
                    
                    tools.Add(new Tool
                    {
                        Type = "function",
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = parameters
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error configuring tool {ToolName}", t.Name);
                }
            }
            
            _logger.LogInformation("Configured {Count} total tools for OpenAI: {Tools}", 
                tools.Count, 
                string.Join(", ", tools.Select(t => t.Name)));
        }

        // Combine system instructions with MCP discovery info
        var fullInstructions = _systemInstructions;
        if (!string.IsNullOrEmpty(_mcpDiscoveryInfo))
        {
            fullInstructions += _mcpDiscoveryInfo;
        }

        var sessionUpdate = new SessionUpdateEvent
        {
            Session = new SessionConfig
            {
                Modalities = new[] { "text", "audio" },
                Instructions = fullInstructions,
                Voice = _settings.Voice,
                InputAudioFormat = "pcm16",
                OutputAudioFormat = "pcm16",
                // Use the configured transcription model (default: gpt-4o-transcribe)
                // Reference: https://community.openai.com/t/cant-get-the-user-transcription-in-realtime-api/1076308/5
                InputAudioTranscription = new TranscriptionConfig
                {
                    Model = _settings.TranscriptionModel
                },
                TurnDetection = new TurnDetectionConfig
                {
                    Type = _settings.TurnDetection.Type,
                    Threshold = _settings.TurnDetection.Threshold,
                    PrefixPaddingMs = _settings.TurnDetection.PrefixPaddingMs,
                    SilenceDurationMs = _settings.TurnDetection.SilenceDurationMs
                },
                Temperature = _settings.Temperature,
                MaxResponseOutputTokens = _settings.MaxResponseOutputTokens,
                Tools = tools,
                ToolChoice = tools?.Any() == true ? "auto" : null
            }
        };

        await SendToOpenAIAsync(sessionUpdate, cancellationToken);
        _logger.LogInformation("Session configured");
    }

    public async Task SendAudioAsync(string base64Audio, CancellationToken cancellationToken = default)
    {
        if (_openAIWebSocket?.State != WebSocketState.Open)
        {
            _logger.LogWarning("WebSocket is not open, cannot send audio");
            return;
        }

        var appendEvent = new InputAudioBufferAppendEvent
        {
            Audio = base64Audio
        };

        await SendToOpenAIAsync(appendEvent, cancellationToken);
    }

    public async Task SendTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (_openAIWebSocket?.State != WebSocketState.Open)
        {
            _logger.LogWarning("WebSocket is not open, cannot send text");
            return;
        }

        _logger.LogInformation("Sending text message: {Text}", text);

        // Create a conversation item with the user's text
        var createItemEvent = new ConversationItemCreateEvent
        {
            Item = new UserMessageItem
            {
                Type = "message",
                Role = "user",
                Content = new List<MessageContent>
                {
                    new MessageContent
                    {
                        Type = "input_text",
                        Text = text
                    }
                }
            }
        };

        await SendToOpenAIAsync(createItemEvent, cancellationToken);

        // Trigger a response
        var responseEvent = new ResponseCreateEvent();
        await SendToOpenAIAsync(responseEvent, cancellationToken);

        _logger.LogInformation("Text message sent and response triggered");
    }

    public async Task CommitAudioAsync(CancellationToken cancellationToken = default)
    {
        if (_openAIWebSocket?.State != WebSocketState.Open)
        {
            return;
        }

        var commitEvent = new InputAudioBufferCommitEvent();
        await SendToOpenAIAsync(commitEvent, cancellationToken);
    }

    public async Task CreateResponseAsync(CancellationToken cancellationToken = default)
    {
        if (_openAIWebSocket?.State != WebSocketState.Open)
        {
            return;
        }

        var responseEvent = new ResponseCreateEvent();
        await SendToOpenAIAsync(responseEvent, cancellationToken);
    }

    public async Task CancelResponseAsync(CancellationToken cancellationToken = default)
    {
        if (_openAIWebSocket?.State != WebSocketState.Open)
        {
            return;
        }

        var cancelEvent = new ResponseCancelEvent();
        await SendToOpenAIAsync(cancelEvent, cancellationToken);
    }

    private async Task SendToOpenAIAsync<T>(T eventData, CancellationToken cancellationToken) where T : class
    {
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            if (_openAIWebSocket?.State != WebSocketState.Open)
            {
                return;
            }

            var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Sending to OpenAI: {Json}", json.Length > 200 ? json.Substring(0, 200) + "..." : json);

            var bytes = Encoding.UTF8.GetBytes(json);
            await _openAIWebSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 64]; // 64KB buffer
        var messageBuilder = new StringBuilder();

        try
        {
            while (_openAIWebSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await _openAIWebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("OpenAI WebSocket closed");
                    await NotifyStatus("Disconnected from OpenAI");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuilder.Append(message);

                if (result.EndOfMessage)
                {
                    var completeMessage = messageBuilder.ToString();
                    messageBuilder.Clear();

                    await ProcessOpenAIMessageAsync(completeMessage);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Receive operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages from OpenAI");
            await NotifyError($"Receive error: {ex.Message}");
        }
    }

    private async Task ProcessOpenAIMessageAsync(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeElement))
            {
                return;
            }

            var eventType = typeElement.GetString();

            switch (eventType)
            {
                case "session.created":
                case "session.updated":
                    await NotifyStatus("Session ready");
                    break;

                case "input_audio_buffer.speech_started":
                    await NotifyStatus("User speaking...");
                    
                    // Cancel any ongoing response (barge-in / interruption)
                    if (_isResponseActive)
                    {
                        _logger.LogInformation("User interrupted - cancelling active response");
                        try
                        {
                            var cancelEvent = new ResponseCancelEvent();
                            await SendToOpenAIAsync(cancelEvent, CancellationToken.None);
                            _logger.LogInformation("Response cancelled successfully");
                            
                            // Notify client to stop audio playback and clear queue
                            await NotifyTranscript("system", "__CANCEL_AUDIO__");
                            
                            _isResponseActive = false;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error cancelling response");
                        }
                    }
                    else
                    {
                        _logger.LogDebug("User started speaking (no active response to cancel)");
                    }
                    break;

                case "input_audio_buffer.speech_stopped":
                    await NotifyStatus("Processing...");
                    break;

                case "input_audio_buffer.committed":
                    break;

                case "conversation.item.created":
                    // Check if this item contains a transcript (for user input)
                    if (root.TryGetProperty("item", out var item))
                    {
                        var itemRole = item.TryGetProperty("role", out var roleVal) ? roleVal.GetString() : null;
                        
                        // For user messages, check if there's a transcript
                        if (itemRole == "user" && item.TryGetProperty("content", out var content))
                        {
                            foreach (var contentItem in content.EnumerateArray())
                            {
                                if (contentItem.TryGetProperty("transcript", out var transcript))
                                {
                                    var transcriptText = transcript.GetString();
                                    if (!string.IsNullOrEmpty(transcriptText))
                                    {
                                        await NotifyTranscript("user", transcriptText);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case "conversation.item.input_audio_transcription.completed":
                    if (root.TryGetProperty("transcript", out var userTranscript))
                    {
                        var transcriptText = userTranscript.GetString();
                        if (!string.IsNullOrEmpty(transcriptText))
                        {
                            await NotifyTranscript("user", transcriptText);
                        }
                    }
                    break;

                case "conversation.item.input_audio_transcription.failed":
                    if (root.TryGetProperty("error", out var transcriptError))
                    {
                        var errorMessage = transcriptError.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                        _logger.LogError("User audio transcription failed: {Error}", errorMessage);
                    }
                    break;

                case "response.created":
                    // Mark response as active as soon as it's created (before audio arrives)
                    _isResponseActive = true;
                    _logger.LogInformation("Response created - now interruptible");
                    break;

                case "response.audio.delta":
                    if (root.TryGetProperty("delta", out var audioDelta))
                    {
                        var audioData = audioDelta.GetString();
                        if (!string.IsNullOrEmpty(audioData))
                        {
                            await NotifyAudio(audioData);
                        }
                    }
                    break;

                case "response.audio_transcript.delta":
                    if (root.TryGetProperty("delta", out var transcriptDelta))
                    {
                        var delta = transcriptDelta.GetString();
                        if (!string.IsNullOrEmpty(delta))
                        {
                            await NotifyTranscript("assistant", delta);
                        }
                    }
                    break;

                case "response.audio_transcript.done":
                    break;

                case "response.function_call_arguments.done":
                    await HandleFunctionCallAsync(root);
                    break;

                case "response.done":
                    _isResponseActive = false;
                    _logger.LogDebug("Response completed");
                    
                    // Log token usage
                    if (root.TryGetProperty("response", out var response) && 
                        response.TryGetProperty("usage", out var usage))
                    {
                        var totalTokens = usage.TryGetProperty("total_tokens", out var total) ? total.GetInt32() : 0;
                        var inputTokens = usage.TryGetProperty("input_tokens", out var input) ? input.GetInt32() : 0;
                        var outputTokens = usage.TryGetProperty("output_tokens", out var output) ? output.GetInt32() : 0;
                        
                        _logger.LogInformation("📊 Token usage - Total: {Total} | Input: {Input} | Output: {Output}", 
                            totalTokens, inputTokens, outputTokens);
                        
                        // Log detailed token breakdown if available
                        if (usage.TryGetProperty("input_token_details", out var inputDetails))
                        {
                            var textTokens = inputDetails.TryGetProperty("text_tokens", out var text) ? text.GetInt32() : 0;
                            var audioTokens = inputDetails.TryGetProperty("audio_tokens", out var audio) ? audio.GetInt32() : 0;
                            var cachedTokens = inputDetails.TryGetProperty("cached_tokens", out var cached) ? cached.GetInt32() : 0;
                            
                            if (textTokens > 0 || audioTokens > 0 || cachedTokens > 0)
                            {
                                _logger.LogInformation("  ↳ Input details - Text: {Text} | Audio: {Audio} | Cached: {Cached}", 
                                    textTokens, audioTokens, cachedTokens);
                            }
                        }
                        
                        if (usage.TryGetProperty("output_token_details", out var outputDetails))
                        {
                            var textTokens = outputDetails.TryGetProperty("text_tokens", out var text) ? text.GetInt32() : 0;
                            var audioTokens = outputDetails.TryGetProperty("audio_tokens", out var audio) ? audio.GetInt32() : 0;
                            
                            if (textTokens > 0 || audioTokens > 0)
                            {
                                _logger.LogInformation("  ↳ Output details - Text: {Text} | Audio: {Audio}", 
                                    textTokens, audioTokens);
                            }
                        }
                    }
                    
                    await NotifyStatus("Ready");
                    await NotifyResponseComplete();
                    break;

                case "error":
                    if (root.TryGetProperty("error", out var error))
                    {
                        var errorMessage = error.GetProperty("message").GetString() ?? "Unknown error";
                        // Don't log "no active response" errors as they're expected during interruption
                        if (!errorMessage.Contains("no active response"))
                        {
                            _logger.LogError("OpenAI error: {Error}", errorMessage);
                            await NotifyError(errorMessage);
                        }
                        else
                        {
                            _logger.LogDebug("Cancellation timing issue (response already completed): {Error}", errorMessage);
                        }
                    }
                    break;

                default:
                    // Silently ignore unhandled events
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OpenAI message: {Message}", message);
        }
    }

    private async Task NotifyAudio(string base64Audio)
    {
        if (OnAudioReceived != null)
        {
            await OnAudioReceived(base64Audio);
        }
    }

    private async Task NotifyTranscript(string role, string transcript)
    {
        if (OnTranscriptReceived != null)
        {
            await OnTranscriptReceived(role, transcript);
        }
    }

    private async Task NotifyError(string error)
    {
        if (OnError != null)
        {
            await OnError(error);
        }
    }

    private async Task NotifyStatus(string status)
    {
        if (OnStatusChanged != null)
        {
            await OnStatusChanged(status);
        }
    }

    private async Task NotifyResponseComplete()
    {
        if (OnTranscriptReceived != null)
        {
            await OnTranscriptReceived("system", "__RESPONSE_DONE__");
        }
    }

    private async Task HandleFunctionCallAsync(JsonElement root)
    {
        try
        {
            // Extract function call details
            var callId = root.TryGetProperty("call_id", out var cid) ? cid.GetString() : null;
            var name = root.TryGetProperty("name", out var n) ? n.GetString() : null;
            var argumentsString = root.TryGetProperty("arguments", out var args) ? args.GetString() : "{}";

            if (string.IsNullOrEmpty(callId) || string.IsNullOrEmpty(name))
            {
                _logger.LogWarning("Invalid function call event: missing call_id or name");
                return;
            }

            _logger.LogInformation("Function call requested: {FunctionName} with call_id: {CallId}", name, callId);
            await NotifyStatus($"Executing tool: {name}...");

            try
            {
                // Parse arguments
                var argumentsJson = JsonDocument.Parse(argumentsString).RootElement;

                // Execute the tool
                var result = await _toolExecutor.ExecuteAsync(name, argumentsJson);

                // Serialize result
                var resultJson = JsonSerializer.Serialize(result);
                _logger.LogInformation("Tool {FunctionName} executed successfully. Result: {Result}", 
                    name, resultJson.Length > 200 ? resultJson.Substring(0, 200) + "..." : resultJson);

                // Send function call output back to OpenAI
                var outputEvent = new ConversationItemCreateEvent
                {
                    Item = new FunctionCallOutputItem
                    {
                        Type = "function_call_output",
                        CallId = callId,
                        Output = resultJson
                    }
                };

                await SendToOpenAIAsync(outputEvent, CancellationToken.None);

                // Trigger a new response to continue the conversation
                var responseEvent = new ResponseCreateEvent();
                await SendToOpenAIAsync(responseEvent, CancellationToken.None);

                _logger.LogInformation("Function call result sent to OpenAI, response triggered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool: {FunctionName}", name);
                
                // Send error back to OpenAI
                var errorOutput = new
                {
                    error = true,
                    message = ex.Message,
                    type = ex.GetType().Name
                };

                var outputEvent = new ConversationItemCreateEvent
                {
                    Item = new FunctionCallOutputItem
                    {
                        Type = "function_call_output",
                        CallId = callId,
                        Output = JsonSerializer.Serialize(errorOutput)
                    }
                };

                await SendToOpenAIAsync(outputEvent, CancellationToken.None);
                await SendToOpenAIAsync(new ResponseCreateEvent(), CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling function call");
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _receiveCts?.Cancel();

            if (_openAIWebSocket?.State == WebSocketState.Open)
            {
                await _openAIWebSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client disconnecting",
                    CancellationToken.None);
            }

            _logger.LogInformation("Disconnected from OpenAI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from OpenAI");
        }
    }

    public void Dispose()
    {
        _receiveCts?.Cancel();
        _receiveCts?.Dispose();
        _openAIWebSocket?.Dispose();
        _sendLock?.Dispose();
    }
}
