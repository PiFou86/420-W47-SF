using System.Collections.Generic;

namespace M01_Srv_Municipalite
{
    public interface IDepotImportationMunicipalites
    {
        IEnumerable<Municipalite> LireMunicipalites();
    }
}
