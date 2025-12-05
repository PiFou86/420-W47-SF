using System.Text.Json.Serialization;

namespace mcpservertest01.Models.Mcp;

/// <summary>
/// Représente un prompt MCP
/// </summary>
public class Prompt
{
    public required string name { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? title { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? description { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PromptArgument[]? arguments { get; set; }
}

/// <summary>
/// Argument d'un prompt
/// </summary>
public class PromptArgument
{
    public required string name { get; set; }
    public bool required { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? description { get; set; }
}

/// <summary>
/// Message d'un prompt
/// </summary>
public class PromptMessage
{
    public required string role { get; set; }
    public required PromptContent content { get; set; }
}

/// <summary>
/// Contenu d'un message de prompt
/// </summary>
public class PromptContent
{
    public required string type { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? text { get; set; }
}
