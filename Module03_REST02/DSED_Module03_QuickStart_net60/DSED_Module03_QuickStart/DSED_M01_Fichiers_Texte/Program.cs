using M01_Configuration_Application;
using M01_DAL_Import_Munic_CSV;
using M01_DAL_Municipalite_SQLServer;
using M01_Srv_Municipalite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace DSED_M01_Fichiers_Texte
{
    class Program
    {
        static void Main(string[] args)
        {
            DbContextOptionsBuilder<MunicipaliteContextSQLServer> dbContextOptionsBuilder = 
                new DbContextOptionsBuilder<MunicipaliteContextSQLServer>();
            dbContextOptionsBuilder.UseSqlServer(Configuration.ChaineConnextion)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
#if DEBUG
                            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                            .EnableSensitiveDataLogging()
#endif
                            ;

            IDepotMunicipalites depotMunicipalites = new DepotMunicipalitesSQLServer(new MunicipaliteContextSQLServer(dbContextOptionsBuilder.Options));
            IDepotImportationMunicipalites depotImportationMunicipalites = new DepotImportationMunicipaliteCSV(Path.Combine(Directory.GetParent(AppContext.BaseDirectory).FullName, @"Donnees\MUN.csv"));

            TraitementImporterDonneesMunicipalite tidm = new TraitementImporterDonneesMunicipalite(depotImportationMunicipalites, depotMunicipalites);
            StatistiquesImportationDonnees sid = tidm.Executer();
            Console.Out.WriteLine(sid);
        }
    }
}
