# Prompts Système

Ce dossier contient les fichiers de prompts système pour configurer la personnalité de votre assistant vocal.

## Utilisation

Pour changer le prompt système, modifiez le paramètre `SystemPromptFile` dans `appsettings.json` :

```json
{
  "OpenAI": {
    "SystemPromptFile": "Prompts/VotrePrompt.md"
  }
}
```

## Prompts disponibles

### Marvin.md
Personnalité de Marvin, le robot paranoïde android du Guide du voyageur galactique (H2G2).
- Intelligent mais déprimé
- Sarcastique et ironique
- Se plaint constamment
- Parfait pour une expérience humoristique et décalée

## Créer votre propre prompt

1. Créez un nouveau fichier `.md` dans ce dossier
2. Décrivez la personnalité, le style et les consignes pour l'assistant
3. Mettez à jour `appsettings.json` pour pointer vers votre nouveau fichier
4. Redémarrez l'application

## Exemple de structure

```markdown
# Nom du personnage

Description brève du personnage.

## Caractéristiques

- Liste des traits de personnalité
- Comportements spécifiques

## Style de réponse

- Ton à utiliser
- Façon de s'exprimer

## Exemples de phrases typiques

- "Exemple 1"
- "Exemple 2"

## Consignes importantes

- Règles à respecter
- Limitations ou recommandations
```

## Voix disponibles

Vous pouvez changer la voix dans `appsettings.json`. Voix disponibles :
- `alloy` - Neutre et équilibrée
- `echo` - Masculine et claire
- `fable` - Expressive et britannique
- `onyx` - Profonde et masculine
- `nova` - Douce et agréable
- `shimmer` - Énergique et féminine

Pour Marvin, nous recommandons `echo` pour une voix plus robotique et masculine.
