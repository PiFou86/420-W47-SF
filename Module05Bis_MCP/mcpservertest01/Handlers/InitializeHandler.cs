using System.Text.Json;
using mcpservertest01.Models.Mcp;

namespace mcpservertest01.Handlers;

/// <summary>
/// Gestionnaire pour la méthode initialize
/// </summary>
public class InitializeHandler : IMethodHandler
{
    public string[] SupportedMethods => new[] { "initialize" };

    public Task<object?> HandleAsync(string method, JsonElement parameters, int requestId)
    {
        InitializeResponse response = new InitializeResponse
        {
            protocolVersion = "2025-06-18",
            capabilities = new Capabilities
            {
                tools = new ToolsCapability { subscribe = false },
                resources = new ResourcesCapability { listChanged = false },
                prompts = new PromptsCapability { listChanged = false }
            },
            serverInfo = new ServerInfo
            {
                name = "mon-mcp-server",
                version = "1.0.0"
            }
        };

        return Task.FromResult<object?>(response);
    }
}
