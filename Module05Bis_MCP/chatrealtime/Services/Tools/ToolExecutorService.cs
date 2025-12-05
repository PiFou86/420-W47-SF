using System.Text.Json;
using chatrealtime.Configuration;
using Microsoft.Extensions.Options;

namespace chatrealtime.Services.Tools;

public class ToolExecutorService : IToolExecutor
{
    private readonly ILogger<ToolExecutorService> _logger;
    private readonly OpenAISettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public ToolExecutorService(
        ILogger<ToolExecutorService> logger,
        IOptions<OpenAISettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<object> ExecuteAsync(string toolName, JsonElement arguments)
    {
        _logger.LogInformation("Executing tool: {ToolName} with arguments: {Arguments}", 
            toolName, arguments.ToString());

        // Find tool configuration
        var toolConfig = _settings.Tools?.FirstOrDefault(t => 
            t.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));

        if (toolConfig == null)
        {
            throw new ArgumentException($"Tool configuration not found: {toolName}");
        }

        // Execute based on tool type
        return toolConfig.Type.ToLowerInvariant() switch
        {
            "http" => await ExecuteHttpToolAsync(toolConfig, arguments),
            "mcp" => await ExecuteMcpToolAsync(toolConfig, arguments),
            "mcp_dynamic" => await ExecuteMcpDynamicToolAsync(toolConfig, arguments),
            "builtin" => await ExecuteBuiltinToolAsync(toolName, arguments),
            _ => throw new ArgumentException($"Unknown tool type: {toolConfig.Type}")
        };
    }

    private async Task<object> ExecuteMcpDynamicToolAsync(ToolConfig toolConfig, JsonElement arguments)
    {
        // For dynamically discovered MCP tools, we need to extract the original tool name
        // and call the MCP server's tools/call endpoint
        
        if (toolConfig.Http == null)
        {
            throw new InvalidOperationException($"HTTP configuration missing for dynamic MCP tool: {toolConfig.Name}");
        }

        var httpConfig = toolConfig.Http;
        
        // Extract the original MCP tool name from the wrapper name
        // Format: {serverPrefix}_{originalToolName}
        var toolNameParts = toolConfig.Name.Split('_', 2);
        if (toolNameParts.Length < 2)
        {
            throw new InvalidOperationException($"Invalid dynamic MCP tool name format: {toolConfig.Name}");
        }

        var serverPrefix = toolNameParts[0];
        var originalToolName = toolNameParts[1];

        _logger.LogInformation("[MCP Dynamic] Calling tool '{OriginalName}' on server '{Prefix}'", 
            originalToolName, serverPrefix);
        _logger.LogInformation("[MCP Dynamic] RAW arguments from OpenAI: {Arguments}", arguments.GetRawText());

        // Use named client with Polly policies
        var httpClient = _httpClientFactory.CreateClient("ToolsHttpClient");
        
        // Add custom headers
        foreach (var header in httpConfig.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Prepare the arguments for the MCP tool
        // Keep arguments as JsonElement to preserve types correctly
        JsonElement mcpArguments = arguments;

        // Build JSON-RPC request for tools/call
        // Parse the entire request structure properly to preserve argument types
        var paramsObj = new Dictionary<string, object>
        {
            ["name"] = originalToolName
        };

        // Only add arguments if there are any (and not empty object)
        if (arguments.ValueKind != JsonValueKind.Undefined && 
            arguments.ValueKind != JsonValueKind.Null &&
            !(arguments.ValueKind == JsonValueKind.Object && arguments.EnumerateObject().Any() == false))
        {
            // Parse arguments element by element to preserve types
            var argsDict = new Dictionary<string, object?>();
            foreach (var property in arguments.EnumerateObject())
            {
                argsDict[property.Name] = ParseJsonElementToObject(property.Value);
            }
            paramsObj["arguments"] = argsDict;
        }

        var mcpRequest = new
        {
            jsonrpc = "2.0",
            id = Random.Shared.Next(1, 1000000),
            method = "tools/call",
            @params = paramsObj
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var requestJson = JsonSerializer.Serialize(mcpRequest, jsonOptions);
        _logger.LogInformation("[MCP Dynamic] Final JSON-RPC request being sent:");
        _logger.LogInformation("[MCP Dynamic] {Request}", requestJson);

        var content = new StringContent(
            requestJson,
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(httpConfig.Url, content);
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("[MCP Dynamic] Received response: {Response}", 
            responseBody.Length > 500 ? responseBody.Substring(0, 500) + "..." : responseBody);

        try
        {
            // Parse JSON-RPC response
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Check for error in JSON-RPC response
            if (root.TryGetProperty("error", out var errorElement))
            {
                var errorMessage = errorElement.TryGetProperty("message", out var msg) 
                    ? msg.GetString() 
                    : "Unknown MCP error";
                throw new InvalidOperationException($"MCP Error: {errorMessage}");
            }

            // Return the result field from JSON-RPC response
            if (root.TryGetProperty("result", out var resultElement))
            {
                return JsonSerializer.Deserialize<object>(resultElement.GetRawText()) ?? new { };
            }

            return new { response = responseBody };
        }
        catch (JsonException)
        {
            // Return as string if not valid JSON
            return new { response = responseBody };
        }
    }

    private async Task<object> ExecuteMcpToolAsync(ToolConfig toolConfig, JsonElement arguments)
    {
        if (toolConfig.Http == null)
        {
            throw new InvalidOperationException($"HTTP configuration missing for MCP tool: {toolConfig.Name}");
        }

        var httpConfig = toolConfig.Http;
        _logger.LogInformation("[MCP {ToolName}] RAW arguments received: {Arguments}", toolConfig.Name, arguments.GetRawText());

        // Use named client with Polly policies
        var httpClient = _httpClientFactory.CreateClient("ToolsHttpClient");
        
        // Add custom headers
        foreach (var header in httpConfig.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Extract MCP method and params from arguments
        string mcpMethod = "resources/list"; // default
        object? mcpParams = null;

        var argumentsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments.GetRawText());
        _logger.LogInformation("[MCP {ToolName}] Parsed arguments dictionary: {Dict}", 
            toolConfig.Name, 
            argumentsDict != null ? string.Join(", ", argumentsDict.Keys) : "null");
        if (argumentsDict != null)
        {
            if (argumentsDict.TryGetValue("method", out var methodElement))
            {
                mcpMethod = methodElement.GetString() ?? "resources/list";
            }
            
            if (argumentsDict.TryGetValue("params", out var paramsElement))
            {
                // Parse params as a dictionary to properly serialize it
                try
                {
                    var paramsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(paramsElement.GetRawText());
                    mcpParams = paramsDict;
                }
                catch
                {
                    mcpParams = JsonSerializer.Deserialize<object>(paramsElement.GetRawText());
                }
            }
            // If no explicit "params", extract all keys as params
            else if (argumentsDict.Count > 0 && !argumentsDict.ContainsKey("method"))
            {
                // All arguments become the params
                var paramDict = new Dictionary<string, object?>();
                foreach (var kvp in argumentsDict)
                {
                    try
                    {
                        var keyName = kvp.Key;
                        
                        // Normalize "input" to "arguments" for MCP tools/call
                        if (keyName == "input" && toolConfig.Name.EndsWith("_call_tool"))
                        {
                            keyName = "arguments";
                            _logger.LogInformation("[MCP {ToolName}] Normalizing 'input' -> 'arguments'", toolConfig.Name);
                        }
                        
                        // Special handling for nested "arguments" field (for tools/call)
                        if (keyName == "arguments" && kvp.Value.ValueKind == JsonValueKind.Object)
                        {
                            var nestedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(kvp.Value.GetRawText());
                            paramDict[keyName] = nestedDict;
                        }
                        else
                        {
                            paramDict[keyName] = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
                        }
                    }
                    catch
                    {
                        paramDict[kvp.Key] = kvp.Value.GetRawText();
                    }
                }
                
                if (paramDict.Count > 0)
                {
                    mcpParams = paramDict;
                }
            }
        }

        // Determine the actual method based on tool name if not explicitly provided
        if (mcpMethod == "resources/list")
        {
            if (toolConfig.Name.EndsWith("_read_resource"))
            {
                mcpMethod = "resources/read";
            }
            else if (toolConfig.Name.EndsWith("_list_tools"))
            {
                mcpMethod = "tools/list";
            }
            else if (toolConfig.Name.EndsWith("_call_tool"))
            {
                mcpMethod = "tools/call";
            }
            else if (toolConfig.Name.EndsWith("_list_prompts"))
            {
                mcpMethod = "prompts/list";
            }
            else if (toolConfig.Name.EndsWith("_get_prompt"))
            {
                mcpMethod = "prompts/get";
            }
            else if (toolConfig.Name.EndsWith("_ping"))
            {
                mcpMethod = "ping";
            }
        }

        // Log the final method and params before building request
        _logger.LogInformation("[MCP {ToolName}] Final: method={Method}, params={Params}", 
            toolConfig.Name, 
            mcpMethod, 
            mcpParams != null ? JsonSerializer.Serialize(mcpParams) : "null");

        // Build JSON-RPC request with numeric ID (required by MCP protocol)
        var mcpRequest = new
        {
            jsonrpc = "2.0",
            id = Random.Shared.Next(1, 1000000),
            method = mcpMethod,
            @params = mcpParams
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var requestJson = JsonSerializer.Serialize(mcpRequest, jsonOptions);
        _logger.LogInformation("[MCP {ToolName}] Sending request: {Request}", toolConfig.Name, requestJson);

        var content = new StringContent(
            requestJson,
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync(httpConfig.Url, content);
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("[MCP {ToolName}] Received response: {Response}", 
            toolConfig.Name, 
            responseBody.Length > 500 ? responseBody.Substring(0, 500) + "..." : responseBody);

        try
        {
            // Parse JSON-RPC response
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Check for error in JSON-RPC response
            if (root.TryGetProperty("error", out var errorElement))
            {
                var errorMessage = errorElement.TryGetProperty("message", out var msg) 
                    ? msg.GetString() 
                    : "Unknown MCP error";
                throw new InvalidOperationException($"MCP Error: {errorMessage}");
            }

            // Return the result field from JSON-RPC response
            if (root.TryGetProperty("result", out var resultElement))
            {
                return JsonSerializer.Deserialize<object>(resultElement.GetRawText()) ?? new { };
            }

            return new { response = responseBody };
        }
        catch (JsonException)
        {
            // Return as string if not valid JSON
            return new { response = responseBody };
        }
    }

    private async Task<object> ExecuteHttpToolAsync(ToolConfig toolConfig, JsonElement arguments)
    {
        if (toolConfig.Http == null)
        {
            throw new InvalidOperationException($"HTTP configuration missing for tool: {toolConfig.Name}");
        }

        var httpConfig = toolConfig.Http;
        _logger.LogInformation("Executing HTTP tool: {Method} {Url}", httpConfig.Method, httpConfig.Url);

        // Use named client with Polly policies
        var httpClient = _httpClientFactory.CreateClient("ToolsHttpClient");
        
        // Add custom headers
        foreach (var header in httpConfig.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        HttpResponseMessage response;
        var url = httpConfig.Url;

        // Replace URL parameters from arguments (for GET requests with path params)
        var argumentsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments.GetRawText());
        if (argumentsDict != null)
        {
            foreach (var arg in argumentsDict)
            {
                url = url.Replace($"{{{arg.Key}}}", arg.Value.ToString());
            }
        }

        switch (httpConfig.Method.ToUpperInvariant())
        {
            case "GET":
                // Add query parameters
                if (argumentsDict != null && argumentsDict.Any())
                {
                    var queryString = string.Join("&", argumentsDict.Select(kvp => 
                        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"));
                    url = url.Contains("?") ? $"{url}&{queryString}" : $"{url}?{queryString}";
                }
                response = await httpClient.GetAsync(url);
                break;

            case "POST":
            case "PUT":
                var content = new StringContent(
                    arguments.GetRawText(),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                response = httpConfig.Method.ToUpperInvariant() == "POST"
                    ? await httpClient.PostAsync(url, content)
                    : await httpClient.PutAsync(url, content);
                break;

            case "DELETE":
                response = await httpClient.DeleteAsync(url);
                break;

            default:
                throw new ArgumentException($"Unsupported HTTP method: {httpConfig.Method}");
        }

        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();

        try
        {
            // Try to parse as JSON
            var jsonResponse = JsonSerializer.Deserialize<object>(responseBody);
            return jsonResponse ?? responseBody;
        }
        catch
        {
            // Return as string if not valid JSON
            return new { response = responseBody };
        }
    }

    private async Task<object> ExecuteBuiltinToolAsync(string toolName, JsonElement arguments)
    {
        return toolName switch
        {
            "get_weather" => await GetWeatherAsync(arguments),
            "get_time" => await GetTimeAsync(arguments),
            "calculate" => await CalculateAsync(arguments),
            _ => throw new ArgumentException($"Unknown builtin tool: {toolName}")
        };
    }

    public List<string> GetAvailableTools()
    {
        return _settings.Tools?.Select(t => t.Name).ToList() ?? new List<string>();
    }

    /// <summary>
    /// Parse JsonElement to appropriate C# type (string, number, bool, object, array)
    /// This ensures types are preserved correctly when forwarding to MCP server
    /// </summary>
    private object? ParseJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : 
                                   element.TryGetInt64(out var longVal) ? longVal : 
                                   element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => ParseJsonObject(element),
            JsonValueKind.Array => ParseJsonArray(element),
            _ => element.GetRawText()
        };
    }

    private Dictionary<string, object?> ParseJsonObject(JsonElement element)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = ParseJsonElementToObject(property.Value);
        }
        return dict;
    }

    private List<object?> ParseJsonArray(JsonElement element)
    {
        var list = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(ParseJsonElementToObject(item));
        }
        return list;
    }

    private async Task<object> GetWeatherAsync(JsonElement arguments)
    {
        // Extract parameters
        var location = arguments.TryGetProperty("location", out var loc) 
            ? loc.GetString() 
            : throw new ArgumentException("Missing required parameter: location");

        var unit = arguments.TryGetProperty("unit", out var u) && u.GetString() == "fahrenheit" 
            ? "fahrenheit" 
            : "celsius";

        _logger.LogInformation("Getting weather for {Location} in {Unit}", location, unit);

        // Simulate API call (replace with real weather API)
        await Task.Delay(100);

        // Mock weather data
        var temperature = unit == "celsius" ? 22 : 72;
        var tempUnit = unit == "celsius" ? "°C" : "°F";

        return new
        {
            location = location,
            temperature = temperature,
            unit = tempUnit,
            condition = "Ensoleillé",
            humidity = 65,
            wind_speed = 15,
            description = $"Il fait actuellement {temperature}{tempUnit} à {location} avec un temps ensoleillé."
        };
    }

    private async Task<object> GetTimeAsync(JsonElement arguments)
    {
        var timezone = arguments.TryGetProperty("timezone", out var tz) 
            ? tz.GetString() 
            : throw new ArgumentException("Missing required parameter: timezone");

        _logger.LogInformation("Getting time for timezone: {Timezone}", timezone);

        await Task.Delay(50);

        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var currentTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);

            return new
            {
                timezone = timezone,
                datetime = currentTime.ToString("o"),
                formatted = currentTime.ToString("dddd d MMMM yyyy HH:mm:ss"),
                hour = currentTime.Hour,
                minute = currentTime.Minute,
                second = currentTime.Second
            };
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"Timezone not found: {timezone}");
        }
    }

    private async Task<object> CalculateAsync(JsonElement arguments)
    {
        var expression = arguments.TryGetProperty("expression", out var expr) 
            ? expr.GetString() 
            : throw new ArgumentException("Missing required parameter: expression");

        _logger.LogInformation("Calculating expression: {Expression}", expression);

        await Task.Delay(50);

        try
        {
            // Simple calculator using DataTable.Compute (for basic expressions)
            var dataTable = new System.Data.DataTable();
            var result = dataTable.Compute(expression, "");

            return new
            {
                expression = expression,
                result = result,
                formatted = $"{expression} = {result}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating expression: {Expression}", expression);
            throw new ArgumentException($"Invalid expression: {expression}");
        }
    }
}
