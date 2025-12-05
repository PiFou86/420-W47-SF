using System.Text.Json;

namespace mcpservertest01.Handlers;

/// <summary>
/// Interface pour les gestionnaires de méthodes MCP
/// </summary>
public interface IMethodHandler
{
    /// <summary>
    /// Méthode(s) supportée(s) par ce handler
    /// </summary>
    string[] SupportedMethods { get; }

    /// <summary>
    /// Gère l'exécution d'une méthode
    /// </summary>
    Task<object?> HandleAsync(string method, JsonElement parameters, int requestId);
}
