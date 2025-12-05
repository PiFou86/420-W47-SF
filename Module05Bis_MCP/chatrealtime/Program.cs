using chatrealtime.Configuration;
using chatrealtime.Services;
using chatrealtime.Services.Tools;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenAI settings from appsettings.json
var openAISettings = builder.Configuration.GetSection("OpenAI").Get<OpenAISettings>() ?? new OpenAISettings();

// Generate MCP tools automatically from McpServers configuration
if (openAISettings.McpServers != null && openAISettings.McpServers.Any())
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    openAISettings.Tools ??= new List<ToolConfig>();
    
    foreach (var mcpServer in openAISettings.McpServers)
    {
        logger.LogInformation("Generating MCP tools for server: {Name} at {Url}", mcpServer.Name, mcpServer.Url);
        openAISettings.Tools.AddRange(GenerateMcpTools(mcpServer));
    }
}

builder.Services.Configure<OpenAISettings>(options =>
{
    builder.Configuration.GetSection("OpenAI").Bind(options);
    options.Tools = openAISettings.Tools;
});

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configure HttpClient with Polly resilience policies
var resilienceSettings = openAISettings.Resilience;

builder.Services.AddHttpClient("ToolsHttpClient")
    .AddPolicyHandler((services, request) =>
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Build the pipeline of policies
        var policies = new List<IAsyncPolicy<HttpResponseMessage>>();

        // 1. Timeout policy (innermost)
        if (resilienceSettings.Timeout.Enabled)
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(resilienceSettings.Timeout.TimeoutSeconds),
                TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, timeout, task) =>
                {
                    logger.LogWarning("[Polly Timeout] Request timed out after {Timeout}s", timeout.TotalSeconds);
                    return Task.CompletedTask;
                });
            policies.Add(timeoutPolicy);
        }

        // 2. Retry policy (middle layer)
        if (resilienceSettings.Retry.Enabled)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: resilienceSettings.Retry.MaxRetryAttempts,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var delay = Math.Min(
                            resilienceSettings.Retry.InitialDelayMs * Math.Pow(2, retryAttempt - 1),
                            resilienceSettings.Retry.MaxDelayMs
                        );
                        return TimeSpan.FromMilliseconds(delay);
                    },
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning(
                            "[Polly Retry] Retry {RetryAttempt}/{MaxRetries} after {Delay}ms. Reason: {Reason}",
                            retryAttempt,
                            resilienceSettings.Retry.MaxRetryAttempts,
                            timespan.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown"
                        );
                    });
            policies.Add(retryPolicy);
        }

        // 3. Circuit Breaker policy (outermost)
        if (resilienceSettings.CircuitBreaker.Enabled)
        {
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5, // 50% failure rate
                    samplingDuration: TimeSpan.FromSeconds(resilienceSettings.CircuitBreaker.SamplingDurationSeconds),
                    minimumThroughput: resilienceSettings.CircuitBreaker.FailureThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(resilienceSettings.CircuitBreaker.BreakDurationSeconds),
                    onBreak: (result, breakDelay) =>
                    {
                        logger.LogError(
                            "[Polly Circuit Breaker] Circuit opened for {BreakDuration}s. Reason: {Reason}",
                            breakDelay.TotalSeconds,
                            result.Exception?.Message ?? result.Result?.StatusCode.ToString() ?? "Unknown"
                        );
                    },
                    onReset: () =>
                    {
                        logger.LogInformation("[Polly Circuit Breaker] Circuit reset (closed)");
                    },
                    onHalfOpen: () =>
                    {
                        logger.LogInformation("[Polly Circuit Breaker] Circuit half-open (testing)");
                    });
            policies.Add(circuitBreakerPolicy);
        }

        // Combine all policies into a single policy wrap (outer to inner: CB -> Retry -> Timeout)
        if (policies.Count == 0)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }
        else if (policies.Count == 1)
        {
            return policies[0];
        }
        else
        {
            // Reverse to wrap correctly: CircuitBreaker wraps Retry wraps Timeout
            policies.Reverse();
            return Policy.WrapAsync(policies.ToArray());
        }
    });

builder.Services.AddTransient<OpenAIRealtimeService>();
builder.Services.AddSingleton<RealtimeWebSocketHandler>();
builder.Services.AddSingleton<IToolExecutor, ToolExecutorService>();
builder.Services.AddSingleton<McpDiscoveryService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable WebSocket support
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

// Map API controllers
app.MapControllers();

// WebSocket endpoint for realtime communication
app.Map("/ws/realtime", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<RealtimeWebSocketHandler>();
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await handler.HandleWebSocketAsync(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

// Helper method to generate MCP tools
static List<ToolConfig> GenerateMcpTools(McpServerConfig mcpServer)
{
    var tools = new List<ToolConfig>();
    var serverPrefix = string.IsNullOrEmpty(mcpServer.Name) ? "mcp" : mcpServer.Name;
    var serverDesc = string.IsNullOrEmpty(mcpServer.Description) ? "serveur MCP" : mcpServer.Description;

    // 1. List Resources
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_list_resources",
        Description = $"Lister toutes les ressources disponibles sur {serverDesc}",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new { }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    // 2. List Tools
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_list_tools",
        Description = $"Lister tous les outils disponibles sur {serverDesc}",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new { }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    // 3. Read Resource
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_read_resource",
        Description = $"Lire le contenu d'une ressource spécifique de {serverDesc}. L'URI est obtenu via {serverPrefix}_list_resources.",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                uri = new
                {
                    type = "string",
                    description = "URI de la ressource à lire (ex: 'file:///path/to/file.txt')"
                }
            },
            required = new[] { "uri" }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    // 4. Call Tool
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_call_tool",
        Description = $"Appeler un outil spécifique de {serverDesc}. Le nom de l'outil est obtenu via {serverPrefix}_list_tools.",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                name = new
                {
                    type = "string",
                    description = "Nom de l'outil MCP à appeler"
                },
                arguments = new
                {
                    type = "object",
                    description = "Arguments à passer à l'outil (optionnel)"
                }
            },
            required = new[] { "name" }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    // 5. List Prompts
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_list_prompts",
        Description = $"Lister tous les prompts (templates de prompts) disponibles sur {serverDesc}",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new { }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    // 6. Get Prompt
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_get_prompt",
        Description = $"Obtenir un prompt spécifique de {serverDesc}. Le nom du prompt est obtenu via {serverPrefix}_list_prompts.",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                name = new
                {
                    type = "string",
                    description = "Nom du prompt à obtenir"
                },
                arguments = new
                {
                    type = "object",
                    description = "Arguments pour le prompt (optionnel)"
                }
            },
            required = new[] { "name" }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    // 7. Ping (health check)
    tools.Add(new ToolConfig
    {
        Name = $"{serverPrefix}_ping",
        Description = $"Vérifier que {serverDesc} est accessible et fonctionnel (health check)",
        Type = "mcp",
        Parameters = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new { }
        }),
        Http = new HttpToolConfig
        {
            Url = mcpServer.Url,
            Method = "POST",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        }
    });

    return tools;
}
