using mcpservertest01.Models.JsonRpc;

namespace mcpservertest01.Services;

/// <summary>
/// Interface pour le service MCP principal
/// </summary>
public interface IMcpService
{
    /// <summary>
    /// Traite une requête JSON-RPC
    /// </summary>
    Task<JsonRpcResponse> ProcessRequestAsync(string requestBody);
}
