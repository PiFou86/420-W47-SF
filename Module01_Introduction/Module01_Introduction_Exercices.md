# Module 01 - Introduction

## Exercice 1 - Lecture des municipalités - CSV

À partir de la spécification donnée sur la page web contenant les données des municipalités vous allez extraire :

- Le code géographique
- Le nom de la municipalité
- L'adresse courriel
- L'adresse du site web
- La date des prochaines élections

Une fois ces données extraites, vous devez les insérer ou les mettre à jour dans une base de données MySQL ou Microsoft SQL Server ou Oracle.

<details>
    <summary>Diagramme de classe global</summary>

![Diagramme UML](../images/Module01_Introduction/uml_exercice1/uml_exercice1.png)
</details>

### Étape 1 - Visualisation du fichier

- Téléchargez les données des municipalités à partir de [la page du MAMH du site des données libres du québec](https://www.donneesquebec.ca/recherche/fr/dataset/repertoire-des-municipalites-du-quebec/resource/19385b4e-5503-4330-9e59-f998f5918363).
- Ouvrez le document avec Visual Studio Code ou NotePad++ et essayez de comprendre comment est fait le fichier.
- Ouvrez ce même fichier dans Excel et choisissez "Données" puis "Convertir". Débrouillez vous pour que le fichier s'affiche correctement

### Étape 2 - Depot de type EF

---

Pour accéder à la base de données, vous devez utiliser Entity Framework core. Si la librairie vous est inconnue, basez-vous sur l'article https://docs.microsoft.com/en-us/ef/core/ et sur l'approche code first.

De manière grossière :

- Une propriété de type DbSet correspond à une table. Par convention, la table doit avoir le même nom que la classe et inversement.
- Le type paramétré du DbSet correspond à la structure de la table
- La clef primaire est, par convention, nommé par le nom de la classe suivi de "Id". Exemple pour la table "Client", on doit avoir une classe appelée "Client" et la clef primaire doit être stockée dans la propriété "ClientId".
- Chaque propriété correspond à un champ dans la table. Le type du champ détermine le type dans la base de données.
- Le comportement par convention peut être modifié en utilisant des attributs, mais je vous conseille de suivre la méthode par convention

Afin de respecter les bonnes pratiques vous devez implanter le patron "depot" que vous avez vu dans le [module 06 en POOII](https://github.com/PiFou86/420-W30-SF/blob/master/Module06_Formats_Echanges/Module06_Formats_Echanges_Exercices.md) dans le module sur les formats d'échanges de données.

Si c'est votre première utilisation d'entityFramework, il faut installer les outils. Pour cela, ouvrez une ligne de commande et tapez :

```powershell
dotnet tool install --global dotnet-ef
```

---

- Créez une solution Visual Studio du type "console" avec le cadriciel .Net core. Le projet doit être nommé "DSED_M01_Fichiers_Texte"
- Ajoutez le projet "M01_Srv_Municipalite" de type "bibliothèque de classes". Ce projet va contenir la classe "Municipalite" qui contient les informations pertinentes sur les municipalitées plus une propriété nommée "Actif" de type booléen. Le booléen "Actif" permet simuler la suppression d'un enregistrement (suppression logique à la place de physique)
- Ajoutez les interfaces "IDepotMunicipalites" et "IDepotImportationMunicipalites" :
  - IDepotMunicipalites :
    - ChercherMunicipaliteParCodeGeographique : int -> Municipalite (Renvoie la municipalité active ou non par son code géographique)
    - ListerMunicipalitesActives : () -> IEnumerable\<Municipalite> (Renvoie seulement les municipalités actives)
    - DesactiverMunicipalite : (Municipalite) -> ()
    - AjouterMunicipalite : Municipalite -> ()
    - MAJMunicipalite : Municipalite -> ()
  - IDepotImportationMunicipalite:
    - LireMunicipalite : () ->  IEnumerable\<Municipalite>
- Ajoutez le projet "M01_DAL_Municipalite_MySQL" ou "M01_DAL_Municipalite_SQLServer" de type "bibliothèque de classes". Ce projet va implanter l'interface "IDepotMunicipalites"
- Dans le projet "M01_DAL_Municipalite_XYZ", installez les packages NuGet :
  - "MySql.Data.EntityFrameworkCore" si vous décidez d'utiliser MySql
  - "Oracle.EntityFrameworkCore" si vous décidez d'utiliser Oracle
  - "Microsoft.EntityFrameworkCore.SqlServer" si vous décidez d'utiliser SqlServer
  - "Microsoft.EntityFrameworkCore.Design" quelque soit votre cas
- Créez une classe de contexte qui peut se connecter à votre base de données (MySQL ou SQLServer)
  - Pour la surcharge de la méthode "OnConfiguring", inspirez vous des liens suivants :
    - [MySQL](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html)
    - [Oracle](https://github.com/oracle/dotnet-db-samples/blob/master/samples/dotnet-core/ef-core/get-started/create-model-save-query-scaffold.cs)
    - [SQL Server](https://docs.microsoft.com/en-us/ef/core)
  - À la suite de l'appel à "UseMySQL / UseSQLServer / UseOracle", ajoutez l'appel à la méthode U le tout devrait ressembler à cela :

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options) {
  options.UseMySQL("server=localhost;database=municipalites;user=root;password=Passw0rd")
         .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
```

  - Pour l'exercice, la chaine de connexion peut être codée. Dans un contexte réel, elle serait renseignée dans un fichier de configuration (Ex. appsettings.json : [voir module 08 - Architecture des applications du cours de POOII](https://github.com/PiFou86/420-W30-SF/blob/master/Module08_ArchitectureDesApplications/Module08_ArchitectureDesApplications_Exercices.md)
- Créez la base de données avec les commandes suivantes :
  - ```dotnet ef migrations add "initial"``` : la commande va parcourir votre code à la recherche de modification de structure de base de données afin de créer une méthode de migration nommée ici "initial". Le nom de chaque migration doit être différent d'une migration à l'autre
  - ```dotnet ef database update``` : applique la/les migrations
  - ***Les commandes doivent être tapées à partir de la racine du projet "M01_DAL_Municipalite_XYZ". L'utilitaire dotnet-ef doit être installé***
- Validez que votre base de données à la bonne structure.

### Étape 3 - Lecture C# du fichier CSV

- Ajoutez le projet "M01_DAL_Import_Munic_CSV" de type "bibliothèque de classes"
- Ajoutez-y une classe qui implante l'interface "IDepotImportationMunicipalite"
- Codez la méthode "LireMunicipalite" :
  - Ouvrez le fichier en mode lecture
  - Lisez le fichier ligne par ligne (ReadLine)
  - Pour chacune des lignes, créez un objet de type "Municipalite" avec les bonnes valeurs. (Vous pouvez utiliser la méthode ["Split" de la classe "String"](https://docs.microsoft.com/en-us/dotnet/api/system.string.split?view=netcore-3.1))

### Étape 4 - Traitement du fichier

- À partir du projet "M01_Srv_Municipalite", ajoutez la classe "StatistiquesImportationDonnees" avec les propriétés suivantes :
  - NombreEnregistrementsAjoutes : int
  - NombreEnregistrementsModifies : int
  - NombreEnregistrementsDesactives : int
- Implantez la méthode "ToString" de cette dernière.
- Ajoutez la classe "TraitementImporterDonneesMunicipalite"
- Créez un constructeur d'initialisation qui reçoit deux objets, un objet de type "IDepotImportationMunicipalite" et l'autre de type "IDepotMunicipalites".
- Ajoutez la méthode "Executer" : () -> StatistiquesImportationDonnees
  - Si la municipalité est manquante, l'ajouter
  - Si la municipalité est existante, la mettre à jour seulement si nécessaire (ex. si inactive, l'activer)
  - Si la municipalité n'existe pas dans le fichier à importer, la marquer inactive
  - Suivant le cas, incrémentez le compteur correspondant
  - À la fin du traitement, renvoie les statistiques

### Étape 5 - Programme principal

- Votre programme principal doit créer une instance de traitement. Pour cela, vous devez préalablement créer les instances des dépôts et les passer au constructeur d'initialisation.
- Le programme exécute ensuite le traitement et affiche les statistiques sur la sortie standard.

### Étape 5 - Tests unitaires (Optionnel mais fortement recommandé !)

- Faîtes vos tests unitaires de la classe de traitement en utilisant les packages "XUnit" et "Moq".

## Exercice 2 - Lecture des municipalités - JSON

- Refaites les étapes 1 (Affichage texte seulement sans Excel) et 3 de l'exercice 1 mais avec le format JSON disponible à l'adresse suivante : https://www.donneesquebec.ca/recherche/api/action/datastore_search?resource_id=19385b4e-5503-4330-9e59-f998f5918363&limit=3000.
- Modifiez votre programme principal afin qu'il utilise maintenant le dépôt de type JSON pour faire vos exécutions.
