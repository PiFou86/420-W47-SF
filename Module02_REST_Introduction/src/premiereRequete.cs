class Program
{
    private static HttpClient httpClient = new HttpClient();
    private static string uriRechercheParAuteur = "/query.json?type=/type/edition&authors=/authors/{refAuteur}&*=";
    static void Main(string[] args)
    {
        Task<List<Livre>> res = Requete("OL44388A");
        res.Wait();
        res.Result.ForEach(livre => Console.Out.WriteLine(livre.title));
    }

    private static async Task<List<Livre>> Requete(string p_auteur)
    {
        List<Livre> livres = null;

        httpClient.BaseAddress = new Uri("http://openlibrary.org");
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await httpClient.GetAsync(uriRechercheParAuteur.Replace("{refAuteur}", p_auteur));

        if (response.IsSuccessStatusCode)
        {
            string contenuJSON = await response.Content.ReadAsStringAsync();
            livres = JsonConvert.DeserializeObject<List<Livre>>(contenuJSON);
        }

        return livres??new List<Livre>();
    }
}

public class Livre
{
    public string title { get; set; }
}