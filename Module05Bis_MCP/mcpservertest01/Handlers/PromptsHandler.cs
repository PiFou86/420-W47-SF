using System.Text.Json;
using mcpservertest01.Extensions;
using mcpservertest01.Models.Mcp;
using mcpservertest01.Models.Mcp.Responses;

namespace mcpservertest01.Handlers;

/// <summary>
/// Gestionnaire pour les méthodes prompts/*
/// </summary>
public class PromptsHandler : IMethodHandler
{
    public string[] SupportedMethods => new[] { "prompts/list", "prompts/get" };

    public Task<object?> HandleAsync(string method, JsonElement parameters, int requestId)
    {
        return method switch
        {
            "prompts/list" => HandlePromptsListAsync(),
            "prompts/get" => HandlePromptsGetAsync(parameters),
            _ => throw new InvalidOperationException($"Unsupported method: {method}")
        };
    }

    private Task<object?> HandlePromptsListAsync()
    {
        PromptListResponse response = new PromptListResponse
        {
            prompts = new[]
            {
                new Prompt
                {
                    name = "creer_fichier_avec_contenu",
                    title = "Créer un fichier avec du contenu",
                    description = "Demande au LLM de créer un fichier avec un contenu spécifique.",
                    arguments = new[]
                    {
                        new PromptArgument { name = "fileName", required = true },
                        new PromptArgument { name = "path", required = true },
                        new PromptArgument { name = "content", required = true }
                    }
                }
            }
        };

        return Task.FromResult<object?>(response);
    }

    private Task<object?> HandlePromptsGetAsync(JsonElement parameters)
    {
        string promptName = parameters.GetPropertyString("name") ?? "";

        if (promptName != "creer_fichier_avec_contenu")
        {
            throw new InvalidOperationException($"Unknown prompt: {promptName}");
        }

        if (!parameters.TryGetProperty("arguments", out JsonElement args))
        {
            throw new ArgumentException("Missing 'arguments' parameter");
        }

        string fileName = args.GetPropertyString("fileName") ?? "nouveau_fichier.txt";
        string path = args.GetPropertyString("path") ?? "/documents";
        string content = args.GetPropertyString("content") ?? "Contenu par défaut";

        PromptGetResponse response = new PromptGetResponse
        {
            description = "Demande au LLM de créer un fichier avec un contenu spécifique.",
            messages = new[]
            {
                new PromptMessage
                {
                    role = "user",
                    content = new PromptContent
                    {
                        type = "text",
                        text = $"Crée un fichier nommé '{fileName}' dans le répertoire '{path}' avec le contenu suivant :\n\n{content}"
                    }
                }
            }
        };

        return Task.FromResult<object?>(response);
    }
}
