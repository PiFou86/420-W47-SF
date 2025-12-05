# Chat Realtime avec OpenAI GPT

Application de chat vocal en temps réel utilisant l'API OpenAI Realtime pour des conversations audio bidirectionnelles.

## 🎯 Fonctionnalités

- 🎤 **Capture audio en temps réel** depuis le microphone
- 🤖 **Intégration OpenAI Realtime API** avec le modèle `gpt-realtime-mini-2025-10-06`
- 💬 **Transcription automatique** des conversations (utilisateur et IA)
- 🔊 **Réponses audio** jouées directement dans le navigateur
- 🎨 **Interface moderne et responsive** avec animations
- ⚡ **Communication WebSocket** bidirectionnelle (Client ↔ Serveur C# ↔ OpenAI)

## 🏗️ Architecture

```
Client Web (Browser)
    ↓ WebSocket
Serveur ASP.NET Core 9.0
    ↓ WebSocket
OpenAI Realtime API
```

### Flux de données

1. **Audio utilisateur** : Microphone → Client → WebSocket → Serveur C# → OpenAI
2. **Réponse IA** : OpenAI → Serveur C# → WebSocket → Client → Haut-parleurs

## 📋 Prérequis

- **.NET 9.0 SDK** ou supérieur
- **Clé API OpenAI** avec accès à l'API Realtime
- **Navigateur moderne** supportant WebSocket et Web Audio API (Chrome, Edge recommandés)
- **Microphone** fonctionnel

## 🚀 Installation

### 1. Cloner ou télécharger le projet

```bash
cd /chemin/vers/chatrealtime
```

### 2. Configurer la clé API OpenAI

Ouvrez le fichier `appsettings.json` et remplacez `YOUR_OPENAI_API_KEY_HERE` par votre clé API :

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-xxxxxxxxxxxxxxxxxxxxx",
    "Model": "gpt-realtime-mini-2025-10-06",
    ...
  }
}
```

### 3. Restaurer les dépendances

```bash
dotnet restore
```

### 4. Lancer l'application

```bash
dotnet run
```

L'application sera disponible à :
- **HTTP** : http://localhost:5000
- **HTTPS** : https://localhost:5001

## ⚙️ Configuration

Toutes les configurations se trouvent dans `appsettings.json` :

| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `ApiKey` | Votre clé API OpenAI | À configurer |
| `Model` | Modèle OpenAI à utiliser | `gpt-realtime-mini-2025-10-06` |
| `Voice` | Voix de l'IA (alloy, echo, fable, onyx, nova, shimmer) | `echo` |
| `TranscriptionModel` | Modèle de transcription | `gpt-4o-transcribe` |
| `SystemPromptFile` | Fichier contenant le prompt système | `Prompts/Marvin.md` |
| `Temperature` | Créativité des réponses (0.0 - 2.0) | `0.8` |
| `MaxResponseOutputTokens` | Nombre maximum de tokens en sortie | `4096` |
| `Instructions` | Instructions système inline (si pas de fichier) | Texte personnalisable |
| `TurnDetection.Type` | Type de détection de tour de parole | `server_vad` |
| `TurnDetection.Threshold` | Seuil de détection vocale (0.0 - 1.0) | `0.5` |
| `TurnDetection.SilenceDurationMs` | Durée de silence pour fin de phrase | `500` ms |
| `Tools` | Liste d'outils MCP (voir ci-dessous) | `[]` |

### 🎭 Prompts système personnalisés

L'application supporte des **prompts système depuis des fichiers Markdown** :

```json
{
  "OpenAI": {
    "SystemPromptFile": "Prompts/Marvin.md"
  }
}
```

**Exemple fourni** : `Prompts/Marvin.md` - Personnalité de Marvin, le robot déprimé de H2G2

Pour créer votre propre personnalité :
1. Créez un fichier `.md` dans le dossier `Prompts/`
2. Décrivez la personnalité, le style et les consignes
3. Mettez à jour `SystemPromptFile` dans `appsettings.json`

### 🛠️ Configuration MCP (Model Context Protocol)

Vous pouvez configurer des **outils externes** que l'IA peut appeler pendant la conversation :

```json
{
  "OpenAI": {
    "Tools": [
      {
        "Name": "get_weather",
        "Description": "Obtenir la météo actuelle pour une ville donnée",
        "Parameters": {
          "type": "object",
          "properties": {
            "location": {
              "type": "string",
              "description": "Le nom de la ville (ex: Paris, Londres)"
            },
            "unit": {
              "type": "string",
              "enum": ["celsius", "fahrenheit"],
              "description": "L'unité de température"
            }
          },
          "required": ["location"]
        }
      }
    ]
  }
}
```

#### Structure d'un outil MCP

| Champ | Type | Description |
|-------|------|-------------|
| `Name` | string | Nom unique de l'outil (snake_case) |
| `Description` | string | Description de ce que fait l'outil |
| `Parameters` | object | Schéma JSON des paramètres (format OpenAPI) |

#### Exemples d'outils MCP

**1. Météo**
```json
{
  "Name": "get_weather",
  "Description": "Obtenir la météo actuelle",
  "Parameters": {
    "type": "object",
    "properties": {
      "location": { "type": "string", "description": "Ville" }
    },
    "required": ["location"]
  }
}
```

**2. Heure actuelle**
```json
{
  "Name": "get_time",
  "Description": "Obtenir l'heure actuelle dans un fuseau horaire",
  "Parameters": {
    "type": "object",
    "properties": {
      "timezone": { "type": "string", "description": "Fuseau horaire (ex: Europe/Paris)" }
    },
    "required": ["timezone"]
  }
}
```

**3. Calculatrice**
```json
{
  "Name": "calculate",
  "Description": "Effectuer un calcul mathématique",
  "Parameters": {
    "type": "object",
    "properties": {
      "expression": { "type": "string", "description": "Expression mathématique" }
    },
    "required": ["expression"]
  }
}
```

#### ⚠️ Note importante sur les MCPs

Les outils configurés sont **déclarés** à l'API OpenAI, mais vous devez **implémenter la logique d'exécution** dans votre code.

L'API vous enverra des événements `function_call` que vous devrez intercepter et traiter dans `OpenAIRealtimeService.cs`.

### 🎵 Configuration de la vitesse audio

L'application utilise **soundtouch-js** pour modifier la vitesse audio **sans changer la hauteur de la voix** :

- **Slider dans l'interface** : 0.5x à 2.0x
- **Buffer de démarrage** : 8 chunks (configurable dans `app.js` ligne 29)

Pour ajuster le buffer si vous avez des coupures :
```javascript
// Dans wwwroot/app.js
this.minBufferChunks = 10; // Augmenter pour plus de stabilité
```

### Exemple de personnalisation

Pour changer la voix :

```json
"Voice": "nova"
```

Voix disponibles : `alloy`, `echo`, `fable`, `onyx`, `nova`, `shimmer`

## 🌐 API REST pour les outils MCP

En plus de l'interface vocale, tous les outils MCP sont **accessibles via HTTP REST** !

### Endpoints disponibles

#### 1. Liste des outils
```http
GET /api/tools
```

**Réponse** :
```json
{
  "count": 2,
  "tools": [
    {
      "name": "get_weather",
      "description": "Obtenir la météo actuelle pour une ville donnée",
      "parameters": { ... }
    },
    {
      "name": "get_time",
      "description": "Obtenir l'heure actuelle dans un fuseau horaire donné",
      "parameters": { ... }
    }
  ]
}
```

#### 2. Exécuter un outil (générique)
```http
POST /api/tools/{toolName}
Content-Type: application/json

{
  "location": "Paris",
  "unit": "celsius"
}
```

**Réponse** :
```json
{
  "tool": "get_weather",
  "success": true,
  "result": {
    "location": "Paris",
    "temperature": 22,
    "unit": "°C",
    "condition": "Ensoleillé",
    "description": "Il fait actuellement 22°C à Paris avec un temps ensoleillé."
  },
  "executedAt": "2025-10-08T14:30:00Z"
}
```

#### 3. Raccourcis pratiques

**Météo** :
```http
GET /api/tools/weather/Paris?unit=celsius
```

**Heure** :
```http
GET /api/tools/time/Europe_Paris
```

### Exemples avec curl

```bash
# Liste des outils
curl http://localhost:5166/api/tools

# Météo à Paris
curl http://localhost:5166/api/tools/weather/Paris

# Météo en Fahrenheit
curl http://localhost:5166/api/tools/weather/London?unit=fahrenheit

# Heure à New York
curl http://localhost:5166/api/tools/time/America_New_York

# Exécution générique
curl -X POST http://localhost:5166/api/tools/get_weather \
  -H "Content-Type: application/json" \
  -d '{"location": "Tokyo", "unit": "celsius"}'
```

### Outils implémentés

#### 🌤️ `get_weather`
Obtenir la météo d'une ville (données simulées)

**Paramètres** :
- `location` (string, requis) : Nom de la ville
- `unit` (string, optionnel) : `celsius` ou `fahrenheit`

**Exemple** :
```bash
curl http://localhost:5166/api/tools/weather/Paris
```

#### 🕐 `get_time`
Obtenir l'heure actuelle dans un fuseau horaire

**Paramètres** :
- `timezone` (string, requis) : Fuseau horaire (ex: `Europe/Paris`, `America/New_York`)

**Exemple** :
```bash
curl http://localhost:5166/api/tools/time/Europe_Paris
```

#### 🧮 `calculate`
Effectuer un calcul mathématique simple

**Paramètres** :
- `expression` (string, requis) : Expression mathématique (ex: `2 + 2`, `10 * 5`)

**Exemple** :
```bash
curl -X POST http://localhost:5166/api/tools/calculate \
  -H "Content-Type: application/json" \
  -d '{"expression": "2 + 2"}'
```

### Ajouter vos propres outils

#### Option 1 : Outils HTTP (appels vers vos URLs locales ou externes)

Parfait pour **appeler vos propres APIs** sans modifier le code C# !

```json
{
  "Tools": [
    {
      "Name": "mon_outil_local",
      "Description": "Appelle mon API locale",
      "Type": "http",
      "Parameters": {
        "type": "object",
        "properties": {
          "param1": { "type": "string", "description": "Premier paramètre" }
        },
        "required": ["param1"]
      },
      "Http": {
        "Url": "http://localhost:3000/api/mon-endpoint",
        "Method": "POST",
        "Headers": {
          "Authorization": "Bearer mon-token",
          "X-Custom-Header": "valeur"
        }
      }
    }
  ]
}
```

**Exemples d'outils HTTP** :

**1. Appel GET avec paramètres dans l'URL** :
```json
{
  "Name": "get_user",
  "Type": "http",
  "Parameters": { ... },
  "Http": {
    "Url": "http://localhost:3000/users/{user_id}",
    "Method": "GET"
  }
}
```

**2. Appel POST avec body JSON** :
```json
{
  "Name": "create_task",
  "Type": "http",
  "Parameters": { ... },
  "Http": {
    "Url": "http://localhost:5000/tasks",
    "Method": "POST",
    "Headers": {
      "Content-Type": "application/json"
    }
  }
}
```

**3. Appel vers une API externe** :
```json
{
  "Name": "check_stock",
  "Type": "http",
  "Parameters": { ... },
  "Http": {
    "Url": "https://api.example.com/stock/{symbol}",
    "Method": "GET",
    "Headers": {
      "X-API-Key": "votre-clé-api"
    }
  }
}
```

#### Option 2 : Outils intégrés (builtin)

Pour des outils avec logique personnalisée en C# :

1. **Déclarez l'outil** dans `appsettings.json` avec `"Type": "builtin"`
2. **Implémentez la logique** dans `Services/Tools/ToolExecutorService.cs`
3. L'outil sera **automatiquement disponible** :
   - Via l'API REST (`POST /api/tools/{nom}`)
   - Via l'IA vocale (Marvin peut l'appeler)

#### Marvin peut maintenant appeler vos outils ! 🤖

Quand vous parlez à Marvin, il peut **automatiquement** :
- Appeler vos APIs locales (localhost)
- Utiliser les outils intégrés (météo, heure, calcul)
- Vous communiquer les résultats vocalement

**Exemple de conversation** :
- **Vous** : "Marvin, quelle heure est-il à Paris ?"
- **Marvin** : *Appelle automatiquement `get_time` avec `timezone: "Europe/Paris"`* 
- **Marvin** : "Il est actuellement 14h30 à Paris... Encore une question futile pour mon cerveau gigantesque..."

### 🛡️ Résilience des appels HTTP (Polly)

L'application utilise **Polly**, la bibliothèque de résilience .NET de référence, pour gérer les appels HTTP des outils avec des **politiques de retry** et **circuit breaker**.

#### Configuration par défaut

```json
{
  "OpenAI": {
    "Resilience": {
      "Retry": {
        "Enabled": true,
        "MaxRetryAttempts": 3,
        "InitialDelayMs": 100,
        "MaxDelayMs": 5000
      },
      "CircuitBreaker": {
        "Enabled": true,
        "FailureThreshold": 5,
        "BreakDurationSeconds": 30,
        "SamplingDurationSeconds": 60
      },
      "Timeout": {
        "Enabled": true,
        "TimeoutSeconds": 30
      }
    }
  }
}
```

#### 📋 Politiques de résilience

**1. Retry (Politique de réessai)** 🔄
- **Exponential backoff** : délai initial de 100ms, doublé à chaque tentative, jusqu'à 5 secondes max
- **3 tentatives** par défaut
- Déclenché automatiquement pour :
  - Erreurs HTTP transitoires (500, 502, 503, 504, 408)
  - Timeouts
  - Erreurs réseau

**2. Circuit Breaker (Disjoncteur)** ⚡
- **Protège vos APIs** contre les surcharges
- S'ouvre après **5 échecs** dans une fenêtre de **60 secondes**
- Reste ouvert pendant **30 secondes** (aucun appel n'est effectué)
- Passe en mode "half-open" pour tester si l'API est revenue
- **États** :
  - 🟢 **Closed (fermé)** : Fonctionnement normal
  - 🔴 **Open (ouvert)** : Toutes les requêtes échouent immédiatement
  - 🟡 **Half-Open (semi-ouvert)** : Test si le service est revenu

**3. Timeout (Délai d'expiration)** ⏱️
- **30 secondes** par défaut par requête HTTP
- Évite les appels qui bloquent indéfiniment

#### Architecture des politiques (ordre d'exécution)

```
Circuit Breaker (outermost)
    ↓
  Retry Policy (middle)
    ↓
  Timeout Policy (innermost)
    ↓
HTTP Request
```

1. Le **Circuit Breaker** vérifie s'il doit laisser passer la requête
2. La **Retry Policy** gère les échecs et réessaye si nécessaire
3. Le **Timeout** limite la durée de chaque tentative
4. La requête HTTP est finalement exécutée

#### Logs de Polly

Les politiques génèrent des logs détaillés :

```
[Polly Retry] Retry 1/3 after 100ms. Reason: 503 Service Unavailable
[Polly Retry] Retry 2/3 after 200ms. Reason: Timeout
[Polly Circuit Breaker] Circuit opened for 30s. Reason: 500 Internal Server Error
[Polly Circuit Breaker] Circuit half-open (testing)
[Polly Circuit Breaker] Circuit reset (closed)
[Polly Timeout] Request timed out after 30s
```

#### Personnalisation

Vous pouvez **désactiver** ou **ajuster** chaque politique individuellement :

**Désactiver le retry** :
```json
{
  "Resilience": {
    "Retry": {
      "Enabled": false
    }
  }
}
```

**Augmenter le nombre de tentatives** :
```json
{
  "Resilience": {
    "Retry": {
      "Enabled": true,
      "MaxRetryAttempts": 5,
      "InitialDelayMs": 200,
      "MaxDelayMs": 10000
    }
  }
}
```

**Circuit Breaker plus agressif** :
```json
{
  "Resilience": {
    "CircuitBreaker": {
      "Enabled": true,
      "FailureThreshold": 3,
      "BreakDurationSeconds": 60,
      "SamplingDurationSeconds": 30
    }
  }
}
```

#### Cas d'usage

**APIs externes instables** :
```json
{
  "Retry": { "MaxRetryAttempts": 5 },
  "CircuitBreaker": { "FailureThreshold": 10 }
}
```

**Appels locaux rapides** :
```json
{
  "Retry": { "MaxRetryAttempts": 2, "MaxDelayMs": 1000 },
  "Timeout": { "TimeoutSeconds": 5 }
}
```

**Pas de retry (tests uniquement)** :
```json
{
  "Retry": { "Enabled": false },
  "CircuitBreaker": { "Enabled": false }
}
```

#### Avantages

- ✅ **Fiabilité accrue** : Gère automatiquement les erreurs transitoires
- ✅ **Protection contre les surcharges** : Le circuit breaker protège vos APIs
- ✅ **Logs détaillés** : Visibilité complète sur les échecs et réessais
- ✅ **Configuration sans code** : Tout se configure dans `appsettings.json`
- ✅ **Standard .NET** : Polly est la bibliothèque de résilience de référence

#### 📋 Exemples de configuration prêts à l'emploi

Le projet inclut plusieurs fichiers d'exemple pour différents cas d'usage :

**1. APIs externes instables** (`appsettings.example-external-unstable.json`)
```json
{
  "Resilience": {
    "Retry": { "MaxRetryAttempts": 5, "InitialDelayMs": 500, "MaxDelayMs": 15000 },
    "CircuitBreaker": { "FailureThreshold": 10, "BreakDurationSeconds": 60 },
    "Timeout": { "TimeoutSeconds": 60 }
  }
}
```

**2. Appels locaux rapides** (`appsettings.example-local-fast.json`)
```json
{
  "Resilience": {
    "Retry": { "MaxRetryAttempts": 2, "InitialDelayMs": 50, "MaxDelayMs": 500 },
    "CircuitBreaker": { "FailureThreshold": 3, "BreakDurationSeconds": 10 },
    "Timeout": { "TimeoutSeconds": 5 }
  }
}
```

**3. Tests sans résilience** (`appsettings.example-no-resilience.json`)
```json
{
  "Resilience": {
    "Retry": { "Enabled": false },
    "CircuitBreaker": { "Enabled": false },
    "Timeout": { "Enabled": false }
  }
}
```

**4. Production équilibrée** (`appsettings.example-production.json`)
```json
{
  "Resilience": {
    "Retry": { "MaxRetryAttempts": 4, "InitialDelayMs": 200, "MaxDelayMs": 10000 },
    "CircuitBreaker": { "FailureThreshold": 7, "BreakDurationSeconds": 45 },
    "Timeout": { "TimeoutSeconds": 30 }
  }
}
```

**Utilisation** :
```bash
# Copier un exemple
cp appsettings.example-production.json appsettings.json

# Ou fusionner la section "Resilience" dans votre config existante
```

📖 **Guide complet** : Consultez `Prompts/ResilienceGuide.md` pour :
- Comprendre chaque paramètre
- Choisir la bonne configuration
- Éviter les pièges courants
- Tableaux de décision rapide

## 🎮 Utilisation

### Mode vocal

1. **Ouvrez l'application** dans votre navigateur
2. **Sélectionnez un microphone** dans la liste déroulante
3. **Cliquez sur "Démarrer l'écoute"** (le bouton devient rouge)
4. **Parlez naturellement** - l'IA vous répondra automatiquement
5. **Les transcriptions** apparaissent en temps réel dans le chat
6. **Cliquez sur "Arrêter l'écoute"** pour terminer

### Indicateurs visuels

- 🟢 **Vert** : Prêt à écouter
- 🔴 **Rouge** : En écoute active
- 💬 **Messages bleus** : Vos paroles
- 💬 **Messages gris** : Réponses de l'IA

## 📁 Structure du projet

```
chatrealtime/
├── Configuration/
│   └── OpenAISettings.cs          # Configuration OpenAI
├── Models/
│   ├── RealtimeEvents.cs          # Événements API Realtime
│   └── ClientMessage.cs           # Messages WebSocket
├── Services/
│   ├── OpenAIRealtimeService.cs   # Service connexion OpenAI
│   └── RealtimeWebSocketHandler.cs # Gestion WebSocket client
├── wwwroot/
│   ├── index.html                 # Interface utilisateur
│   ├── styles.css                 # Styles CSS
│   └── app.js                     # Logique JavaScript
├── Program.cs                     # Point d'entrée
├── appsettings.json              # Configuration
└── chatrealtime.csproj           # Fichier projet
```

## 🔧 Spécifications techniques

### Audio

- **Format** : PCM16 (16-bit linear PCM)
- **Sample Rate** : 24 000 Hz
- **Canaux** : Mono (1 canal)
- **Taille buffer** : 4096 samples

### WebSocket

- **Endpoint client** : `ws(s)://host/ws/realtime`
- **Keep-alive** : 120 secondes
- **Format messages** : JSON

### Messages WebSocket

#### Client → Serveur

```json
{
  "type": "audio",
  "audio": "base64_encoded_pcm16_data"
}
```

#### Serveur → Client

```json
{
  "type": "audio|transcript|status|error",
  "audio": "base64_audio",
  "transcript": "texte transcrit",
  "role": "user|assistant",
  "status": "message de statut"
}
```

## 🐛 Résolution des problèmes

### "OpenAI API Key is not configured"

➡️ Vérifiez que vous avez bien configuré votre clé API dans `appsettings.json`

### "Impossible d'accéder au microphone"

➡️ Autorisez l'accès au microphone dans les paramètres de votre navigateur

### "Failed to connect to OpenAI"

➡️ Vérifiez :
- Votre clé API est valide
- Vous avez accès à l'API Realtime
- Votre connexion Internet fonctionne

### Pas de son dans les réponses

➡️ Vérifiez :
- Le volume de votre navigateur
- Les permissions audio du navigateur
- Que vous utilisez Chrome ou Edge

## 📝 Notes importantes

- **Coût** : L'API OpenAI Realtime est facturée à l'usage. Surveillez votre consommation.
- **Navigateurs** : Chrome et Edge recommandés pour une meilleure compatibilité
- **Sécurité** : Ne commitez JAMAIS votre clé API dans un repository public
- **Production** : Pour la production, utilisez des variables d'environnement pour stocker la clé API

## 🔒 Sécurité

Pour la production, utilisez des variables d'environnement :

```bash
export OpenAI__ApiKey="sk-proj-xxxxx"
dotnet run
```

Ou configurez dans `appsettings.Development.json` (non versionné) :

```json
{
  "OpenAI": {
    "ApiKey": "votre-clé-ici"
  }
}
```

## 📚 Ressources

- [Documentation OpenAI Realtime API](https://platform.openai.com/docs/guides/realtime)
- [ASP.NET Core WebSockets](https://learn.microsoft.com/aspnet/core/fundamentals/websockets)
- [Web Audio API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Audio_API)

## 📄 Licence

Ce projet est fourni à des fins éducatives et de démonstration.
