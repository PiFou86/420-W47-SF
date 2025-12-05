# Guide de Configuration de la Résilience HTTP (Polly)

Ce guide vous aide à choisir la bonne configuration Polly selon votre cas d'usage.

## 🎯 Cas d'usage courants

### 1. APIs externes instables ou lentes

**Fichier d'exemple** : `appsettings.example-external-unstable.json`

**Scénario** :
- Appels vers des APIs tierces (météo, stock, paiement, etc.)
- APIs connues pour avoir des temps de réponse variables
- Services externes qui peuvent avoir des interruptions temporaires

**Configuration recommandée** :
```json
{
  "Resilience": {
    "Retry": {
      "Enabled": true,
      "MaxRetryAttempts": 5,           // Plus de tentatives
      "InitialDelayMs": 500,            // Délai initial plus long
      "MaxDelayMs": 15000               // Jusqu'à 15 secondes entre retries
    },
    "CircuitBreaker": {
      "Enabled": true,
      "FailureThreshold": 10,           // Tolérance plus élevée
      "BreakDurationSeconds": 60,       // Pause de 1 minute
      "SamplingDurationSeconds": 120    // Fenêtre de 2 minutes
    },
    "Timeout": {
      "Enabled": true,
      "TimeoutSeconds": 60              // Timeout généreux
    }
  }
}
```

**Pourquoi ?**
- ✅ Donne plusieurs chances à l'API de répondre
- ✅ Laisse le temps aux services de se stabiliser (exponential backoff)
- ✅ Circuit breaker tolérant pour ne pas bloquer trop vite
- ✅ Timeout long pour les opérations complexes

---

### 2. Appels locaux rapides (localhost, LAN)

**Fichier d'exemple** : `appsettings.example-local-fast.json`

**Scénario** :
- Appels vers `localhost` (Redis, PostgreSQL, services internes)
- Microservices dans le même réseau local
- APIs de cache ou de recherche rapide

**Configuration recommandée** :
```json
{
  "Resilience": {
    "Retry": {
      "Enabled": true,
      "MaxRetryAttempts": 2,            // Peu de retries
      "InitialDelayMs": 50,             // Délai minimal
      "MaxDelayMs": 500                 // Max 500ms
    },
    "CircuitBreaker": {
      "Enabled": true,
      "FailureThreshold": 3,            // Ouvre rapidement
      "BreakDurationSeconds": 10,       // Pause courte
      "SamplingDurationSeconds": 30     // Fenêtre de 30 secondes
    },
    "Timeout": {
      "Enabled": true,
      "TimeoutSeconds": 5               // Timeout court
    }
  }
}
```

**Pourquoi ?**
- ✅ Réponse rapide attendue (timeout court)
- ✅ Si ça échoue localement, c'est probablement grave (peu de retries)
- ✅ Circuit breaker réactif pour détecter rapidement les problèmes
- ✅ Pas d'attente excessive pour l'utilisateur

---

### 3. Tests et développement (sans résilience)

**Fichier d'exemple** : `appsettings.example-no-resilience.json`

**Scénario** :
- Tests unitaires / intégration
- Développement local
- Debugging d'APIs
- Vérification de la logique sans masquer les erreurs

**Configuration recommandée** :
```json
{
  "Resilience": {
    "Retry": {
      "Enabled": false                  // Pas de retry
    },
    "CircuitBreaker": {
      "Enabled": false                  // Pas de circuit breaker
    },
    "Timeout": {
      "Enabled": false                  // Pas de timeout
    }
  }
}
```

**Pourquoi ?**
- ✅ Erreurs visibles immédiatement
- ✅ Pas de retry qui masque les bugs
- ✅ Temps de debug réduit
- ⚠️ **NE JAMAIS utiliser en production !**

---

### 4. Production équilibrée

**Fichier d'exemple** : `appsettings.example-production.json`

**Scénario** :
- Environnement de production
- Mix d'APIs externes fiables et services internes
- Besoin de fiabilité et de performance

**Configuration recommandée** :
```json
{
  "Resilience": {
    "Retry": {
      "Enabled": true,
      "MaxRetryAttempts": 4,            // Équilibre
      "InitialDelayMs": 200,            // Délai raisonnable
      "MaxDelayMs": 10000               // Max 10 secondes
    },
    "CircuitBreaker": {
      "Enabled": true,
      "FailureThreshold": 7,            // Tolérance modérée
      "BreakDurationSeconds": 45,       // Pause de 45 secondes
      "SamplingDurationSeconds": 90     // Fenêtre de 1.5 minutes
    },
    "Timeout": {
      "Enabled": true,
      "TimeoutSeconds": 30              // 30 secondes standard
    }
  }
}
```

**Pourquoi ?**
- ✅ Configuration équilibrée pour la plupart des cas
- ✅ Protège contre les erreurs transitoires
- ✅ Performance acceptable pour l'utilisateur
- ✅ Logs détaillés pour le monitoring

---

## 📊 Tableau de décision rapide

| Cas d'usage | Retry | Initial Delay | Max Delay | Circuit Breaker Threshold | Break Duration | Timeout |
|-------------|-------|---------------|-----------|---------------------------|----------------|---------|
| **API externe instable** | 5 | 500ms | 15s | 10 échecs | 60s | 60s |
| **Localhost / LAN** | 2 | 50ms | 500ms | 3 échecs | 10s | 5s |
| **Tests / Debug** | ❌ Désactivé | - | - | ❌ Désactivé | - | ❌ Désactivé |
| **Production** | 4 | 200ms | 10s | 7 échecs | 45s | 30s |

---

## 🔍 Comprendre les valeurs

### Retry Policy

- **MaxRetryAttempts** : Nombre de tentatives après le premier échec
  - `2` = Total de 3 appels (1 initial + 2 retries)
  - `5` = Total de 6 appels (1 initial + 5 retries)

- **InitialDelayMs** : Délai avant le premier retry
  - `50ms` = Très rapide (local)
  - `200ms` = Standard
  - `500ms` = Tolérant (externe)

- **MaxDelayMs** : Délai maximum (exponential backoff plafonné)
  - Formula: `min(InitialDelay * 2^(retry-1), MaxDelay)`
  - Exemple avec `InitialDelay=100ms`, `MaxDelay=5000ms` :
    - Retry 1: 100ms
    - Retry 2: 200ms
    - Retry 3: 400ms
    - Retry 4: 800ms
    - Retry 5+: 5000ms (plafonné)

### Circuit Breaker

- **FailureThreshold** : Nombre minimum d'appels dans la fenêtre
  - `3` = Réactif (local)
  - `5-7` = Standard
  - `10+` = Tolérant (externe)

- **BreakDurationSeconds** : Durée pendant laquelle le circuit reste ouvert
  - `10s` = Court (local)
  - `30-45s` = Standard
  - `60s+` = Long (externe instable)

- **SamplingDurationSeconds** : Durée de la fenêtre d'observation
  - Le circuit s'ouvre si `>50%` des appels échouent dans cette fenêtre
  - `30s` = Fenêtre courte (local)
  - `60-90s` = Standard
  - `120s+` = Fenêtre longue (externe)

### Timeout

- **TimeoutSeconds** : Délai avant d'annuler la requête
  - `5s` = Local rapide
  - `30s` = Standard
  - `60s+` = Opérations longues (upload, batch)

---

## 🚀 Comment utiliser ces exemples

### Option 1 : Copier directement
```bash
# Pour des APIs externes instables
cp appsettings.example-external-unstable.json appsettings.json

# Pour des appels locaux rapides
cp appsettings.example-local-fast.json appsettings.json

# Pour la production
cp appsettings.example-production.json appsettings.json
```

### Option 2 : Fusionner avec votre config existante
Copiez uniquement la section `"Resilience"` de l'exemple dans votre `appsettings.json`

---

## 📈 Monitoring et Logs

Polly génère des logs détaillés. Recherchez ces patterns :

```bash
# Retries
[Polly Retry] Retry 1/3 after 100ms. Reason: 503 Service Unavailable

# Circuit ouvert (service down)
[Polly Circuit Breaker] Circuit opened for 30s. Reason: 500 Internal Server Error

# Circuit fermé (service récupéré)
[Polly Circuit Breaker] Circuit reset (closed)

# Timeout
[Polly Timeout] Request timed out after 30s
```

---

## ⚠️ Pièges courants à éviter

### ❌ Trop de retries + timeout trop long
```json
{
  "MaxRetryAttempts": 10,
  "TimeoutSeconds": 60
}
```
**Problème** : L'utilisateur attend jusqu'à 10 minutes (10 × 60s) !

**Solution** : Réduire les retries OU le timeout
```json
{
  "MaxRetryAttempts": 3,
  "TimeoutSeconds": 10
}
```

---

### ❌ Circuit breaker trop sensible
```json
{
  "FailureThreshold": 1,
  "BreakDurationSeconds": 300
}
```
**Problème** : Un seul échec bloque pendant 5 minutes !

**Solution** : Augmenter le seuil
```json
{
  "FailureThreshold": 5,
  "BreakDurationSeconds": 30
}
```

---

### ❌ Retry sans exponential backoff
```json
{
  "InitialDelayMs": 100,
  "MaxDelayMs": 100
}
```
**Problème** : Tous les retries à 100ms → surcharge du serveur

**Solution** : Laisser le backoff exponentiel
```json
{
  "InitialDelayMs": 100,
  "MaxDelayMs": 5000
}
```

---

## 🎓 Ressources supplémentaires

- [Documentation Polly](https://github.com/App-vNext/Polly)
- [Polly & HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- [Circuit Breaker Pattern](https://martinfowler.com/bliki/CircuitBreaker.html)

---

**Besoin d'aide ?** Consultez les logs de Polly ou créez une configuration personnalisée !
