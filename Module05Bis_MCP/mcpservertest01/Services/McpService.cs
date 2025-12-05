using System.Text.Json;
using mcpservertest01.Configuration;
using mcpservertest01.Extensions;
using mcpservertest01.Handlers;
using mcpservertest01.Models.JsonRpc;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace mcpservertest01.Services;

/// <summary>
/// Service principal pour gérer les requêtes MCP
/// </summary>
public class McpService : IMcpService
{
    private readonly IEnumerable<IMethodHandler> _handlers;
    private readonly McpOptions _options;
    private readonly ISerializer _yamlSerializer;
    private readonly ILogger<McpService> _logger;
    private readonly Dictionary<string, IMethodHandler> _handlerCache;

    public McpService(
        IEnumerable<IMethodHandler> handlers,
        IOptions<McpOptions> options,
        ILogger<McpService> logger)
    {
        _handlers = handlers;
        _options = options.Value;
        _logger = logger;

        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Cache pour mapper méthode -> handler
        _handlerCache = new Dictionary<string, IMethodHandler>();
        foreach (IMethodHandler handler in _handlers)
        {
            foreach (string method in handler.SupportedMethods)
            {
                _handlerCache[method] = handler;
            }
        }
    }

    public async Task<JsonRpcResponse> ProcessRequestAsync(string requestBody)
    {
        try
        {
            // Log de la requête
            LogRequest(requestBody);

            // Parsing de la requête
            JsonRpcRequest? request = JsonSerializer.Deserialize<JsonRpcRequest>(requestBody);

            if (request == null)
            {
                return CreateErrorResponse(
                    JsonRpcErrorCodes.InvalidRequest,
                    "Invalid Request",
                    0);
            }

            // Validation JSON-RPC
            if (string.IsNullOrEmpty(request.jsonrpc) || request.jsonrpc != "2.0")
            {
                return CreateErrorResponse(
                    JsonRpcErrorCodes.InvalidRequest,
                    "Invalid JSON-RPC version",
                    request.id);
            }

            if (string.IsNullOrEmpty(request.method))
            {
                return CreateErrorResponse(
                    JsonRpcErrorCodes.InvalidRequest,
                    "Missing method",
                    request.id);
            }

            // Traitement des notifications (pas de réponse attendue)
            if (request.method.StartsWith("notifications/"))
            {
                _logger.LogInformation("Received notification: {Method}", request.method);
                return new JsonRpcResponse { id = request.id };
            }

            _logger.LogInformation("Processing method: {Method}", request.method);

            // Recherche du handler approprié
            if (!_handlerCache.TryGetValue(request.method, out IMethodHandler? handler))
            {
                return CreateErrorResponse(
                    JsonRpcErrorCodes.MethodNotFound,
                    $"Unknown method: {request.method}",
                    request.id);
            }

            // Exécution du handler
            try
            {
                object? result = await handler.HandleAsync(request.method, request.@params, request.id);
                JsonRpcResponse response = CreateSuccessResponse(result, request.id);
                LogResponse(response);
                return response;
            }
            catch (ArgumentException ex)
            {
                return CreateErrorResponse(
                    JsonRpcErrorCodes.InvalidParams,
                    ex.Message,
                    request.id);
            }
            catch (InvalidOperationException ex)
            {
                return CreateErrorResponse(
                    JsonRpcErrorCodes.InternalError,
                    ex.Message,
                    request.id);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error");
            return CreateErrorResponse(
                JsonRpcErrorCodes.ParseError,
                "Parse error",
                0,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return CreateErrorResponse(
                JsonRpcErrorCodes.InternalError,
                "Internal error",
                0,
                ex.Message);
        }
    }

    private JsonRpcResponse CreateSuccessResponse(object? result, int requestId)
    {
        return new JsonRpcResponse
        {
            result = result,
            id = requestId
        };
    }

    private JsonRpcResponse CreateErrorResponse(int errorCode, string message, int requestId, object? data = null)
    {
        return new JsonRpcResponse
        {
            error = new JsonRpcError
            {
                code = errorCode,
                message = message,
                data = data
            },
            id = requestId
        };
    }

    private void LogRequest(string body)
    {
        if (!_options.EnableDebugOutput) return;

        try
        {
            using JsonDocument jsonBody = JsonDocument.Parse(body);
            object? obj = jsonBody.RootElement.ToObject();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("========================================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("+--------------------- MCP Request ---------------------+");
            Console.Write(SerializeForDebug(obj));
            Console.WriteLine("+-------------------------------------------------------+");
            Console.WriteLine();
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log request");
        }
    }

    private void LogResponse(JsonRpcResponse response)
    {
        if (!_options.EnableDebugOutput) return;

        try
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("+--------------------- MCP Response --------------------+");
            Console.Write(SerializeForDebug(response));
            Console.WriteLine("+-------------------------------------------------------+");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("========================================================");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log response");
        }
    }

    private string SerializeForDebug(object? obj)
    {
        return _options.Format switch
        {
            "json" => JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }),
            "yaml" => _yamlSerializer.Serialize(obj ?? new object()),
            _ => "Invalid serializer format"
        };
    }
}
