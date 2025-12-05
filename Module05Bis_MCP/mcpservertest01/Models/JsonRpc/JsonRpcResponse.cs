using System.Text.Json.Serialization;

namespace mcpservertest01.Models.JsonRpc;

/// <summary>
/// Représente une réponse JSON-RPC 2.0
/// </summary>
public class JsonRpcResponse
{
    public string jsonrpc { get; set; } = "2.0";
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? result { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? error { get; set; }
    
    public int id { get; set; }
}

/// <summary>
/// Réponse JSON-RPC générique avec résultat typé
/// </summary>
/// <typeparam name="TResult">Type du résultat</typeparam>
public class JsonRpcResponse<TResult>
{
    public string jsonrpc { get; set; } = "2.0";
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TResult? result { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonRpcError? error { get; set; }
    
    public int id { get; set; }
}
