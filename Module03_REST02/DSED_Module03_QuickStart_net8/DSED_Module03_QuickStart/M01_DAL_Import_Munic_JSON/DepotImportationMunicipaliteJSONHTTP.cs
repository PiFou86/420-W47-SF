using System.Text.Json;

using Microsoft.Extensions.Options;

using M01_Configuration_Application;
using M01_Entite;

namespace M01_DAL_Import_Munic_JSON
{
    public class DepotImportationMunicipaliteJSONHTTP : IDepotImportationMunicipalites
    {
        private string m_uri;

        public DepotImportationMunicipaliteJSONHTTP(IOptions<ConfigurationImportationMunicipalites> depotImportationMunicipaliteOptions)
        {
            this.m_uri = depotImportationMunicipaliteOptions.Value.Uri;
        }

        public IEnumerable<Municipalite> LireMunicipalites()
        {
            string json = LireContenuURI(this.m_uri);
            Rootobject? root = JsonSerializer.Deserialize<Rootobject>(json);

            return root!.result!.records!.Select(m =>
                new Municipalite(
                                 int.Parse(m!.mcode!),
                                 m!.munnom!,
                                 m!.mcourriel,
                                 m!.mweb,
                                 m!.datelec is not null ? DateOnly.FromDateTime(m.datelec.Value) : null

                )
            ).ToList();
        }

        protected virtual string LireContenuURI(string uri)
        {
            using HttpClient httpClient = new HttpClient();
            Task<HttpResponseMessage> responseTask = httpClient.GetAsync(uri);
            responseTask.Wait();
            HttpResponseMessage response = responseTask.Result;
            response.EnsureSuccessStatusCode();
            Task<string> contentTask = response.Content.ReadAsStringAsync();
            contentTask.Wait();
            return contentTask.Result;
        }

        public class Rootobject
        {
            public Result? result { get; set; }
        }

        // Classes internes pour désérialisation JSON
        public class Result
        {
            public Record[]? records { get; set; }
        }

        public class Record
        {
            public string? mcode { get; set; }
            public string? munnom { get; set; }
            public string? mcourriel { get; set; }
            public string? mweb { get; set; }
            public DateTime? datelec { get; set; }
        }

    }
}
