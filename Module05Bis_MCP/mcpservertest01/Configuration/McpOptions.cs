namespace mcpservertest01.Configuration;

/// <summary>
/// Options de configuration pour le serveur MCP
/// </summary>
public record McpOptions
{
    public bool EnableDebugOutput { get; init; } = false;
    public string Format { get; init; } = "yaml";
}
