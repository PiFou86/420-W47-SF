using System.Text.Json;

using Microsoft.Extensions.Options;

using M01_Configuration_Application;
using M01_Entite;

namespace M01_DAL_Import_Munic_JSON
{
    public class DepotImportationMunicipaliteJSON : IDepotImportationMunicipalites
    {
        private string m_nomFichierAImporter;

        public DepotImportationMunicipaliteJSON(IOptions<DepotImportationMunicipaliteOptions> depotImportationMunicipaliteOptions)
            : this(depotImportationMunicipaliteOptions.Value.FilePath)
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
            string json = File.ReadAllText(this.m_nomFichierAImporter);
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

        public class Rootobject
        {
            public Result? result { get; set; }
        }

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
