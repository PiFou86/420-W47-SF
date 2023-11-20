# Module 04 - Authentification

Objectifs :

- Observer le mécanisme d'authentification OpenID
- Mettre en place un contrôle de type clef API

## Exercice 1 - Observation des mécanismes d'authentification dans une API ASP.Net MVC avec OpenID

### Exercice 1.1 - Mise en place de la solution

- Assurez-vous que [Firefox](https://www.mozilla.org) et [NodeJs](https://nodejs.org) sont installés sur votre poste de développement. Dans une console, entrez `node -v`
- Créez la solution Visual Studio "DSED_Module04_React" de type "ASP.NET Core avec React.js" et choisissez le framework ".NET 6.0" et le type d'authentification "Comptes d'utilisateurs individuels" (Si vous voulez créer le projet sans Visual Studio, créez-le en ligne de commande avec la commande ```dotnet new react --auth Individual --name "DSED_Module04_React"``` et faites ensuite un ```dotnet run``` pour lancer le programme)
- Compilez votre programme : la restauration des packages npm (node) va prendre un peu de temps. En attendant la fin, allez observer les attributs de la classe "WeatherForecastController"
- Une fois compilée, lancez l'exécution de votre projet principal.

### Exercice 1.2 - Création d'un premier compte

- Lancez votre site web local dans Firefox (Chrome ne garde pas les réponses !) et ouvrez le mode développeur (F12)
- Enregistrez un utilisateur avec le mot de passe "Passw0rd.". N'oubliez pas de confirmer l'adresse courriel en cliquant simplement sur le lien de la page.
- Cliquez sur "Login"
- Validez que votre compte fonctionne
- Quittez votre session en cliquant sur "Logout"

### Exercice 1.3 - Exploitation de la trace réseau pour valider le fonctionnement d'OpenID Connect

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
- Essayez de faire un "GET" vers "https://localhost:5001/weatherforecast" (remplacez 5001 par votre numéro de port local. Si vous n'exposez pas en TLS, n'oubliez pas de modifier le protocole pour http). Vous devriez avoir une réponse "401".
- Allez dans l'onglet "Authorization", sélectionnez le type "BearerToken" et renseignez le champ "Token". Refaite maintenant la requête : vous devriez avoir les informations de météo

Si vous avez une erreur d'authentification à cause du jeton :

- Modifiez le fichier "Program.cs", avant le `var app = builder.Build()`, ajouter (Réf. https://github.com/dotnet/core/blob/main/release-notes/6.0/known-issues.md#aspnet-core) :
```csharp
builder.Services.Configure<JwtBearerOptions>("IdentityServerJwtBearer", o => o.Authority = "https://localhost:44416");
```
- Modifiez le port (ici 44416) par celui de votre application NodeJS
- Relancez votre application et refaite le test

## Exercice 2 - Protégez votre API de municipalités

### Exercice 2.1 - Reproduire le mécanisme d'authentification par clef d'API

- Reprenez votre solution qui contient l'API de manipulation des municipalités
- Implantez le filtre qui valide la sécurité avec une clef API. Pour cela inspirez vous du cours.
- Ajoutez l'attribut que vous venez de créer à votre contrôleur "MunicipalitesController" 
- Validez que tout fonctionne avec "Postman"

### Exercice 2.2 - Améliorer la validation des clefs API en les stockant dans un dépôt de données

- Dans votre base de données, créez la table "ClefAPI" avec la clef primaire "ClefAPIId" de type Guid ou VARCHAR(140) si le type Guid n'existe pas
- De retour dans votre projet, créez la classe "ClefAPI" qui permet de stocker des clefs API de type "Guid"
- À partir de la classe "ApplicationDbContext", ajoutez la propriété table "ClefsAPIs" de type "DbSet\<ClefAPI>"
- Modifiez votre dépot afin de pouvoir valider l'existance d'une clef API dans la table éponyme clef API
- Modifiez le filtre d'authentification afin de valider la clef, non pas à partir de votre constante mais à partir du dépot. Inspirez-vous du code qui suit pour obtenir le dépôt
- Validez que le tout fonctionne

Si vous avez besoin d'un objet, comme un dépot, vous pouvez utiliser la méthode "GetService" de la propriété RequestServices du contexte http :

```csharp
using Microsoft.Extensions.DependencyInjection;
// ...

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter {
  public async Task OnActionExecutionAsync(ActionExecutingContext p_context, ActionExecutionDelegate p_next) {
    IDepotXYZ appContext = p_context.HttpContext.RequestServices.GetService<IDepotXYZ>();

    // ...
  }
}
```
