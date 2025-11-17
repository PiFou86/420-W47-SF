using OpenAI.Chat;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hostBuilder = Host.CreateApplicationBuilder(args);
var configuration = hostBuilder.Configuration;

// Configuration directe dans le code
string apiKey = configuration["ClefOpenAPI"];
string modele = configuration["modele"] ?? "gpt-5-nano";
string promptSysteme = configuration["promptSysteme"] ?? "Tu es un assistant utile et amical. Réponds de manière concise et claire.";

#if DEBUG
Console.Out.WriteLine($"Clef API OpenAI: {apiKey}");
Console.Out.WriteLine($"Modèle: {modele}");
Console.Out.WriteLine($"Prompt Système: {promptSysteme}");
#endif

// Initialisation du client OpenAI
var client = new ChatClient(modele, apiKey);

// Historique de conversation
var messages = new List<ChatMessage>
{
    new SystemChatMessage(promptSysteme)
};

Console.WriteLine("=== Chat Simple avec ChatGPT ===");
Console.WriteLine($"Modèle: {modele}");
Console.WriteLine("Tapez 'exit' ou 'quit' pour quitter\n");

// Boucle de conversation
while (true)
{
    Console.Write("Vous: ");
    string? question = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(question)) continue;
    if (question.ToLower() is "exit" or "quit") break;
    
    // Ajouter la question de l'utilisateur
    messages.Add(new UserChatMessage(question));
    
    try
    {
#if DEBUG_JSON
        // Affichage de la requête en JSON pour débogage
        string requeteJson = JsonSerializer.Serialize(messages.Select(m => new 
        { 
            Role = m.GetType().Name.Replace("ChatMessage", ""),
            Content = m switch
            {
                SystemChatMessage scm => scm.Content[0].Text,
                UserChatMessage ucm => ucm.Content[0].Text,
                AssistantChatMessage acm => acm.Content[0].Text,
                _ => "Unknown"
            }
        }), new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("\n=== DEBUG REQUÊTE JSON ===");
        Console.WriteLine(requeteJson);
        Console.WriteLine("==========================\n");
#endif
        
        // Appel à l'API OpenAI
        var completion = client.CompleteChat(messages);
        
#if DEBUG_JSON
        // Affichage de la réponse complète en JSON pour débogage
        string reponseJson = JsonSerializer.Serialize(completion.Value, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        Console.WriteLine("\n=== DEBUG RÉPONSE JSON ===");
        Console.WriteLine(reponseJson);
        Console.WriteLine("==========================\n");
#endif
        
        string reponse = completion.Value.Content[0].Text;
        
        // Ajouter la réponse à l'historique
        messages.Add(new AssistantChatMessage(reponse));
        
        Console.WriteLine($"Assistant: {reponse}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur: {ex.Message}\n");
    }
}

Console.WriteLine("Au revoir!");
