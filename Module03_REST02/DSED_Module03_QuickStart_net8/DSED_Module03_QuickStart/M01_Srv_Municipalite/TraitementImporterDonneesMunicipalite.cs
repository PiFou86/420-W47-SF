using System.Collections.Generic;
using System.Linq;

using M01_Entite;

namespace M01_Srv_Municipalite
{
    public class TraitementImporterDonneesMunicipalite
    {
        private IDepotImportationMunicipalites m_depotImportationMunicipalites;
        private IDepotMunicipalites m_depotMunicipalites;

        public TraitementImporterDonneesMunicipalite(IDepotImportationMunicipalites p_depotImportationMunicipalites, IDepotMunicipalites p_depotMunicipalites)
        {
            this.m_depotImportationMunicipalites = p_depotImportationMunicipalites;
            this.m_depotMunicipalites = p_depotMunicipalites;
        }
        public StatistiquesImportationDonnees Executer()
        {
            StatistiquesImportationDonnees stats = new StatistiquesImportationDonnees();
            Dictionary<int, Municipalite > municipalitesImportees = 
                this.m_depotImportationMunicipalites
                    .LireMunicipalites()
                    .ToDictionary(m => m.CodeGeographique);
            Dictionary<int, Municipalite> municipalitesBD = 
                this.m_depotMunicipalites
                    .ListerMunicipalitesActives()
                    .ToDictionary(m => m.CodeGeographique);


            foreach (Municipalite municipaliteImportee in municipalitesImportees.Values)
            {
                Municipalite? municipaliteDuDepot = municipalitesBD.GetValueOrDefault(municipaliteImportee.CodeGeographique);

                if (municipaliteDuDepot is null)
                {
                    this.m_depotMunicipalites.AjouterMunicipalite(municipaliteImportee);
                    ++stats.NombreEnregistrementsAjoutes;
                }
                else
                {
                    if (municipaliteDuDepot != municipaliteImportee)
                    {
                        this.m_depotMunicipalites.MAJMunicipalite(municipaliteImportee);
                        ++stats.NombreEnregistrementsModifies;
                    }
                    else
                    {
                        ++stats.NombreEnregistrementsNonModifies;
                    }
                }
                ++stats.NombreMunicipalitesImportees;
            }

            IEnumerable<Municipalite> municipalitesADesactiver = 
                municipalitesBD.Values
                               .Where(m => 
                                      !municipalitesImportees.ContainsKey(m.CodeGeographique)
                                     );

            foreach (Municipalite municipalite in municipalitesADesactiver)
            {
                this.m_depotMunicipalites.DesactiverMunicipalite(municipalite);
                ++stats.NombreEnregistrementsDesactives;
            }

            return stats;
        }
    }
}
