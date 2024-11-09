using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Options;

using M01_Configuration_Application;
using M01_Entite;

namespace M01_DAL_Import_Munic_CSV
{
    public class DepotImportationMunicipaliteCSV : IDepotImportationMunicipalites
    {
        private readonly string _separateurChamps = "\",\"";

        private string m_nomFichierAImporter;

        public DepotImportationMunicipaliteCSV(IOptions<DepotImportationMunicipaliteOptions> depotImportationMunicipaliteOptions)
            : this(depotImportationMunicipaliteOptions.Value.FilePath)
        {
            ;
        }

        public DepotImportationMunicipaliteCSV(string? p_nomFichierAImporter)
        {
            if (string.IsNullOrWhiteSpace(p_nomFichierAImporter))
            {
                throw new ArgumentOutOfRangeException(nameof(p_nomFichierAImporter));
            }

            if (!File.Exists(p_nomFichierAImporter))
            {
                throw new InvalidOperationException($"Impossible de trouver le fichier {p_nomFichierAImporter}");
            }

            if (Path.GetExtension(p_nomFichierAImporter) != ".csv")
            {
                throw new InvalidOperationException($"Le fichier {p_nomFichierAImporter} n'est pas un fichier CSV.");
            }

            this.m_nomFichierAImporter = p_nomFichierAImporter;
        }

        public IEnumerable<Municipalite> LireMunicipalites()
        {
            List<Municipalite> municipalites = new List<Municipalite>();

            using (StreamReader sr = File.OpenText(this.m_nomFichierAImporter))
            {
                string? ligneCourante = null;
                int numeroLigneCourante = 0;
                while (!sr.EndOfStream)
                {
                    ligneCourante = sr.ReadLine();
                    ++numeroLigneCourante;
                    if (numeroLigneCourante > 1 && !string.IsNullOrWhiteSpace(ligneCourante))
                    {
                        try
                        {
                            ligneCourante = ligneCourante.Substring(1, ligneCourante.Length - 2);
                            string[] colonnes = ligneCourante.Split(_separateurChamps);
                            DateOnly dateElections;

                            Municipalite municipalite = new Municipalite(
                            int.Parse(colonnes[0]),
                            colonnes[1],
                            !string.IsNullOrWhiteSpace(colonnes[7]) ? colonnes[7] : null,
                            !string.IsNullOrWhiteSpace(colonnes[8]) ? colonnes[8] : null,
                            DateOnly.TryParseExact(colonnes[23], "yyyy-MM-dd", out dateElections) ? dateElections : null
                            );

                            municipalites.Add(municipalite);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidDataException($"Le fichier {this.m_nomFichierAImporter} n'est pas au bon format à la ligne {numeroLigneCourante}", ex);
                        }
                    }
                }

                sr.Close();
            }

            return municipalites;
        }
    }
}
