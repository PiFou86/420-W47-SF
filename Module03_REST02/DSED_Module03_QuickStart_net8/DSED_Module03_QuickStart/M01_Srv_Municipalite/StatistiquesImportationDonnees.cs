using System.Text;

namespace M01_Srv_Municipalite
{
    public class StatistiquesImportationDonnees
    {
        public int NombreEnregistrementsAjoutes { get; set; }
        public int NombreEnregistrementsModifies { get; set; }
        public int NombreEnregistrementsDesactives { get; set; }
        public int NombreEnregistrementsNonModifies { get; set; }
        public int NombreMunicipalitesImportees { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Nombre enregistrements ajoutés : {this.NombreEnregistrementsAjoutes}");
            sb.AppendLine($"Nombre enregistrements modifiés : {this.NombreEnregistrementsModifies}");
            sb.AppendLine($"Nombre enregistrements desactivés : {this.NombreEnregistrementsDesactives}");
            sb.AppendLine($"Nombre enregistrements non modifiés : {this.NombreEnregistrementsNonModifies}");
            sb.AppendLine($"Nombre municipalites importées : {this.NombreMunicipalitesImportees}");

            return sb.ToString();
        }
    }
}
