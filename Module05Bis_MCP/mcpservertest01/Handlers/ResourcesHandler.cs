using System.Text.Json;
using mcpservertest01.Extensions;
using mcpservertest01.Models.Mcp;
using mcpservertest01.Models.Mcp.Responses;
using mcpservertest01.Services;

namespace mcpservertest01.Handlers;

/// <summary>
/// Gestionnaire pour les méthodes resources/*
/// </summary>
public class ResourcesHandler : IMethodHandler
{
    private readonly IResourceRepository _resourceRepository;

    public ResourcesHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public string[] SupportedMethods => new[]
    {
        "resources/list",
        "resources/read",
        "resources/templates/list"
    };

    public Task<object?> HandleAsync(string method, JsonElement parameters, int requestId)
    {
        return method switch
        {
            "resources/list" => HandleResourcesListAsync(),
            "resources/read" => HandleResourcesReadAsync(parameters),
            "resources/templates/list" => HandleResourceTemplatesListAsync(),
            _ => throw new InvalidOperationException($"Unsupported method: {method}")
        };
    }

    private Task<object?> HandleResourcesListAsync()
    {
        ResourceListResponse response = new ResourceListResponse
        {
            resources = _resourceRepository.GetAllResources()
        };
        return Task.FromResult<object?>(response);
    }

    private Task<object?> HandleResourcesReadAsync(JsonElement parameters)
    {
        string uri = parameters.GetPropertyString("uri") ?? "";

        ResourceContent? content = _resourceRepository.ReadResource(uri);
        ResourceReadResponse response = new ResourceReadResponse
        {
            contents = new[] { content }
        };

        return Task.FromResult<object?>(response);
    }

    private Task<object?> HandleResourceTemplatesListAsync()
    {
        ResourceTemplateListResponse response = new ResourceTemplateListResponse
        {
            resourceTemplates = Array.Empty<object>()
        };
        return Task.FromResult<object?>(response);
    }
}
