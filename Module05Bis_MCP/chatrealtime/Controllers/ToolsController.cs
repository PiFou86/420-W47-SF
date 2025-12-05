using System.Text.Json;
using chatrealtime.Configuration;
using chatrealtime.Services.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace chatrealtime.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToolsController : ControllerBase
{
    private readonly IToolExecutor _toolExecutor;
    private readonly OpenAISettings _settings;
    private readonly ILogger<ToolsController> _logger;

    public ToolsController(
        IToolExecutor toolExecutor,
        IOptions<OpenAISettings> settings,
        ILogger<ToolsController> logger)
    {
        _toolExecutor = toolExecutor;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get list of available tools
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ToolInfo>), 200)]
    public IActionResult GetTools()
    {
        var tools = _settings.Tools?.Select(t => new ToolInfo
        {
            Name = t.Name,
            Description = t.Description,
            Parameters = t.Parameters
        }).ToList() ?? new List<ToolInfo>();

        return Ok(new
        {
            count = tools.Count,
            tools = tools
        });
    }

    /// <summary>
    /// Execute a specific tool
    /// </summary>
    [HttpPost("{toolName}")]
    [ProducesResponseType(typeof(ToolExecutionResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExecuteTool(
        [FromRoute] string toolName,
        [FromBody] JsonElement arguments)
    {
        try
        {
            // Check if tool exists
            var tool = _settings.Tools?.FirstOrDefault(t => 
                t.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));

            if (tool == null)
            {
                return NotFound(new
                {
                    error = "Tool not found",
                    tool = toolName,
                    available_tools = _toolExecutor.GetAvailableTools()
                });
            }

            _logger.LogInformation("HTTP request to execute tool: {ToolName}", toolName);

            var result = await _toolExecutor.ExecuteAsync(toolName, arguments);

            return Ok(new ToolExecutionResult
            {
                Tool = toolName,
                Success = true,
                Result = result,
                ExecutedAt = DateTime.UtcNow
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments for tool: {ToolName}", toolName);
            return BadRequest(new
            {
                error = ex.Message,
                tool = toolName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolName);
            return StatusCode(500, new
            {
                error = "Internal server error",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get weather for a specific location (shortcut endpoint)
    /// </summary>
    [HttpGet("weather/{location}")]
    [ProducesResponseType(typeof(ToolExecutionResult), 200)]
    public async Task<IActionResult> GetWeather(
        [FromRoute] string location,
        [FromQuery] string unit = "celsius")
    {
        var arguments = JsonDocument.Parse($@"{{
            ""location"": ""{location}"",
            ""unit"": ""{unit}""
        }}").RootElement;

        return await ExecuteTool("get_weather", arguments);
    }

    /// <summary>
    /// Get time for a specific timezone (shortcut endpoint)
    /// </summary>
    [HttpGet("time/{timezone}")]
    [ProducesResponseType(typeof(ToolExecutionResult), 200)]
    public async Task<IActionResult> GetTime([FromRoute] string timezone)
    {
        // Replace underscores with slashes for timezone format
        timezone = timezone.Replace("_", "/");
        
        var arguments = JsonDocument.Parse($@"{{
            ""timezone"": ""{timezone}""
        }}").RootElement;

        return await ExecuteTool("get_time", arguments);
    }
}

public class ToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object Parameters { get; set; } = new();
}

public class ToolExecutionResult
{
    public string Tool { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? Result { get; set; }
    public DateTime ExecutedAt { get; set; }
}
