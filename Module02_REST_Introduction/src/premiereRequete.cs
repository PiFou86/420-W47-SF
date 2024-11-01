using System.Text.Json;
using System.Net.Http.Headers;

class Program
{
    private static HttpClient httpClient = new HttpClient();
    private static string uriRechercheParAuteur = "/query.json?type=/type/edition&authors=/authors/{refAuteur}&*=";
    static void Main(string[] args)
    {
        List<Livre> res = Requete("OL44388A");
        res.ForEach(livre => Console.Out.WriteLine(livre.title));
    }
    private static List<Livre> Requete(string p_auteur)
    {
        List<Livre>? livres = null;

        httpClient.BaseAddress = new Uri("http://openlibrary.org");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Task<HttpResponseMessage> responseTask = httpClient.GetAsync(uriRechercheParAuteur.Replace("{refAuteur}", p_auteur));
        // PFL : la partie asynchrone est enlevée pour simplifier le code
        // PFL : la notion est/sera abordée dans un autre cours (JS et TypeScript)
        responseTask.Wait();
        HttpResponseMessage response = responseTask.Result;

        if (response.IsSuccessStatusCode)
        {
            Task<string> contenuTask = response.Content.ReadAsStringAsync();
            contenuTask.Wait();
            string contenuJSON = contenuTask.Result;
            livres = JsonSerializer.Deserialize<List<Livre>>(contenuJSON);
        }
        return livres ?? new List<Livre>();
    }
}
public class Livre
{
    public string title { get; set; } = string.Empty;
}
