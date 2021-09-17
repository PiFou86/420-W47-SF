using M01_Configuration_Application;
using M01_DAL_Import_Munic_CSV;
using M01_DAL_Municipalite_MySQL;
using M01_Srv_Municipalite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace DSED_M01_Fichiers_Texte
{
    class Program
    {
        static void Main(string[] args)
        {
            DbContextOptionsBuilder<MunicipaliteContextMySQL> dbContextOptionsBuilder = 
                new DbContextOptionsBuilder<MunicipaliteContextMySQL>();
            dbContextOptionsBuilder.UseMySQL(Configuration.ChaineConnextion)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            IDepotMunicipalites depotMunicipalites = new DepotMunicipalitesMySQL(new MunicipaliteContextMySQL(dbContextOptionsBuilder.Options));
            IDepotImportationMunicipalites depotImportationMunicipalites = new DepotImportationMunicipaliteCSV(Path.Combine(Directory.GetParent(AppContext.BaseDirectory).FullName, @"Donnees\MUN.csv"));

            TraitementImporterDonneesMunicipalite tidm = new TraitementImporterDonneesMunicipalite(depotImportationMunicipalites, depotMunicipalites);
            StatistiquesImportationDonnees sid = tidm.Executer();
            Console.Out.WriteLine(sid);
        }
    }
}
