# 🚀 Guide de Démarrage Rapide

## Étapes pour lancer l'application

### 1️⃣ Configurer votre clé API OpenAI

**Option A : Fichier de développement (recommandé)**

Éditez `appsettings.Development.json` et remplacez la clé :

```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-VOTRE_CLE_ICI"
  }
}
```

**Option B : Variable d'environnement**

```bash
export OpenAI__ApiKey="sk-proj-VOTRE_CLE_ICI"
```

### 2️⃣ Lancer l'application

```bash
dotnet run
```

### 3️⃣ Ouvrir dans le navigateur

Ouvrez **Chrome** ou **Edge** et allez sur :
- http://localhost:5166 (HTTP)

### 4️⃣ Utiliser l'application

1. **Autorisez** l'accès au microphone quand demandé
2. **Sélectionnez** votre microphone dans la liste
3. **Cliquez** sur "Démarrer l'écoute"
4. **Parlez** naturellement !

## ✅ Vérification

Si tout fonctionne correctement, vous devriez voir :

1. ✅ Message "Connecté à OpenAI"
2. ✅ Bouton rouge "Arrêter l'écoute"
3. ✅ Indicateur "En écoute..." en haut
4. ✅ Vos paroles transcrites apparaissent en bleu
5. ✅ L'IA répond avec audio + texte en gris

## ⚠️ Problèmes courants

### "OpenAI API Key is not configured"
→ Vous n'avez pas configuré votre clé API dans `appsettings.Development.json`

### "Impossible d'accéder au microphone"
→ Autorisez l'accès dans les paramètres du navigateur

### "Failed to connect to OpenAI"
→ Vérifiez que votre clé API est valide et que vous avez accès à l'API Realtime

### Pas de son
→ Vérifiez le volume de votre navigateur et que vous utilisez Chrome/Edge

## 📝 Personnalisation rapide

### Changer la voix de l'IA

Dans `appsettings.json`, changez :
```json
"Voice": "nova"
```

Voix disponibles : `alloy`, `echo`, `fable`, `onyx`, `nova`, `shimmer`

### Changer le comportement de l'IA

Modifiez `Instructions` dans `appsettings.json` :
```json
"Instructions": "Vous êtes un expert en cuisine. Répondez avec des conseils culinaires."
```

### Changer la sensibilité de détection vocale

Ajustez `Threshold` (0.0 = très sensible, 1.0 = peu sensible) :
```json
"TurnDetection": {
  "Threshold": 0.3
}
```

## 🎯 Prêt !

Vous êtes maintenant prêt à avoir des conversations vocales en temps réel avec GPT ! 🎉

Pour plus de détails, consultez le **README.md**.
