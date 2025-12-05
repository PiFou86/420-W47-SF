using System.Text.Json.Serialization;

namespace mcpservertest01.Models.Mcp;

/// <summary>
/// Représente un outil MCP
/// </summary>
public class Tool
{
    public required string name { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? title { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? description { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? inputSchema { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? outputSchema { get; set; }
}

/// <summary>
/// Contenu de la réponse d'un appel d'outil
/// </summary>
public class ToolCallContent
{
    public required string type { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? text { get; set; }
}
