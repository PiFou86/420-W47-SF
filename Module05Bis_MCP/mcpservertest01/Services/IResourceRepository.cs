using mcpservertest01.Models.Mcp;

namespace mcpservertest01.Services;

/// <summary>
/// Interface pour le repository des ressources
/// </summary>
public interface IResourceRepository
{
    /// <summary>
    /// Récupère toutes les ressources disponibles
    /// </summary>
    Resource[] GetAllResources();

    /// <summary>
    /// Lit le contenu d'une ressource
    /// </summary>
    ResourceContent ReadResource(string uri);
}
