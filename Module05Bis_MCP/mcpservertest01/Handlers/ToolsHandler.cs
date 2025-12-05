using System.Diagnostics;
using System.Text.Json;
using mcpservertest01.Extensions;
using mcpservertest01.Models.Mcp;
using mcpservertest01.Models.Mcp.Responses;
using mcpservertest01.Services;

namespace mcpservertest01.Handlers;

/// <summary>
/// Gestionnaire pour les méthodes tools/*
/// </summary>
public class ToolsHandler : IMethodHandler
{
    public string[] SupportedMethods => new[] { "tools/list", "tools/call" };

    public Task<object?> HandleAsync(string method, JsonElement parameters, int requestId)
    {
        return method switch
        {
            "tools/list" => HandleToolsListAsync(),
            "tools/call" => HandleToolsCallAsync(parameters),
            _ => throw new InvalidOperationException($"Unsupported method: {method}")
        };
    }

    private Task<object?> HandleToolsListAsync()
    {
        ToolListResponse response = new ToolListResponse
        {
            tools = new[]
            {
                DescribeUseMySuperOperationTool()
            }
        };

        return Task.FromResult<object?>(response);
    }

    private Task<object?> HandleToolsCallAsync(JsonElement parameters)
    {
        string toolName = parameters.GetPropertyString("name") ?? "";

        if (!parameters.TryGetProperty("arguments", out JsonElement toolInput))
        {
            throw new ArgumentException("Missing 'arguments' parameter");
        }

        return toolName switch
        {
            "use_my_super_operation" => HandleUseMySuperOperationAsync(toolInput),
            _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
        };
    }

    private Tool DescribeUseMySuperOperationTool()
    {
        return new Tool
        {
            name = "use_my_super_operation",
            title = "Calcul entre deux nombres",
            description = "Calcul super complexe entre deux nombres",
            inputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["valeur1"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "Premier nombre"
                    },
                    ["valeur2"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "Deuxième nombre"
                    }
                },
                ["required"] = new[] { "valeur1", "valeur2" }
            },
            outputSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["result"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "Le résultat du calcul"
                    }
                },
                ["required"] = new[] { "result" }
            }
        };
    }

    private async Task<object?> HandleUseMySuperOperationAsync(JsonElement toolInput)
    {
        int valeur1 = toolInput.GetPropertyInt32("valeur1");
        int valeur2 = toolInput.GetPropertyInt32("valeur2");
        int result = valeur1 + valeur2;

        ToolCallResponse response = new ToolCallResponse
        {
            content = new[]
            {
                new ToolCallContent
                {
                    type = "text",
                    text = JsonSerializer.Serialize(new { result })
                }
            },
            structuredContent = new { result }
        };

        return await Task.FromResult<object?>(response);
    }
}
