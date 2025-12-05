using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();

MCPDebugOptions debugOptions = builder.Configuration.GetSection("DebugMCP").Get<MCPDebugOptions>() ?? new MCPDebugOptions();

ISerializer serializer = new SerializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

app.MapPost("/mcp", async (HttpContext httpContext) =>
{
    using StreamReader reader = new StreamReader(httpContext.Request.Body);
    string body = await reader.ReadToEndAsync();

    DisplayRequest(body);

    JSONRPCRequest? request = JsonSerializer.Deserialize<JSONRPCRequest>(body);
    if (request == null)
    {
        return Results.BadRequest("Invalid JSON-RPC request.");
    }

    object? response = null;
#if DEBUG
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.Out.WriteLine($"Processing method: {request.method}");
    Console.ResetColor();
#endif
    switch (request.method)
    {
        case "initialize":
            response = MCPDemoInitialize();
            break;
        case "ping":
            response = new object();
            break;
        case "resources/list":
            response = new
            {
                resources = new[]
                {
                    new
                    {
                        uri = "file:///couleurs.md",
                        name = "couleurs.md",
                        title = "Couleurs",
                        description = "Un fichier de documentation sur les couleurs.",
                        mimeType = "text/markdown"
                    }
                }
            };
            break;
        case "resources/read":
            string uri = request.@params.GetProperty("uri").GetString() ?? "";
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Out.WriteLine($"Reading resource with params: {uri}");
            Console.ResetColor();
#endif
            response = new
            {
                contents = MCPDemoReadResource(uri)
            };
            break;
        case "notifications/initialized":
            // Just acknowledge, no response needed
            response = null;
            break;
        case "resources/templates/list":
            response = new
            {
                resourceTemplates = new object[] { }
            };
            break;
        case "tools/list":
            response = new
            {
                tools = new[] {
                    new {
                        name = "use_my_super_operation",
                        title = "Calcul entre deux nombres",
                        description = "Calcul super complexe entre deux nombres",
                        inputSchema = new {
                            type = "object",
                            properties = new {
                                valeur1 = new {
                                    type = "integer",
                                    description = "Premier nombre"
                                },
                                valeur2 = new {
                                    type = "integer",
                                    description = "Deuxième nombre"
                                }
                            }
                        },
                        outputSchema = new {
                            type = "object",
                            properties = new {
                                result = new {
                                    type = "integer",
                                    description = "Le résultat du calcul"
                                }
                            }
                        }
                    }
                }
            };
            break;
        case "prompts/list":
            response = new
            {
                prompts = new object[] {
                    new {
                        name = "creer_fichier_avec_contenu",
                        title = "Créer un fichier avec du contenu",
                        description = "Demande au LLM de créer un fichier avec un contenu spécifique.",
                        arguments = new [] {
                            new {
                                name = "fileName",
                                required = true,
                            },
                            new {
                                name = "path",
                                required = true,
                            },
                            new {
                                name = "content",
                                required = true,
                            }
                        }
                    }
                }
            };
            break;
        case "prompts/get":
            string nomPrompt = request.@params.GetProperty("name").GetString() ?? "";
            switch (nomPrompt)
            {
                case "creer_fichier_avec_contenu":
                    string fileName = request.@params.GetProperty("arguments").GetProperty("fileName").GetString() ?? "nouveau_fichier.txt";
                    string path = request.@params.GetProperty("arguments").GetProperty("path").GetString() ?? "/documents";
                    string content = request.@params.GetProperty("arguments").GetProperty("content").GetString() ?? "Contenu par défaut";
                    response = new
                    {

                        description = "Demande au LLM de créer un fichier avec un contenu spécifique.",
                        messages = new object[] {
                                new {
                                    role = "user",
                                    content = new {
                                        type = "text",
                                        text = $"Crée un fichier nommé '{fileName}' dans le répertoire '{path}' avec le contenu suivant :\n\n{content}"
                                    }
                                }
                            }

                    };
                    break;
                default:
                    return Results.BadRequest($"Unknown prompt: {request.@params.GetProperty("name").GetString()}");
            }

            break;
        case "tools/call":
            {
                string toolName = request.@params.GetProperty("name").GetString() ?? "";
                JsonElement toolInput = request.@params.GetProperty("arguments");
#if DEBUG
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Out.WriteLine($"Tool call: {toolName} with input: {toolInput}");
                Console.ResetColor();
#endif

                if (toolName == "use_my_super_operation")
                {
                    int valeur1 = toolInput.GetProperty("valeur1").GetInt32();
                    int valeur2 = toolInput.GetProperty("valeur2").GetInt32();
                    int result = valeur1 + valeur2;

                    response = new
                    {
                        content = new object[] {
                            new
                            {
                                type = "text",
                                text = JsonSerializer.Serialize(new
                                {
                                    result = result
                                })
                            }
                        },
                        structuredContent = new
                        {
                            result = result
                        }
                    };
                }
                else
                {
                    return Results.BadRequest($"Unknown tool: {toolName}");
                }
            }
            break;
        default:
            return Results.BadRequest($"Unknown method: {request.method}");
    }

    var rpcResponse = new
    {
        jsonrpc = "2.0",
        result = response,
        id = request.id
    };

    DisplayResponse(rpcResponse);

    return Results.Ok(rpcResponse);
});

app.Run();

object MCPDemoReadResource(string uri)
{
    switch (uri)
    {
        case "file:///couleurs.md":
            return new[] {
                new {
                    uri = uri,
                    name = "couleurs.md",
                    title = "Couleurs",
                    mimeType = "text/markdown",
                    text = "# Couleurs\n\n- Rouge\n- Vert\n- Bleu\n"
                }
            };
        default:
            return new
            {
                uri = uri,
                name = "unknown",
                title = "Unknown Resource",
                mimeType = "text/plain",
                text = "Resource not found."
            };
    }
}

object MCPDemoInitialize()
{
    // Construction d'un vrai objet pour sérialisation JSON
    var response = new
    {
        protocolVersion = "2025-06-18",
        capabilities = new
        {
            tools = new { subscribe = false },
            resources = new { listChanged = false },
            prompts = new { listChanged = false },
        },
        serverInfo = new
        {
            name = "mon-mcp-server",
            version = "1.0.0"
        }
    };
    return response;
}

// PFL : From AI
object? FromElement(JsonElement el)
{
    switch (el.ValueKind)
    {
        case JsonValueKind.Object:
            var dict = new Dictionary<string, object?>();
            foreach (var p in el.EnumerateObject())
            {
                dict[p.Name] = FromElement(p.Value);
            }
            return dict;

        case JsonValueKind.Array:
            var list = new List<object?>();
            foreach (var item in el.EnumerateArray())
            {
                list.Add(FromElement(item));
            }
            return list;

        case JsonValueKind.String:
            return el.GetString();
        case JsonValueKind.Number:
            // essaie int sinon double
            if (el.TryGetInt64(out long l)) return l;
            if (el.TryGetDouble(out double d)) return d;
            return el.GetRawText();
        case JsonValueKind.True:
            return true;
        case JsonValueKind.False:
            return false;
        case JsonValueKind.Null:
        case JsonValueKind.Undefined:
        default:
            return null;
    }
}

string debugSerialize(object obj)
{
    string res = "bad serializer name";
    switch (debugOptions.Format)
    {
        case "json":
            res = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            break;
        case "yaml":
            res = serializer.Serialize(obj);
            break;
    }
    return res;
}

void DisplayRequest(string body)
{
    if (debugOptions.EnableDebugOutput)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Out.WriteLine("========================================================");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Out.WriteLine("+--------------------- MCP Request ---------------------+");
        using var jsonBody = JsonDocument.Parse(body);
        Console.Out.Write(debugSerialize(FromElement(jsonBody.RootElement)));
        Console.Out.WriteLine("+-------------------------------------------------------+");
        Console.Out.WriteLine();
        Console.ResetColor();
    }
}

void DisplayResponse(object response)
{
    if (debugOptions.EnableDebugOutput)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Out.WriteLine("+--------------------- MCP Response --------------------+");
        Console.Out.Write(debugSerialize(response));
        Console.Out.WriteLine("+-------------------------------------------------------+");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Out.WriteLine("========================================================");
        Console.ResetColor();
        Console.Out.WriteLine();
        Console.Out.WriteLine();
    }
}

public class JSONRPCRequest
{
    public string jsonrpc { get; set; }
    public string method { get; set; }
    public JsonElement @params { get; set; }
    public int id { get; set; }
}

public record MCPDebugOptions
{
    public bool EnableDebugOutput { get; init; } = false;
    public string Format { get; init; } = "yaml";
}

