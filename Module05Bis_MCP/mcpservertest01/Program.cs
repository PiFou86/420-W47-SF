using mcpservertest01.Extensions;
using mcpservertest01.Models.JsonRpc;
using mcpservertest01.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration des services MCP
builder.Services.AddMcpServices(builder.Configuration);

// Ajouter les contrôleurs pour l'API Spotify
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers();

// Endpoint MCP principal
app.MapPost("/mcp", async (HttpContext httpContext, IMcpService mcpService) =>
{
    using StreamReader reader = new StreamReader(httpContext.Request.Body);
    string body = await reader.ReadToEndAsync();

    JsonRpcResponse response = await mcpService.ProcessRequestAsync(body);

    return Results.Ok(response);
});

await app.RunAsync();
