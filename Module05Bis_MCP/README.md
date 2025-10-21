# Module 5 - Model Context Protocol (MCP)

## Prérequis

Pour chaque séance de cours, lancez l'outils client de test de MCPs d'Anthropic :

```bash
docker run --rm -p 6274:6274 -e HOST=0.0.0.0 -e ALLOWED_ORIGINS=http://127.0.0.1:6274 -p 6277:6277 --network bridge ghcr.io/modelcontextprotocol/inspector:latest
```

Puis ouvrez votre navigateur à l'adresse donnée sur la console en remplaçant `http://0.0.0.0` par `http://127.0.0.1`.

Quand vous essayerez de vous connecter à **vos** MCPs, utilisez l'URL suivante : `http://host.docker.internal:<VotrePortIci>/mcp`.

## Lancez la démo du cours

