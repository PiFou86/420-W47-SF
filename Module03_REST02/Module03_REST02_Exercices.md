# Module 03 - REST 02

## Exercice 1 - Municipalités

L'objectif de l'exercice est de créer une API REST permettant de fournir les opérations CRUD aux entités de type Municipalités.

### Exercice 1.1 - Création de l'API REST

***Afin de simplifier le code, je vous demande de ne faire qu'une solution avec un projet de type Application web et de créer vos différentes couches dans des répertoires qui simulent vos projets services et accès aux données. Ce type de projet utilise le moteur d'injection de dépendances : pour avoir un contexte, il vous suffit de créer un constructeur avec en paramètre le type d'objet voulu.***

- Créez la solution "DSED_M03_REST01" de type "Application Web ASP.NET Core" :
  - Choisissez le modèle général "Appication web (Model-View-Controller)"
  - Choisissez le type d'authentification "Comptes d'utilisateurs individuels"
- Par défaut, le projet va créer la chaine de connexion "DefaultConnection" qui utilise une base de données de type "localdb". Allez modifier cette chaîne de connexion pour la faire correspondre à votre base de données locale qui contient vos données de municipalités.
- Modifiez vos options de démarrage de projet pour que ce ne soit pas "IIS Express" qui héberge votre site web, mais que ce soir le programme lui-même en sélectionnant "DSED_M03_REST01"
- Ajoutez le support de Swagger en ajoutant le package Nuget "NSwag.AspNetCore" et en modifiant la classe "Startup" comme indiqué dans le cours et la démo
- Dans votre projet, créez les classes qui permettent de manipuler des ressources de types "Municipalite" en utilisant une abstraction de dépot de municipalités dans un répertoire "services" :
  - Les municipalités sont définies comme dans les modules précédents
  - Vos services doivent permettre :
    - De lister les municipalités ou d'avoir l'information par rapport à un identifiant
    - De créer / modifier une municipalité
    - De supprimer une municipalité (suppression logique)
- Toujours dans ce projet et dans le répertoire "Data", modifiez la couche d'accès aux données qui permet de persister les données de l'entité "Municipalite". Ajoutez une implantation de dépot qui utilise cette couche d'accès aux données.
- Ajoutez le contrôleur d'API "MunicipalitesController" :
  - Faites un clic droit sur le répertoire "Controller" de votre projet
  - Choisissez "Ajouter" puis "Contrôleur"
  - Choisissez "Contrôleur d'API avec actions de lecture/écriture"
  - Appelez le contrôleur (la classe) "MunicipalitesController"
  - Ajustez chaque action pour qu'elles renvoient des "ActionResult" comme présenté dans la démonstration
- Testez votre API à travers les pages d'exploration d'API de Swagger

### Exercice 1.2 - Client console

- Créez une nouvelle solution à partir d'une nouvelle instance de Visual Studio (ie, votre solution précédente est toujours disponible)
- Créez le code C# client à partir de NSwag Studio
- Écrivez un programme qui :
  - Liste les municipalités et affiche leurs noms sur la console
  - Modifie le nom de la municipalité de Québec pour "Quebecq" ([retour aux sources de 1601](https://fr.wikipedia.org/wiki/Québec_(ville)#Toponymie))
***Avant de tester votre programme et de générer le code client, n'oubliez pas de valider que votre API est bien en cours d'exécution dans l'autre instance de Visual Studio***

## Exercice 2 - Ajout des élections (Optionnel)

- Ajoutez le support d'élections :
  - Ajouter le contrôleur d'API "ElectionsController". Il doit permettre les opérations de type CRUD sur les objets de type "Election"
  - Les élections contiennent simplement un identifiant, un code géographique pour identifier la municipalité et une date d'élections
  - Il doit y avoir un service spécifique aux municipalités et un pour les élections (ie deux classes dans le même projet)
  - La couche d'accès aux données utilise le même contexte applicatif
- Validez que le tout fonctionne avec Swagger UI.
- 
