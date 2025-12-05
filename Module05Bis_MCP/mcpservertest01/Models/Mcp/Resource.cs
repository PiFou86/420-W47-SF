using System.Text.Json.Serialization;

namespace mcpservertest01.Models.Mcp;

/// <summary>
/// Représente une ressource MCP
/// </summary>
public class Resource
{
    public required string uri { get; set; }
    public required string name { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? title { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? description { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? mimeType { get; set; }
}

/// <summary>
/// Contenu d'une ressource MCP
/// </summary>
public class ResourceContent
{
    public required string uri { get; set; }
    public required string name { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? title { get; set; }
    
    public required string mimeType { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? text { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? blob { get; set; }
}
