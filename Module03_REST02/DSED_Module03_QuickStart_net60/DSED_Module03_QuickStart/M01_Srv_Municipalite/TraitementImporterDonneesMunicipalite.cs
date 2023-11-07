using System.Collections.Generic;
using System.Linq;

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

            IEnumerable<Municipalite> municipalitesImportees = this.m_depotImportationMunicipalites.LireMunicipalites();
            stats.NombreMunicipalitesImportees = municipalitesImportees.Count();

            foreach (Municipalite municipaliteImportee in municipalitesImportees)
            {
                Municipalite municipaliteDuDepot = this.m_depotMunicipalites.ChercherMunicipaliteParCodeGeographique(municipaliteImportee.CodeGeographique);

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
            }

            Dictionary<int, Municipalite> municipalitesImporteesDic = municipalitesImportees.ToDictionary(m => m.CodeGeographique);
            IEnumerable<Municipalite> municipalitesADesactiver =
                this.m_depotMunicipalites
                    .ListerMunicipalitesActives()
                    .Where(m => !municipalitesImporteesDic.ContainsKey(m.CodeGeographique));

            foreach (Municipalite municipalite in municipalitesADesactiver)
            {
                this.m_depotMunicipalites.DesactiverMunicipalite(municipalite);
                ++stats.NombreEnregistrementsDesactives;
            }

            return stats;
        }
    }
}
