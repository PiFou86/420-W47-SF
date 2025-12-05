using System.Text.Json.Serialization;

namespace mcpservertest01.Models.Mcp.Responses;

/// <summary>
/// Réponse à resources/list
/// </summary>
public class ResourceListResponse
{
    public Resource[] resources { get; set; } = Array.Empty<Resource>();
}

/// <summary>
/// Réponse à resources/read
/// </summary>
public class ResourceReadResponse
{
    public ResourceContent[] contents { get; set; } = Array.Empty<ResourceContent>();
}

/// <summary>
/// Réponse à resources/templates/list
/// </summary>
public class ResourceTemplateListResponse
{
    public object[] resourceTemplates { get; set; } = Array.Empty<object>();
}

/// <summary>
/// Réponse à tools/list
/// </summary>
public class ToolListResponse
{
    public Tool[] tools { get; set; } = Array.Empty<Tool>();
}

/// <summary>
/// Réponse à tools/call
/// </summary>
public class ToolCallResponse
{
    public ToolCallContent[] content { get; set; } = Array.Empty<ToolCallContent>();
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? structuredContent { get; set; }
}

/// <summary>
/// Réponse à prompts/list
/// </summary>
public class PromptListResponse
{
    public Prompt[] prompts { get; set; } = Array.Empty<Prompt>();
}

/// <summary>
/// Réponse à prompts/get
/// </summary>
public class PromptGetResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? description { get; set; }
    
    public PromptMessage[] messages { get; set; } = Array.Empty<PromptMessage>();
}
