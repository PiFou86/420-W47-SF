using System.Text.Json;

namespace mcpservertest01.Handlers;

/// <summary>
/// Gestionnaire pour la méthode ping
/// Permet de vérifier que le serveur est actif
/// </summary>
public class PingHandler : IMethodHandler
{
    public string[] SupportedMethods => new[] { "ping" };

    public Task<object?> HandleAsync(string method, JsonElement parameters, int requestId)
    {
        // La méthode ping retourne simplement un objet vide pour confirmer que le serveur répond
        return Task.FromResult<object?>(new { });
    }
}
