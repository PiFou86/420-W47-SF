# Module 04 - Authentification

Objectifs :

- Observer le mécanisme d'authentification OpenID
- Mettre en place un contrôle de type clef API

## Exercice 1 - Observation des mécanismes d'authentification dans une API ASP.Net MVC

- Créez la solution Visual Studio "DSED_Module04_React" de type "ASP.NET Core Wep Application" :
  - Choisissez le gabarit "React.js"
  - Choisissez le type d'authentification "Comptes d'utilisateurs individuels"
- Compilez votre programme : la restauration des packages npm (node) va prendre un peu de temps. En attendant la fin, allez observer les attributs de la classe "WeatherForecastController"
- Une fois compilée, lancez l'exécution de votre projet principal.
- Si Firefox n'est pas installé, l'installer sur votre poste de développement
- Lancez votre site web local dans Firefox (Chrome ne garde pas les réponses !) et ouvrez le mode développeur (F12)
- Enregistrez un utilisateur avec le mot de passe "Passw0rd.". N'oubliez pas de confirmer l'adresse courriel en cliquant simplement sur le lien de la page.
- Cliquez sur "Login"
- Ouvrez les outils de développement et l'onglet réseau. Validez qu'aucun filtre n'est actif ("Tout")
- Dans les options, cochez l'option "Conserver les journaux" si ce n'est pas fait
- Effacez le journal réseau
- Entrez vos identifiants et validez
- Observez le code de retour de la requête vers la page "Login?Return[...]" vers quelle page le navigateur va se rediriger ? Est-ce bien l'URL que vous avez comme requête suivante ?
- Voyez-vous dans la première URL d'où provient cette adresse ?
- Observez la requête vers l'adresse "token". Que voyez-vous dans la réponse ?
- Ouvrez l'entête de la requête "userinfo". Voyez-vous un champ relatif à l'authentification ? Est-il renseigné avec un élément d'une requête précédente ? Qu'observez-vous dans la réponse ?
- Copiez la valeur du couple qui a pour clef "Authorization" (sans le mot clef "Bearer") et allez sur le site jwt.io et collez le jeton JWT. Observez ce qu'il contient.
- Ouvrez "Postman"
- Essayez de faire un "GET" vers "https://localhost:5001/weatherforecast". Vous devriez avoir une réponse "401".
- Allez dans l'onglet "Authorization", sélectionnez le type "BearerToken" et renseignez le champ "Token". Refaite maintenant la requête : vous devriez avoir les informations de météo

## Exercice 2 - Protégez votre API de municipalités

- Reprenez votre solution qui contient l'API de manipulation des municipalités
- Implantez le filtre qui valide la sécurité avec une clef API
- Validez que tout fonctionne avec "Postman"
- Créez la classe "ClefsAPI" qui permet de stocker des clefs API de type "Guid"
- À partir de la classe "ApplicationDbContext", ajoutez la table "ClefsAPI"
- Modifiez votre dépot afin de pouvoir valider une clef API
- Modifiez le filtre d'authentification afin de valider la clef, non pas à partir de votre constante mais à partir du dépot.
