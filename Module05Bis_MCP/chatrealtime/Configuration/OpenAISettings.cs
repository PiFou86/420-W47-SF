using System.Text.Json;

namespace chatrealtime.Configuration;

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-realtime-mini-2025-10-06";
    public string RealtimeUrl { get; set; } = "wss://api.openai.com/v1/realtime";
    public string Voice { get; set; } = "alloy";
    public string TranscriptionModel { get; set; } = "gpt-4o-transcribe";
    public string SystemPromptFile { get; set; } = "Prompts/Marvin.md";
    public double Temperature { get; set; } = 0.8;
    public int MaxResponseOutputTokens { get; set; } = 4096;
    public string Instructions { get; set; } = string.Empty;
    public TurnDetectionSettings TurnDetection { get; set; } = new();
    public List<ToolConfig> Tools { get; set; } = new();
    public List<McpServerConfig> McpServers { get; set; } = new();
    public ResilienceSettings Resilience { get; set; } = new();
}

public class TurnDetectionSettings
{
    public string Type { get; set; } = "server_vad";
    public double Threshold { get; set; } = 0.5;
    public int PrefixPaddingMs { get; set; } = 300;
    public int SilenceDurationMs { get; set; } = 500;
}

public class ToolConfig
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "builtin"; // builtin, http, or custom
    public JsonElement Parameters { get; set; }
    
    // For HTTP tools
    public HttpToolConfig? Http { get; set; }
}

public class HttpToolConfig
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST"; // GET, POST, PUT, DELETE
    public Dictionary<string, string> Headers { get; set; } = new();
    public string BodyTemplate { get; set; } = "{{arguments}}"; // Template for request body
}

public class ResilienceSettings
{
    public RetryPolicySettings Retry { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
    public TimeoutSettings Timeout { get; set; } = new();
}

public class RetryPolicySettings
{
    public bool Enabled { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 100;
    public int MaxDelayMs { get; set; } = 5000;
}

public class CircuitBreakerSettings
{
    public bool Enabled { get; set; } = true;
    public int FailureThreshold { get; set; } = 5;
    public int BreakDurationSeconds { get; set; } = 30;
    public int SamplingDurationSeconds { get; set; } = 60;
}

public class TimeoutSettings
{
    public bool Enabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
}

public class McpServerConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
