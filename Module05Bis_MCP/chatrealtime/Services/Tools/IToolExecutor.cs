using System.Text.Json;

namespace chatrealtime.Services.Tools;

/// <summary>
/// Interface for executing MCP tools
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Execute a tool by name with the provided arguments
    /// </summary>
    /// <param name="toolName">Name of the tool to execute</param>
    /// <param name="arguments">JSON arguments for the tool</param>
    /// <returns>Tool execution result as JSON</returns>
    Task<object> ExecuteAsync(string toolName, JsonElement arguments);
    
    /// <summary>
    /// Get list of available tools
    /// </summary>
    List<string> GetAvailableTools();
}
