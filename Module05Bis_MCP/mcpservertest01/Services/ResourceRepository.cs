using mcpservertest01.Models.Mcp;

namespace mcpservertest01.Services;

/// <summary>
/// Repository pour gérer les ressources MCP
/// </summary>
public class ResourceRepository : IResourceRepository
{
    public Resource[] GetAllResources()
    {
        return new[]
        {
            new Resource
            {
                uri = "file:///couleurs.md",
                name = "couleurs.md",
                title = "Couleurs",
                description = "Un fichier de documentation sur les couleurs.",
                mimeType = "text/markdown"
            }
        };
    }

    public ResourceContent ReadResource(string uri)
    {
        return uri switch
        {
            "file:///couleurs.md" => new ResourceContent
            {
                uri = uri,
                name = "couleurs.md",
                title = "Couleurs",
                mimeType = "text/markdown",
                text = "# Couleurs\n\n- Rouge\n- Vert\n- Bleu\n"
            },
            _ => new ResourceContent
            {
                uri = uri,
                name = "unknown",
                title = "Unknown Resource",
                mimeType = "text/plain",
                text = "Resource not found."
            }
        };
    }
}
