using System.Text.Json;

namespace mcpservertest01.Models.JsonRpc;

/// <summary>
/// Représente une requête JSON-RPC 2.0
/// </summary>
public class JsonRpcRequest
{
    public string jsonrpc { get; set; } = "2.0";
    public required string method { get; set; }
    public JsonElement @params { get; set; }
    public int id { get; set; }
}

/// <summary>
/// Requête JSON-RPC générique avec paramètres typés
/// </summary>
/// <typeparam name="TParams">Type des paramètres</typeparam>
public class JsonRpcRequest<TParams> where TParams : class
{
    public string jsonrpc { get; set; } = "2.0";
    public required string method { get; set; }
    public TParams? @params { get; set; }
    public int id { get; set; }
}
