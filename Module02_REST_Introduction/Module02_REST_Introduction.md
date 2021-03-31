# Module 02 - REST - Introduction

## Préambule

- Créez une solution contenant un projet de type console.
- Adaptez le code suivant dans votre projet et testez le ligne par ligne
- Que fait ce code ?

![Première requête REST](img/premiere_requete.png)

## Outils pour vos tests

### Effectuer des reqêtes HTTP sans code

- Téléchargez et installez l'application Postman (https://www.postman.com/)
- Dans ce logiciel, effectuez une requête de type GET avec l'adresse suivante : "http://openlibrary.org/query.json?type=/type/edition&authors=/authors/OL44388A&*="
- Observez le résultat et les différents indicateurs de l'outil (nous reviendrons dessus plus tard)
- Copiez le résultat du corp ("Body")

### Décortiquer un document JSON

- Allez sur le site : https://codebeautify.org/jsonviewer
- Collez le texte JSON dans la partie de gauche et explorer les options du centre de la page (Tree Viewer, Beautify) et de la partie droite (tri, filtre, etc.)

***Attention : n'utilisez jamais un site du genre avec des données sensibles car vous ne maîtrisez pas ce que le site en fait***

## Exercice 1 - Retour sur les municipalités

- Reprenez la proposition de correction du Module 1 ou votre propre code.
- Ajoutez le projet "M01_DAL_Import_Munic_REST_JSON"
- Ajoutez les packages Nuget "Microsoft.AspNet.WebApi.Client" (Facilite la création de requêtes http) et "Newtonsoft.Json" ((Dé)sérialisation JSON)
- Ajoutez-y une classe qui implante l'interface "IDepotImportationMunicipalite"
- Dans le constructeur, instanciez un HttpClient que vous allez garder dans une données membre
- Codez la méthode "LireMunicipalite" :
  - Spécifiez la bonne adresse de base : "https://www.donneesquebec.ca"
  - Effacez l'entête des fichiers acceptés de votre objet de requêtes
  - Ajoutez l'entête de fichiers acceptés "application/json"
  - Utilisez la méthode "GET" pour obtenir les données présentes à l'adresse :  https://www.donneesquebec.ca/recherche/api/action/datastore_search?resource_id=19385b4e-5503-4330-9e59-f998f5918363&limit=3000
  - Pour réaliser cette requête, inspirez-vous du code du préambule
  - Utilisez la bibliothèque Newtonsoft.Json de Newtonsoft
- Modifiez votre programme principal afin qu'il utilise maintenant le dépôt de type JSON pour faire vos exécutions.

## Exercice 2 - Météo

- Créez une nouvelle solution avec un projet de type console
- Le projet console (interface utilisateur) doit proposer un menu qui permet :
  - De rechercher les villes disponibles à partir des données de latitude et de longitude
  - D'afficher la météo d'une ville saisie
  - De quitter le programme
- Avant d'allez plus loin, essayez de comprendre un peu mieux les données que vous allez manipuler en naviguant les URIs suivantes :
  - https://www.metaweather.com/api/location/search/?lattlong=46.785307,-71.287363
  - https://www.metaweather.com/api/location/3534/
- Créez deux autres projets :
  - Un projet contenant la couche de services pour répondre aux différents besoins de l'interface utilisateur
  - Un projet contenant la couche d'accès aux données qui effectue les requêtes à l'API REST
- La documentation de l'API est disponible à l'adresse suivante : https://www.metaweather.com/api/

## Exercice 3 - Actualité - COVID 19 (Optionnel)

- Explorez l'API covid19api.com.
- Créez un programme qui :
  - Au premier lancement, importe toutes les données du "Canada" et insérez les dans une base de données
  - Aux lancements subséquents, importe seulement les données des jours non déjà importées
