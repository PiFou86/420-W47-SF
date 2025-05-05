# Module 02 - REST - Introduction

## Préambule

- Créez une solution contenant un projet de type console.
- Adaptez le code suivant dans votre projet et testez-le ligne par ligne
- Que fait ce code ?

![Première requête REST](https://github.com/user-attachments/assets/6190b99d-c67a-4946-85c7-a46afaac07d4)

## Outils pour vos tests

### Effectuer des requêtes HTTP sans code

- Téléchargez et installez l'application Postman (https://www.postman.com/)
- Dans ce logiciel, effectuez une requête de type GET avec l'adresse suivante : "http://openlibrary.org/query.json?type=/type/edition&authors=/authors/OL44388A&*="
- Observez le résultat et les différents indicateurs de l'outil (nous reviendrons dessus plus tard)
- Copiez le résultat du corps ("Body")

### Décortiquer un document JSON

- Allez sur le site : https://codebeautify.org/jsonviewer
- Collez le texte JSON dans la partie de gauche et explorez les options du centre de la page (Tree Viewer, Beautify) et de la partie droite (tri, filtre, etc.)

***Attention : n'utilisez jamais un site du genre avec des données sensibles, car vous ne maitrisez pas ce que le site en fait***

## Exercice 1 - Retour sur les municipalités

- Reprenez la proposition de correction du Module 1 ou votre propre code.
- Ajoutez le projet "M01_DAL_Import_Munic_REST_JSON"
- Ajoutez les packages Nuget "Microsoft.AspNet.WebApi.Client" (facilite la création de requêtes HTTP)
- Ajoutez-y une classe qui implante l'interface "IDepotImportationMunicipalites"
- Dans le constructeur, instanciez un HttpClient que vous allez garder dans une donnée membre
- Codez la méthode "LireMunicipalite" :
  - Spécifiez la bonne adresse de base : "https://www.donneesquebec.ca"
  - Effacez l'entête des fichiers acceptés de votre objet de requêtes
  - Ajoutez l'entête de fichiers acceptés "application/json"
  - Utilisez la méthode "GET" pour obtenir les données présentes à l'adresse :  https://www.donneesquebec.ca/recherche/api/action/datastore_search?resource_id=19385b4e-5503-4330-9e59-f998f5918363&limit=3000
  - Pour réaliser cette requête, inspirez-vous du code du préambule
  - Utilisez les méthodes de la bibliothéque standard de .Net [System.Text.Json.JsonSerializer.Deserialize](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializer.deserialize)
- Modifiez votre programme principal afin qu'il utilise maintenant le dépôt de type JSON pour faire vos exécutions.

## Exercice 2 - Utilisation d'un projet open source de vision

### Exercice 2.1 - Détection de visages

Pré-requis :

- Docker
- Connexion réseau
- Des images avec des visages

Dans cet exercice, vous allez utiliser l'API de vision [CodeProjet.AI](https://www.codeproject.com/ai/) (anciennement [DeepStack](https://github.com/johnolafenwa/DeepStack)) afin détecter les visages (Face detection) dans une image. [Le code du projet se trouve sur un dépôt GitHub](https://github.com/johnolafenwa/DeepStack). Le dépôt contient un fichier README.md très riche.

Écrivez un programme qui va :

- Prendre une image en paramètre
- Envoyer cette image à l'API de vision de détection de visages
- Dessiner un rectangle rouge autour des visages
- Enregistrer l'image résultante dans le répertoire "output"

Pour cela, vous allez utiliser les documentations suivantes :

- Lancer le conteneur contenant l'image docker en suivant [Installation par docker](https://www.codeproject.com/ai/docs/install/running_in_docker.html). Raccourci : `docker run --name CodeProject.AI -d -p 32168:32168 codeproject/ai-server` (Ensuite, vous pouvez naviguer l'adresse [http://localhost:32168](http://localhost:32168) pour voir et installer de nouveaux modules)
- Regarder l'exemple de la documentation de l'[API de détection de visages](https://www.codeproject.com/ai/docs/api/api_reference.html#face-detection) afin de voir comment l'exploiter. Pour désérialiser l'objet JSON, débutez par afficher le document JSON sur la console ou utilisez POSTMAN. Faites un collage spécial dans Visual Studio (Coller JSON comme classes)

Version rapide : example pour ceux qui ne veulent pas lire la documentation :

```csharp
using System.Text.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;


namespace appone
{
    class Response
    {
        public bool success { get; set; }
        public DetectedObject[] predictions { get; set; }

    }

    class DetectedObject
    {
        public string label { get; set; }
        public float confidence { get; set; }
        public int y_min { get; set; }
        public int x_min { get; set; }
        public int y_max { get; set; }
        public int x_max { get; set; }

    }

    class App
    {
        static HttpClient client = new HttpClient();

        public static string detectFaceJson(string image_path)
        {
            MultipartFormDataContent request = new MultipartFormDataContent();
            FileStream image_data = File.OpenRead(image_path);
            request.Add(new StreamContent(image_data), "image", Path.GetFileName(image_path));
            request.Add(new StringContent("Mysecretkey"), "api_key");
            Task<HttpResponseMessage> outputTask = client.PostAsync("http://localhost:32168/v1/vision/face", request);
            outputTask.Wait();
            HttpResponseMessage output = outputTask.Result;
            Task<string> jsonStringTask = output.Content.ReadAsStringAsync();
            jsonStringTask.Wait();
            string jsonString = jsonStringTask.Result;

            return jsonString;
        }

        static void Main(string[] args)
        {
            string jsonString = detectFaceJson("Photo le 2022-02-18 à 15.53.jpg");
            Console.Out.WriteLine(jsonString);
        }
    }
}
```

- Pour récupérer le nom de l'image, vous devez utiliser le paramètre "args" de la classe "Program". Vous pouvez utiliser [cette page pour vous aider](https://dailydotnettips.com/how-to-pass-command-line-arguments-using-visual-studio/)
- Pour modifier l'image, vous devez utiliser le package nuget "SixLabors.ImageSharp.Drawing" (Si le package ne s'affiche pas cochez la case "include prerelease"). Exemple de code :

```csharp
using (var img = Image.Load(image_path))
{
    img.Mutate(ctx =>
            ctx.Draw(Color.Red, 2.0f, new RectangleF(
                rectangle.x_min, rectangle.y_min, rectangle.x_max - rectangle.x_min, rectangle.y_max - rectangle.y_min
            )
        )
    );
    img.Save($"output/{image_path}");
}
```

Si vous avez plusieurs rectangles à tracer, faites plusieurs appels à la méthode ```Mutate``` et faites un seul ```Save```. N'oubliez pas de remplacer ```rectangle``` par une variable qui a du sens dans votre code !

Pour tester l'exercice, ma fille s'est donnée à fond : les visages sont penchés, il y a présence de grimages, etc. mais malgré cela, le système fonctionne.

![CodeProject.AI, détection de visages malgré les grimages](img/deepstack01.png)

### Exercice 2.2 - Détection d'objets

- Cherchez une image qui contient des voitures, des personnes et des vélos
- Naviguez le site de l'API et installez le module "Object Detection (YOLOv8)" à partir de l'onglet "Install modules"
- Utilisez la documentation de l'API pour trouver comment détecter les objets de l'image et affichez l'information sur la console
- Modifiez votre programme pour encadrer les objets détectés et mettez aussi un texte contenant le label de l'image

Pour vous aider, voici un bout de code qui permet d'écrire un texte :

```csharp
img.Mutate(ctx =>
         {
             ctx.Draw(Color.Red, 2.0f, new RectangleF(obj.x_min, obj.y_min, obj.x_max - obj.x_min, obj.y_max - obj.y_min));
             ctx.Fill(Color.Red, new RectangleF(obj.x_min, obj.y_min - 20, obj.x_max - obj.x_min, 20));
             ctx.DrawText(obj.label, SystemFonts.CreateFont("Arial", 15), Color.White, new PointF(obj.x_min, obj.y_min - 20));
         }

);
```

Vous pouvez prendre l'image suivante pour avoir un résultat similaire au suivant :

![](img/jack-finnigan-aEkk0KxvPpg-unsplash.jpg)

![](img/jack-finnigan-aEkk0KxvPpg-unsplash-avec-labels.jpg)

## Exercice 3 - Météo - Optionnel

- Créez une nouvelle solution avec un projet de type console
- Le projet console (interface utilisateur) doit proposer un menu qui permet :
  - De rechercher les villes disponibles à partir de leurs noms et d'en déduire la latitude et la longitude
  - D'afficher la météo d'une ville saisie
  - De quitter le programme
- Avant d'aller plus loin, essayez de comprendre un peu mieux les données que vous allez manipuler en naviguant les URIs suivantes :
  - https://geocoding-api.open-meteo.com/v1/search?name=Québec
  - https://api.open-meteo.com/v1/forecast?latitude=46.81228&longitude=-71.21454&hourly=temperature_2m&current_weather=true
- Créez deux autres projets :
  - Un projet contenant la couche de services pour répondre aux différents besoins de l'interface utilisateur
  - Un projet contenant la couche d'accès aux données qui effectue les requêtes à l'API REST
- La documentation de l'API est disponible à l'adresse suivante : https://open-meteo.com/en/docs#api_form

<!-- ## Exercice 3 - Actualité - COVID 19 (Optionnel)

- Explorez l'API covid19api.com.
- Créez un programme qui :
  - Au premier lancement, importe toutes les données du "Canada" et insérez-les dans une base de données
  - Aux lancements subséquents, importe seulement les données des jours non déjà importées -->
