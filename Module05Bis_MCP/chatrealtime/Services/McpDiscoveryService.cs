using System.Text.Json;
using chatrealtime.Configuration;
using chatrealtime.Services.Tools;

namespace chatrealtime.Services;

/// <summary>
/// Service responsible for discovering MCP server capabilities and dynamically registering tools
/// </summary>
public class McpDiscoveryService
{
    private readonly ILogger<McpDiscoveryService> _logger;
    private readonly IToolExecutor _toolExecutor;
    private readonly OpenAISettings _settings;
    
    // Cache for discovered capabilities to avoid duplicate API calls
    private Dictionary<string, ServerCapabilities> _capabilitiesCache = new();

    public McpDiscoveryService(
        ILogger<McpDiscoveryService> logger,
        IToolExecutor toolExecutor,
        Microsoft.Extensions.Options.IOptions<OpenAISettings> settings)
    {
        _logger = logger;
        _toolExecutor = toolExecutor;
        _settings = settings.Value;
    }

    private class ServerCapabilities
    {
        public List<JsonElement> Tools { get; set; } = new();
        public JsonElement? ToolsResponse { get; set; }
        public JsonElement? ResourcesResponse { get; set; }
        public JsonElement? PromptsResponse { get; set; }
    }

    /// <summary>
    /// Discover all MCP servers and their capabilities, returning dynamically generated tool configurations
    /// </summary>
    public async Task<List<ToolConfig>> DiscoverAllServersAsync(CancellationToken cancellationToken = default)
    {
        var discoveredTools = new List<ToolConfig>();

        if (_settings.McpServers == null || !_settings.McpServers.Any())
        {
            _logger.LogInformation("No MCP servers configured, skipping discovery");
            return discoveredTools;
        }

        _logger.LogInformation("Starting MCP discovery for {Count} server(s)", _settings.McpServers.Count);

        foreach (var mcpServer in _settings.McpServers)
        {
            try
            {
                var serverTools = await DiscoverServerAsync(mcpServer, cancellationToken);
                discoveredTools.AddRange(serverTools);
                _logger.LogInformation("Successfully discovered {Count} tools from {ServerName}", 
                    serverTools.Count, mcpServer.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover tools from MCP server: {ServerName}", mcpServer.Name);
            }
        }

        _logger.LogInformation("MCP discovery completed. Total tools discovered: {Count}", discoveredTools.Count);
        return discoveredTools;
    }

    /// <summary>
    /// Discover capabilities for a single MCP server
    /// </summary>
    private async Task<List<ToolConfig>> DiscoverServerAsync(
        McpServerConfig mcpServer, 
        CancellationToken cancellationToken)
    {
        var discoveredTools = new List<ToolConfig>();
        var serverPrefix = string.IsNullOrEmpty(mcpServer.Name) ? "mcp" : mcpServer.Name;
        var serverKey = $"{mcpServer.Name}_{mcpServer.Url}";

        _logger.LogInformation("Discovering tools from MCP server: {Name} ({Url})", mcpServer.Name, mcpServer.Url);

        try
        {
            // 1. Test connectivity with ping
            _logger.LogDebug("Pinging MCP server: {Name}", mcpServer.Name);
            try
            {
                await _toolExecutor.ExecuteAsync($"{serverPrefix}_ping", 
                    JsonSerializer.SerializeToElement(new { }));
                _logger.LogInformation("✅ MCP server {Name} is accessible", mcpServer.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Ping failed for MCP server {Name}, continuing anyway...", mcpServer.Name);
            }

            // 2. List available tools
            _logger.LogInformation("Fetching tool list from MCP server: {Name}", mcpServer.Name);
            var toolsResult = await _toolExecutor.ExecuteAsync($"{serverPrefix}_list_tools", 
                JsonSerializer.SerializeToElement(new { }));

            // Parse the tools list
            var toolsJson = JsonSerializer.Serialize(toolsResult);
            _logger.LogDebug("Tools list response: {Response}", 
                toolsJson.Length > 500 ? toolsJson.Substring(0, 500) + "..." : toolsJson);

            using var doc = JsonDocument.Parse(toolsJson);
            var root = doc.RootElement.Clone(); // Clone to keep it after doc is disposed
            
            // Initialize cache for this server
            _capabilitiesCache[serverKey] = new ServerCapabilities
            {
                ToolsResponse = root
            };

            // MCP protocol returns tools in a "tools" array
            if (root.TryGetProperty("tools", out var toolsArray) && toolsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var tool in toolsArray.EnumerateArray())
                {
                    try
                    {
                        var toolConfig = ParseMcpTool(tool, mcpServer, serverPrefix);
                        if (toolConfig != null)
                        {
                            discoveredTools.Add(toolConfig);
                            _logger.LogInformation("  ✓ Discovered tool: {ToolName}", toolConfig.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse MCP tool from server {ServerName}", mcpServer.Name);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No 'tools' array found in response from {ServerName}", mcpServer.Name);
            }

            // 3. List available resources (for information only, not creating tools)
            try
            {
                _logger.LogDebug("Fetching resource list from MCP server: {Name}", mcpServer.Name);
                var resourcesResult = await _toolExecutor.ExecuteAsync($"{serverPrefix}_list_resources", 
                    JsonSerializer.SerializeToElement(new { }));
                var resourcesJson = JsonSerializer.Serialize(resourcesResult);
                
                using var resDoc = JsonDocument.Parse(resourcesJson);
                var resRoot = resDoc.RootElement.Clone();
                _capabilitiesCache[serverKey].ResourcesResponse = resRoot;
                
                if (resRoot.TryGetProperty("resources", out var resourcesArray) && 
                    resourcesArray.ValueKind == JsonValueKind.Array)
                {
                    var resourceCount = resourcesArray.GetArrayLength();
                    _logger.LogInformation("  ℹ️ Server has {Count} resources available", resourceCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not fetch resources list from {ServerName}", mcpServer.Name);
            }

            // 4. List available prompts (for information only)
            try
            {
                _logger.LogDebug("Fetching prompts list from MCP server: {Name}", mcpServer.Name);
                var promptsResult = await _toolExecutor.ExecuteAsync($"{serverPrefix}_list_prompts", 
                    JsonSerializer.SerializeToElement(new { }));
                var promptsJson = JsonSerializer.Serialize(promptsResult);
                
                using var promptDoc = JsonDocument.Parse(promptsJson);
                var promptRoot = promptDoc.RootElement.Clone();
                _capabilitiesCache[serverKey].PromptsResponse = promptRoot;
                
                if (promptRoot.TryGetProperty("prompts", out var promptsArray) && 
                    promptsArray.ValueKind == JsonValueKind.Array)
                {
                    var promptCount = promptsArray.GetArrayLength();
                    _logger.LogInformation("  ℹ️ Server has {Count} prompts available", promptCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not fetch prompts list from {ServerName}", mcpServer.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering server {ServerName}", mcpServer.Name);
            throw;
        }

        return discoveredTools;
    }

    /// <summary>
    /// Parse a single MCP tool definition and create a ToolConfig
    /// </summary>
    private ToolConfig? ParseMcpTool(JsonElement toolElement, McpServerConfig mcpServer, string serverPrefix)
    {
        // MCP tool schema:
        // {
        //   "name": "tool_name",
        //   "description": "Tool description",
        //   "inputSchema": { JSON Schema }
        // }

        if (!toolElement.TryGetProperty("name", out var nameElement))
        {
            _logger.LogWarning("MCP tool missing 'name' field");
            return null;
        }

        var toolName = nameElement.GetString();
        if (string.IsNullOrEmpty(toolName))
        {
            return null;
        }

        // Get description
        var description = toolElement.TryGetProperty("description", out var descElement) 
            ? descElement.GetString() ?? $"Tool {toolName} from {mcpServer.Name}"
            : $"Tool {toolName} from {mcpServer.Name}";

        // Get input schema
        JsonElement parameters;
        if (toolElement.TryGetProperty("inputSchema", out var inputSchemaElement))
        {
            parameters = inputSchemaElement;
        }
        else
        {
            // Default empty schema
            parameters = JsonSerializer.SerializeToElement(new
            {
                type = "object",
                properties = new { },
                required = new string[] { }
            });
        }

        // Create a wrapper tool that will call the MCP server's call_tool endpoint
        // The OpenAI function call will be: mcp_toolname(args)
        // Which will translate to: mcp_call_tool(name: "toolname", arguments: args)
        var wrappedToolName = $"{serverPrefix}_{toolName}";

        var toolConfig = new ToolConfig
        {
            Name = wrappedToolName,
            Description = description,
            Type = "mcp_dynamic", // Special type to indicate this is a dynamically discovered MCP tool
            Parameters = parameters,
            Http = new HttpToolConfig
            {
                Url = mcpServer.Url,
                Method = "POST",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            }
        };

        // Store the original MCP tool name in a custom property (for later use)
        // We'll need to modify ToolExecutorService to handle "mcp_dynamic" type
        return toolConfig;
    }

    /// <summary>
    /// Generate a markdown summary of discovered capabilities for inclusion in system prompt
    /// Uses cached results from DiscoverAllServersAsync to avoid duplicate API calls
    /// </summary>
    public Task<string> GenerateCapabilitiesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("\n## 🔧 Capacités MCP découvertes\n");

        if (_settings.McpServers == null || !_settings.McpServers.Any())
        {
            summary.AppendLine("_Aucun serveur MCP configuré._");
            return Task.FromResult(summary.ToString());
        }

        foreach (var mcpServer in _settings.McpServers)
        {
            var serverPrefix = string.IsNullOrEmpty(mcpServer.Name) ? "mcp" : mcpServer.Name;
            var serverKey = $"{mcpServer.Name}_{mcpServer.Url}";
            
            summary.AppendLine($"\n### Serveur: {mcpServer.Description ?? mcpServer.Name} ({mcpServer.Url})\n");

            try
            {
                // Use cached capabilities if available
                if (!_capabilitiesCache.TryGetValue(serverKey, out var capabilities))
                {
                    _logger.LogWarning("No cached capabilities for server {Name}, skipping summary", mcpServer.Name);
                    summary.AppendLine("_Capacités non disponibles (découverte non effectuée)_\n");
                    continue;
                }

                // List tools from cache
                if (capabilities.ToolsResponse.HasValue && 
                    capabilities.ToolsResponse.Value.TryGetProperty("tools", out var toolsArray) && 
                    toolsArray.ValueKind == JsonValueKind.Array)
                {
                    var toolCount = toolsArray.GetArrayLength();
                    summary.AppendLine($"**{toolCount} outils disponibles:**\n");
                    
                    foreach (var tool in toolsArray.EnumerateArray())
                    {
                        if (tool.TryGetProperty("name", out var name))
                        {
                            var desc = tool.TryGetProperty("description", out var d) ? d.GetString() : "";
                            summary.AppendLine($"- `{serverPrefix}_{name.GetString()}`: {desc}");
                        }
                    }
                    summary.AppendLine();
                }

                // List resources from cache
                if (capabilities.ResourcesResponse.HasValue &&
                    capabilities.ResourcesResponse.Value.TryGetProperty("resources", out var resourcesArray) && 
                    resourcesArray.ValueKind == JsonValueKind.Array)
                {
                    var resourceCount = resourcesArray.GetArrayLength();
                    if (resourceCount > 0)
                    {
                        summary.AppendLine($"**{resourceCount} ressources disponibles** (accessibles via `{serverPrefix}_read_resource`)\n");
                    }
                }
            }
            catch (Exception ex)
            {
                summary.AppendLine($"⚠️ **Erreur lors de la génération du résumé:** {ex.Message}\n");
                _logger.LogWarning(ex, "Failed to generate summary for MCP server: {Name}", mcpServer.Name);
            }
        }

        return Task.FromResult(summary.ToString());
    }
}
