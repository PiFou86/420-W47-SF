# Architecture du Serveur MCP Refactorisé

## 📁 Structure du Projet

```
mcpservertest01/
├── Models/                          # Modèles de données
│   ├── JsonRpc/                     # Modèles JSON-RPC 2.0
│   │   ├── JsonRpcRequest.cs        # Requête JSON-RPC
│   │   ├── JsonRpcResponse.cs       # Réponse JSON-RPC
│   │   └── JsonRpcError.cs          # Gestion des erreurs
│   └── Mcp/                         # Modèles MCP
│       ├── Resource.cs              # Ressources MCP
│       ├── Tool.cs                  # Outils MCP
│       ├── Prompt.cs                # Prompts MCP
│       ├── Initialize.cs            # Initialisation
│       └── Responses/               # Réponses typées
│           └── McpResponses.cs
│
├── Services/                        # Logique métier
│   ├── IMcpService.cs              # Interface du service principal
│   ├── McpService.cs               # Orchestrateur principal
│   ├── IResourceRepository.cs      # Interface repository
│   └── ResourceRepository.cs       # Gestion des ressources
│
├── Handlers/                        # Gestionnaires de méthodes
│   ├── IMethodHandler.cs           # Interface commune
│   ├── InitializeHandler.cs        # initialize
│   ├── PingHandler.cs              # ping (health check)
│   ├── ResourcesHandler.cs         # resources/*
│   ├── ToolsHandler.cs             # tools/*
│   └── PromptsHandler.cs           # prompts/*
│
├── Extensions/                      # Extensions et helpers
│   ├── JsonElementExtensions.cs    # Manipulation JsonElement
│   └── ServiceCollectionExtensions.cs # Configuration DI
│
├── Configuration/                   # Configuration
│   └── McpOptions.cs               # Options de debug
│
└── Program.cs                       # Point d'entrée (simplifié)
```

## 🎯 Principes d'Architecture

### 1. **Séparation des Responsabilités (SRP)**
- **Models** : Structures de données pures
- **Services** : Logique métier
- **Handlers** : Traitement des méthodes MCP
- **Extensions** : Utilitaires réutilisables

### 2. **Dependency Injection (DI)**
```csharp
builder.Services.AddMcpServices(builder.Configuration);
```
Tous les services sont injectés automatiquement.

### 3. **Pattern Strategy**
Chaque handler implémente `IMethodHandler` :
```csharp
public interface IMethodHandler
{
    string[] SupportedMethods { get; }
    Task<object?> HandleAsync(string method, JsonElement parameters, int requestId);
}
```

### 4. **Pattern Repository**
`ResourceRepository` encapsule l'accès aux ressources :
```csharp
public interface IResourceRepository
{
    Resource[] GetAllResources();
    ResourceContent ReadResource(string uri);
}
```

## 🔧 Améliorations Clés

### ✅ Typage Fort sans Bugs

**Problème résolu** : Les schémas JSON sont dynamiques par nature.

**Solution** : `Dictionary<string, object>` pour les schémas flexibles :
```csharp
public class Tool
{
    public required string name { get; set; }
    public Dictionary<string, object>? inputSchema { get; set; }  // ✅ Flexible
}
```

### ✅ Gestion d'Erreurs Robuste

Codes d'erreur JSON-RPC standardisés :
```csharp
public static class JsonRpcErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    // ...
}
```

### ✅ Extensions pour Simplifier

```csharp
string uri = parameters.GetPropertyString("uri") ?? "";
int value = parameters.GetPropertyInt32("value", 0);
```

### ✅ Logging et Debug

- Logging structuré avec `ILogger`
- Debug YAML/JSON configurable
- Affichage coloré dans la console

## 🚀 Utilisation

### Ajouter un Nouveau Handler

1. Créer une classe implémentant `IMethodHandler`
2. L'enregistrer dans `ServiceCollectionExtensions`
3. C'est tout ! Le système l'utilisera automatiquement

```csharp
public class MyNewHandler : IMethodHandler
{
    public string[] SupportedMethods => new[] { "my/method" };
    
    public Task<object?> HandleAsync(string method, JsonElement parameters, int requestId)
    {
        // Votre logique ici
        return Task.FromResult<object?>(response);
    }
}
```

### Ajouter une Nouvelle Ressource

Modifier `ResourceRepository.GetAllResources()` et `ReadResource()`.

### Configuration Debug

Dans `appsettings.json` :
```json
{
  "DebugMCP": {
    "EnableDebugOutput": true,
    "Format": "yaml"  // ou "json"
  }
}
```

## 🔍 Points Techniques Importants

### Nullable Reference Types
Le projet utilise `<Nullable>enable</Nullable>` pour la sécurité des types.

### Required Properties
```csharp
public required string name { get; set; }
```
Force l'initialisation à la création de l'objet.

### Records
```csharp
public record McpOptions { ... }
```
Immutabilité par défaut pour la configuration.

### Pattern Matching
```csharp
return method switch
{
    "resources/list" => HandleResourcesListAsync(),
    "resources/read" => HandleResourcesReadAsync(parameters),
    _ => throw new InvalidOperationException()
};
```

## 🎓 Concepts Appliqués

- ✅ **SOLID Principles**
- ✅ **Clean Architecture**
- ✅ **Dependency Injection**
- ✅ **Repository Pattern**
- ✅ **Strategy Pattern**
- ✅ **Extension Methods**
- ✅ **Options Pattern**

## 🧪 Tests (À Ajouter)

Structure recommandée :
```
Tests/
├── Handlers/
│   ├── InitializeHandlerTests.cs
│   ├── ResourcesHandlerTests.cs
│   └── ...
└── Services/
    └── McpServiceTests.cs
```

## 📈 Performance

- **Cache des handlers** : Lookup O(1) au lieu de switch O(n)
- **Dependency Injection** : Services singleton
- **Async/Await** : Support natif de l'asynchronie
