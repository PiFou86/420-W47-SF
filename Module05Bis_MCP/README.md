# Module 5 - Model Context Protocol (MCP)

Dans ce module, nous allons découvrir le protocole Model Context Protocol (MCP) développé par Anthropic. Ce protocole permet de créer des serveurs qui exposent des modèles de langage et des outils associés à ces modèles. Nous allons apprendre à créer un MCP simple, puis nous allons créer un MCP qui se connecte à l'API publique de Spotify pour récupérer des informations sur des artistes, leurs albums et les titres de ces albums.

## Prérequis

Pour chaque séance de cours, lancez l'outils client de test de MCPs d'Anthropic :

```bash
docker run --rm -p 6274:6274 -e HOST=0.0.0.0 -e ALLOWED_ORIGINS=http://127.0.0.1:6274,http://localhost:6274 -p 6277:6277 --network bridge ghcr.io/modelcontextprotocol/inspector:latest
```

***Si vous avez l'erreur "docker: Error message from daemon: error from registry: denied", il faut vous authenfier sur le dépot d'images de GitHub en tapant la commande `docker login ghcr.io`. Comme nom d'utilisateur, fournissez votre nom de compte GibHub, comme mot de passe, allez créer un PAT (Personnel Access Token) dans votre compte GitHub.***

Le conteneur Docker va démarrer et afficher des informations sur la console. Il écoute sur toutes ses interfaces (`0.0.0.0`) sur le port `6274`. Vous pouvez vous connecter avec votre navigateur web aux adresses `http://localhost:6274` ou `http://127.0.0.1:6274`. J'ai mis l'interface bridge : si vous voulez y accéder depuis une autre machine, vous devez utiliser l'adresse IP de votre machine hôte et surtout ajouter cette adresse IP dans la variable d'environnement `ALLOWED_ORIGINS`.

Le site se protège avec un token d'authentification. Le token est affiché sur la console au démarrage du conteneur Docker. Pour vous connecter sur le client MCP, copiez l'adresse URL affichée sur la console, par exemple :
`http://0.0.0.0:6274/?MCP_PROXY_AUTH_TOKEN=c537f836a9cc90e4c06af81a54e1a1faca090859b63595747ad3ac78f28aa936` et remplacez `http://0.0.0.0` par `http://localhost`, ce qui donne dans mon cas : `http://localhost:6274/?MCP_PROXY_AUTH_TOKEN=c537f836a9cc90e4c06af81a54e1a1faca090859b63595747ad3ac78f28aa936`.

Quand vous essayerez de vous connecter à **vos** MCPs, utilisez la configuration suivante :

- Transport Type : `Streamable HTTP`
- URL : `http://host.docker.internal:<VotrePortIci>/mcp` (Dans le cas de la démonstration du cours, le MCP est exposé sur le port `5175`, donc l'URL sera `http://host.docker.internal:5175/mcp`)

## Lancez la démonstration du cours

- Lancez votre client `MCP Inspector` (le conteneur Docker comme décrit plus haut)
- Si ce n'est pas fait, téléchargez la solution de démonstration du cours `mcpservertest01` présente à la racine de ce répertoire
- À partir d'un terminal, allez dans le répertoire `mcpservertest01` et lancez la commande suivante pour démarrer le serveur MCP :

```bash
dotnet run
```

- Une fois le serveur démarré, ouvrez votre navigateur web et allez à l'adresse `http://localhost:6274/?MCP_PROXY_AUTH_TOKEN=...` (avec le token affiché sur la console du serveur)
- Configurez les informations de connexion au MCP comme indiqué plus haut
- Testez les différentes opérations offertes par le MCP :
  - Connexion
  - Ressources : récupération de la liste et affichage des détails
  - Prompts : récupération de la liste et affichage d'un prompt avec paramètres
  - Tools : récupération de la liste et exécution d'un outil s
  - Ping : ping du serveur et validation dans l'historique

Durant ce test, explorez votre nouvel outil.

## Exercice 1 - Création d'un MCP simple

Votre mission est de créer un MCP simple qui offre les fonctionnalités suivantes :

- Un outil qui permet de connaitre la date et l'heure actuelle
- Quatre outils de manipulation de chaînes de caractères :
  - Inverser une chaîne de caractères
  - Mettre en majuscule une chaîne de caractères
  - Mettre en minuscule une chaîne de caractères
  - Compter le nombre de caractères dans une chaîne de caractères
- Une ressource qui contient les noms de vos joueurs sportifs préférés ou de vos chanteurs préférés
- Répond aux pings du client MCP

Pour réaliser cet exercice, vous utiliser la solution `mcpservertest01` et la modifier. Pour réaliser l'exercice, écrivez les outils un par un et testez-les avant de passer à l'outil suivant.

Indications pour ajouter un outil en modifiant la classe `ToolsHandler` dans le fichier `ToolsHandler.cs` :

- Modifiez la méthode `HandleToolsListAsync` pour déclarer un outil en suivant la déclaration de l'outil en exemple
- Modifiez la méthode `HandleToolsCallAsync` pour implémenter la logique de l'outil en suivant l'implémentation de l'outil en exemple
- Testez chaque outil avec le client MCP Inspector avant de passer à l'outil suivant

## Exercice 2 - Connectons nous à l'API de Spotify

Dans cet exercice, nous allons créer un MCP qui se connecte à l'API publique de Spotify pour récupérer des informations sur des artistes, leurs albums et les titres de ces albums.

### Exercice 2.1 - Exploration de l'API de Spotify

- Allez sur le site de Spotify pour développeurs : [https://developer.spotify.com/dashboard/applications](https://developer.spotify.com/dashboard/applications)
- Créez une nouvelle application
- Récupérez votre Client ID et votre Client Secret
- Reproduire les étapes de l'exemple de requêtes Spotify ci-dessous pour valider que vous pouvez bien récupérer un token d'accès et faire des requêtes à l'API de Spotify et que vous comprenez le fonctionnement de l'API REST de Spotify. Si curl n'est pas installé sur votre machine, vous pouvez utiliser Postman ou un autre outil similaire.

Exemple de séquence d'appels à l'API de Spotify pour récupérer un token d'accès et faire des requêtes :

Création du token d'accès (Bearer Token) :

```bash
pfleon@pflmb > ~ > curl -X POST "https://accounts.spotify.com/api/token" \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=client_credentials&client_id=fb58ee<...>d4c5aec&client_secret=cfd0b7<...>f00a79ef"
{"access_token":"BQBSG5<...>YwCQ","token_type":"Bearer","expires_in":3600}                               
```

(-d est l'abréviation de --data)

`client_id` et `client_secret` sont à remplacer par vos propres valeurs qui se trouvent dans le tableau de bord de votre application Spotify (Ce ne sont pas non plus mes valeurs, elles sont remplacées par du aléatoire raccourci).

Recherche d'un artiste par son nom (ici "brassens") :

```bash
pfleon@pflmb > ~ > curl --request GET \
  --url 'https://api.spotify.com/v1/search?q=brassens&type=artist' \
  --header 'Authorization: Bearer BQBSG5<...>YwCQ'
{"artists":{"href":"https://api.spotify.com/v1/search?offset=0&limit=20&query=brassens&type=artist","limit":20,"next":"https://api.spotify.com/v1/search?offset=20&limit=20&query=brassens&type=artist","offset":0,"previous":null,"total":881,"items":[{"external_urls":{"spotify":"https://open.spotify.com/artist/5UWyW1PcEM8coxeqg3RIHr"},"followers":{"href":null,"total":544047},"genres":["chanson","variété française","french house"],"href":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr","id":"5UWyW1PcEM8coxeqg3RIHr","images":[{"url":"https://i.scdn.co/image/ab6761610000e5ebaa16370f435f9f9a99813417","height":640,"width":640},{"url":"https://i.scdn.co/image/ab67616100005174aa16370f435f9f9a99813417","height":320,"width":320},{"url":"https://i.scdn.co/image/ab6761610000f178aa16370f435f9f9a99813417","height":160,"width":160}],"name":"Georges Brassens","popularity":50,"type":"artist","uri":"spotify:artist:5UWyW1PcEM8coxeqg3RIHr"},{"external_urls":{"spotify":"https://open.spotify.com/artist/4RN2vlFWepLa46qQIU2PHs"},"followers":{"href":null,"total":889420},"genres":["chanson","variété française"],"href":"https://api.spotify.com/v1/artists/4RN2vlFWepLa46qQIU2PHs","id":"4RN2vlFWepLa46qQIU2PHs","images":[{"url":"https://i.scdn.co/image/ab6761610000e5eb4dfecaf883652dc5600ba055","height":640,"width":640},{"url":"https://i.scdn.co/image/ab676161000051744dfecaf883652dc5600ba055","height":320,"width":320},{"url":"https://i.scdn.co/image/ab6761610000f1784dfecaf883652dc5600ba055","height":160,"width":160}],"name":"Jacques Brel","popularity":53,"type":"artist","uri":"spotify:artist:4RN2vlFWepLa46qQIU2PHs"},<...>]}}
```

On voit que la requête utilise le paramètre `q` pour la recherche et `type=artist` pour indiquer que l'on cherche des artistes. Dans la réponse en JSON (Coupée ici pour plus de clarté), on retrouve l'identifiant de Georges Brassens : `5UWyW1PcEM8coxeqg3RIHr` ainsi que d'autres artistes. On remarque aussi un attribut `total` qui indique le nombre total de résultats (881 ici) ainsi que des liens de pagination (`next`). En observant l'URL du lien `next`, on remarque qu'il y a un paramètre `offset` qui permet la pagination du résultat.

Nous allons maintenant récupérer les albums de Georges Brassens à partir de son identifiant. L'identifiant est passé en paramètre dans l'URL :

```bash
pfleon@pflmb > ~ > curl --request GET \
  --url 'https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr/albums?include_groups=album' \
  --header 'Authorization: Bearer BQBSG5<...>YwCQ'
{"href":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr/albums?offset=0&limit=20&include_groups=album","limit":20,"next":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr/albums?offset=20&limit=20&include_groups=album","offset":0,"previous":null,"total":34,"items":[{"album_type":"album","total_tracks":20,"available_markets":["AR","AU","AT","BE","BO","BR","BG","CA","CL","CO","CR","CY","CZ","DK","DO","DE","EC","EE","SV","FI","FR","GR","GT","HN","HK","HU","IS","IE","IT","LV","LT","LU","MY","MT","MX","NL","NZ","NI","NO","PA","PY","PE","PH","PL","PT","SG","SK","ES","SE","CH","TW","TR","UY","GB","AD","LI","MC","ID","JP","TH","VN","RO","IL","ZA","SA","AE","BH","QA","OM","KW","EG","MA","DZ","TN","LB","JO","PS","IN","BY","KZ","MD","UA","AL","BA","HR","ME","MK","RS","SI","KR","BD","PK","LK","GH","KE","NG","TZ","UG","AG","AM","BS","BB","BZ","BT","BW","BF","CV","CW","DM","FJ","GM","GE","GD","GW","GY","HT","JM","KI","LS","LR","MW","MV","ML","MH","FM","NA","NR","NE","PW","PG","PR","WS","SM","ST","SN","SC","SL","SB","KN","LC","VC","SR","TL","TO","TT","TV","VU","AZ","BN","BI","KH","CM","TD","KM","GQ","SZ","GA","GN","KG","LA","MO","MR","MN","NP","RW","TG","UZ","ZW","BJ","MG","MU","MZ","AO","CI","DJ","ZM","CD","CG","IQ","LY","TJ","VE","ET","XK"],"external_urls":{"spotify":"https://open.spotify.com/album/7ElaerikszHQjNlf0AOKep"},"href":"https://api.spotify.com/v1/albums/7ElaerikszHQjNlf0AOKep","id":"7ElaerikszHQjNlf0AOKep","images":[{"url":"https://i.scdn.co/image/ab67616d0000b273470fe4c6b09f00a1895db256","height":640,"width":640},{"url":"https://i.scdn.co/image/ab67616d00001e02470fe4c6b09f00a1895db256","height":300,"width":300},{"url":"https://i.scdn.co/image/ab67616d00004851470fe4c6b09f00a1895db256","height":64,"width":64}],"name":"Brassens!","release_date":"2024-10-11","release_date_precision":"day","type":"album","uri":"spotify:album:7ElaerikszHQjNlf0AOKep","artists":[{"external_urls":{"spotify":"https://open.spotify.com/artist/5UWyW1PcEM8coxeqg3RIHr"},"href":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr","id":"5UWyW1PcEM8coxeqg3RIHr","name":"Georges Brassens","type":"artist","uri":"spotify:artist:5UWyW1PcEM8coxeqg3RIHr"}],"album_group":"album"},{"album_type":"album","total_tracks":11,"available_markets":["AR","AU","AT","BE","BO","BR","BG","CA","CL","CO","CR","CY","CZ","DK","DO","DE","EC","EE","SV","FI","FR","GR","GT","HN","HK","HU","IS","IE","IT","LV","LT","LU","MY","MT","MX","NL","NZ","NI","NO","PA","PY","PE","PH","PL","PT","SG","SK","ES","SE","CH","TW","TR","UY","US","GB","AD","LI","MC","ID","JP","TH","VN","RO","IL","ZA","SA","AE","BH","QA","OM","KW","EG","MA","DZ","TN","LB","JO","PS","IN","BY","KZ","MD","UA","AL","BA","HR","ME","MK","RS","SI","KR","BD","PK","LK","GH","KE","NG","TZ","UG","AG","AM","BS","BB","BZ","BT","BW","BF","CV","CW","DM","FJ","GM","GE","GD","GW","GY","HT","JM","KI","LS","LR","MW","MV","ML","MH","FM","NA","NR","NE","PW","PG","PR","WS","SM","ST","SN","SC","SL","SB","KN","LC","VC","SR","TL","TO","TT","TV","VU","AZ","BN","BI","KH","CM","TD","KM","GQ","SZ","GA","GN","KG","LA","MO","MR","MN","NP","RW","TG","UZ","ZW","BJ","MG","MU","MZ","AO","CI","DJ","ZM","CD","CG","IQ","LY","TJ","VE","ET","XK"],"external_urls":{"spotify":"https://open.spotify.com/album/7CliyaEoCkPGR1j1qaV17Q"},"href":"https://api.spotify.com/v1/albums/7CliyaEoCkPGR1j1qaV17Q","id":"7CliyaEoCkPGR1j1qaV17Q","images":[{"url":"https://i.scdn.co/image/ab67616d0000b2730129fa32572e985d19623cbf","height":640,"width":640},{"url":"https://i.scdn.co/image/ab67616d00001e020129fa32572e985d19623cbf","height":300,"width":300},{"url":"https://i.scdn.co/image/ab67616d000048510129fa32572e985d19623cbf","height":64,"width":64}],"name":"Aux Trois Baudets, 1953","release_date":"2021-08-26","release_date_precision":"day","type":"album","uri":"spotify:album:7CliyaEoCkPGR1j1qaV17Q","artists":[{"external_urls":{"spotify":"https://open.spotify.com/artist/5UWyW1PcEM8coxeqg3RIHr"},"href":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr","id":"5UWyW1PcEM8coxeqg3RIHr","name":"Georges Brassens","type":"artist","uri":"spotify:artist:5UWyW1PcEM8coxeqg3RIHr"}],"album_group":"album"},<...>]} 
```

Le résultat contient la liste des albums (Tronqué ici pour la lisibilité) de l'artiste avec l'identifiant `5UWyW1PcEM8coxeqg3RIHr` (Georges Brassens). On retrouve aussi la pagination avec le lien vers les données suivantes (`next`) et le total (`total`).

À partir de l'identifiant d'un album, on peut récupérer les titres de l'album à partir de l'URL. Par exemple, pour l'album `4BVfh43LPsAtTYzjgzercl` :

```bash
pfleon@pflmb > ~ > curl --request GET \
  --url https://api.spotify.com/v1/albums/4BVfh43LPsAtTYzjgzercl/tracks \
  --header 'Authorization: Bearer BQBSG5<...>YwCQ'
{"href":"https://api.spotify.com/v1/albums/4BVfh43LPsAtTYzjgzercl/tracks?offset=0&limit=20","items":[{"artists":[{"external_urls":{"spotify":"https://open.spotify.com/artist/5UWyW1PcEM8coxeqg3RIHr"},"href":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr","id":"5UWyW1PcEM8coxeqg3RIHr","name":"Georges Brassens","type":"artist","uri":"spotify:artist:5UWyW1PcEM8coxeqg3RIHr"},{"external_urls":{"spotify":"https://open.spotify.com/artist/7wjSiCt9lKliJKWd92dc1S"},"href":"https://api.spotify.com/v1/artists/7wjSiCt9lKliJKWd92dc1S","id":"7wjSiCt9lKliJKWd92dc1S","name":"Les Petits Français","type":"artist","uri":"spotify:artist:7wjSiCt9lKliJKWd92dc1S"}],"available_markets":["CA","FR"],"disc_number":1,"duration_ms":232266,"explicit":false,"external_urls":{"spotify":"https://open.spotify.com/track/7mjgmfTR7pVH6XIsrQlclR"},"href":"https://api.spotify.com/v1/tracks/7mjgmfTR7pVH6XIsrQlclR","id":"7mjgmfTR7pVH6XIsrQlclR","name":"Elégie à un rat de cave","preview_url":null,"track_number":1,"type":"track","uri":"spotify:track:7mjgmfTR7pVH6XIsrQlclR","is_local":false},{"artists":[{"external_urls":{"spotify":"https://open.spotify.com/artist/5UWyW1PcEM8coxeqg3RIHr"},"href":"https://api.spotify.com/v1/artists/5UWyW1PcEM8coxeqg3RIHr","id":"5UWyW1PcEM8coxeqg3RIHr","name":"Georges Brassens","type":"artist","uri":"spotify:artist:5UWyW1PcEM8coxeqg3RIHr"}],"available_markets":["CA","FR"],"disc_number":1,"duration_ms":218693,"explicit":false,"external_urls":{"spotify":"https://open.spotify.com/track/4O53LdgxaCE55WP9wiNsPk"},"href":"https://api.spotify.com/v1/tracks/4O53LdgxaCE55WP9wiNsPk","id":"4O53LdgxaCE55WP9wiNsPk","name":"Au Bois De Mon Coeur","preview_url":null,"track_number":2,"type":"track","uri":"spotify:track:4O53LdgxaCE55WP9wiNsPk","is_local":false},<...>],"limit":20,"next":"https://api.spotify.com/v1/albums/4BVfh43LPsAtTYzjgzercl/tracks?offset=20&limit=20","offset":0,"previous":null,"total":34}
```

Le résultat contient la liste des titres de l'album avec l'identifiant `4BVfh43LPsAtTYzjgzercl`. On retrouve aussi la pagination avec le lien vers les données suivantes (`next`) et le total (`total`).

### Exercice 2.2 - Création d'un client API Spotify

Reprenez le requête précédente et implémentez un client API Spotify en C# qui offre les fonctionnalités suivantes :

- Récupérer un token d'accès (Bearer Token) à partir du Client ID et du Client Secret
- Rechercher un artiste par son nom
- Récupérer les albums d'un artiste à partir de son identifiant Spotify
- Récupérer les noms des albums d'un artiste à partir de son identifiant Spotify
- Récupérer les titres d'un album à partir de son identifiant Spotify

***Si vous avez passé plus de 3h sur l'API de Spotify, utilisez le code source fourni dans le répertoire `api_spotify_exemple` pour continuer l'exercice.***

### Exercice 2.3 - Création d'un MCP pour Spotify

Votre mission est de créer un MCP qui offre les fonctionnalités suivantes :

- Un outil qui permet de recherche un artiste par son nom
- Un outil qui permet de récupérer les albums d'un artiste à partir de son identifiant Spotify
- Un outil qui permet de récupérer les noms des albums d'un artiste à partir de son identifiant Spotify
- Un outil qui permet de récupérer les titres d'un album à partir de son identifiant Spotify

Pour réaliser cet exercice, vous devez modifier la solution de l'exercice 1 `mcpservertest01` et la modifier. Pour réaliser l'exercice, écrivez les outils un par un et testez-les avant de passer à l'outil suivant.

### Exercice 2.4 - Testez votre MCP Spotify dans un chatbot

Dans cet exercice vous allez pouvoir tester votre MCP Spotify dans un chatbot vocal. Pour cela, vous devez télécharger le projet "chatrealtime" présent à la racine de ce répertoire. Ce projet est une application ASP.NET Core MVC écrit par Claude Sonnet 4.5 qui permet de créer un chatbot vocal en utilisant le service d'OpenAI et d'un connecter des MCPs.

- Ouvrez le projet "chatrealtime" avec Visual Studio 2022 ou Visual Studio Code
- Validez que votre serveur MCP est configuré dans le fichier `appsettings.json` pour ajouter la configuration de votre MCP Spotify.
