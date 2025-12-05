namespace mcpservertest01.Models.JsonRpc;

/// <summary>
/// Représente une erreur JSON-RPC 2.0
/// </summary>
public class JsonRpcError
{
    public int code { get; set; }
    public string message { get; set; } = string.Empty;
    public object? data { get; set; }
}

/// <summary>
/// Codes d'erreur JSON-RPC standardisés et personnalisés
/// </summary>
public static class JsonRpcErrorCodes
{
    // Codes d'erreur standard JSON-RPC 2.0
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;

    // Codes d'erreur spécifiques au serveur MCP (-32000 à -32099)
    public const int ResourceNotFound = -32001;
    public const int ToolNotFound = -32002;
    public const int PromptNotFound = -32003;
}
