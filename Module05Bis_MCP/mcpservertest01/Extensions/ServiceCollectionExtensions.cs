using mcpservertest01.Configuration;
using mcpservertest01.Handlers;
using mcpservertest01.Services;

namespace mcpservertest01.Extensions;

/// <summary>
/// Extensions pour configurer les services MCP
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services MCP à la collection de services
    /// </summary>
    public static IServiceCollection AddMcpServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<McpOptions>(configuration.GetSection("DebugMCP"));

        // Services
        services.AddSingleton<IMcpService, McpService>();
        services.AddSingleton<IResourceRepository, ResourceRepository>();

        // Spotify Client (Singleton pour conserver le token)
        services.AddHttpClient();

        // Handlers
        services.AddSingleton<IMethodHandler, InitializeHandler>();
        services.AddSingleton<IMethodHandler, PingHandler>();
        services.AddSingleton<IMethodHandler, ResourcesHandler>();
        services.AddSingleton<IMethodHandler, ToolsHandler>();
        services.AddSingleton<IMethodHandler, PromptsHandler>();

        return services;
    }
}
