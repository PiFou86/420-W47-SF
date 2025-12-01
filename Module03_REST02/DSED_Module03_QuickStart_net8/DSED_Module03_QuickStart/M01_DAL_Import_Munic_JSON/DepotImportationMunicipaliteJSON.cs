using System.Text.Json;

using Microsoft.Extensions.Options;

using M01_Configuration_Application;
using M01_Entite;

namespace M01_DAL_Import_Munic_JSON
{
    public class DepotImportationMunicipaliteJSON : IDepotImportationMunicipalites
    {
        private string m_nomFichierAImporter;

        public DepotImportationMunicipaliteJSON(IOptions<ConfigurationImportationMunicipalites> depotImportationMunicipaliteOptions)
            : this(depotImportationMunicipaliteOptions.Value.Uri)
        {
            ;
        }

        public DepotImportationMunicipaliteJSON(string? p_nomFichierAImporter)
        {
            if (string.IsNullOrWhiteSpace(p_nomFichierAImporter))
            {
                throw new ArgumentOutOfRangeException(nameof(p_nomFichierAImporter));
            }

            if (!File.Exists(p_nomFichierAImporter))
            {
                throw new InvalidOperationException($"Impossible de trouver le fichier {p_nomFichierAImporter}");
            }

            if (Path.GetExtension(p_nomFichierAImporter) != ".json")
            {
                throw new InvalidOperationException($"Le fichier {p_nomFichierAImporter} n'est pas un fichier JSON.");
            }

            this.m_nomFichierAImporter = p_nomFichierAImporter;
        }

        public IEnumerable<Municipalite> LireMunicipalites()
        {
            string json = LireContenuURI(this.m_nomFichierAImporter);
            Rootobject? root = JsonSerializer.Deserialize<Rootobject>(json);

            return root!.result!.records!.Select(m =>
                new Municipalite(
                                 int.Parse(m!.mcode!),
                                 m!.munnom!,
                                 m!.mcourriel,
                                 m!.mweb,
                                 m!.datelec.HasValue ? DateOnly.FromDateTime(m.datelec.Value) : null

                )
            ).ToList();
        }

        protected virtual string LireContenuURI(string uri)
        {
            return File.ReadAllText(this.m_nomFichierAImporter);
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
