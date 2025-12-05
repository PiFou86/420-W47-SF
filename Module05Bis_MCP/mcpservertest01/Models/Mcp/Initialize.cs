using System.Text.Json.Serialization;

namespace mcpservertest01.Models.Mcp;

/// <summary>
/// Réponse à la méthode initialize
/// </summary>
public class InitializeResponse
{
    public required string protocolVersion { get; set; }
    public required Capabilities capabilities { get; set; }
    public required ServerInfo serverInfo { get; set; }
}

/// <summary>
/// Capacités du serveur MCP
/// </summary>
public class Capabilities
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolsCapability? tools { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResourcesCapability? resources { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PromptsCapability? prompts { get; set; }
}

/// <summary>
/// Capacités des outils
/// </summary>
public class ToolsCapability
{
    public bool subscribe { get; set; }
}

/// <summary>
/// Capacités des ressources
/// </summary>
public class ResourcesCapability
{
    public bool listChanged { get; set; }
}

/// <summary>
/// Capacités des prompts
/// </summary>
public class PromptsCapability
{
    public bool listChanged { get; set; }
}

/// <summary>
/// Information sur le serveur
/// </summary>
public class ServerInfo
{
    public required string name { get; set; }
    public required string version { get; set; }
}
