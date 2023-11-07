using M01_Srv_Municipalite;
using System;
using System.Collections.Generic;
using System.IO;

namespace M01_DAL_Import_Munic_CSV
{
    public class DepotImportationMunicipaliteCSV : IDepotImportationMunicipalites
    {
        private readonly string _separateurChamps = "\",\"";

        private string m_nomFichierAImporter;
        public DepotImportationMunicipaliteCSV(string p_nomFichierAImporter)
        {
            if (string.IsNullOrWhiteSpace(p_nomFichierAImporter))
            {
                throw new ArgumentOutOfRangeException(nameof(p_nomFichierAImporter));
            }

            if (!File.Exists(p_nomFichierAImporter))
            {
                throw new InvalidOperationException($"Impossible de trouver le fichier {p_nomFichierAImporter}");
            }

            this.m_nomFichierAImporter = p_nomFichierAImporter;
        }

        public IEnumerable<Municipalite> LireMunicipalites()
        {
            List<Municipalite> municipalites = new List<Municipalite>();

            using (StreamReader sr = File.OpenText(this.m_nomFichierAImporter))
            {
                string ligneCourante = null;
                int numeroLigneCourante = 0;
                while (!sr.EndOfStream)
                {
                    ligneCourante = sr.ReadLine();
                    ++numeroLigneCourante;
                    if (numeroLigneCourante > 1 && !string.IsNullOrWhiteSpace(ligneCourante))
                    {
                        try
                        {
                            ligneCourante = ligneCourante.Substring(1, ligneCourante.Length - 2);
                            string[] colonnes = ligneCourante.Split(_separateurChamps);
                            DateTime dateElections;
                            DateTime.TryParse(colonnes[20], out dateElections);
                            // DateTime.ParseExact(colonnes[20], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            Municipalite municipalite = new Municipalite(
                                int.Parse(colonnes[0]),
                                colonnes[1],
                                colonnes[7],
                                colonnes[8],
                                dateElections,
                                true
                            );

                            municipalites.Add(municipalite);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidDataException($"Le fichier {this.m_nomFichierAImporter} n'est pas au bon format à la ligne {numeroLigneCourante}", ex);
                        }
                    }
                }

                sr.Close();
            }

            return municipalites;
        }
    }
}
