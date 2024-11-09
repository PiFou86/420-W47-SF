#define INJECTION 

using System;
using System.Diagnostics;
using System.IO;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using M01_Configuration_Application;
using M01_DAL_Import_Munic_CSV;
using M01_DAL_Municipalite_SQLServer;
using M01_Srv_Municipalite;
using M01_DAL_Import_Munic_JSON;
using M01_Entite;

namespace DSED_M01_Fichiers_Texte
{
    class Program
    {
        static void Main(string[] args)
        {
#if INJECTION // Avec injection de dépendances
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            string connectionString = builder.Configuration.GetConnectionString("BDMunicipalites") ?? throw new InvalidOperationException("Connection string 'PersonnesConnection' not found.");

            builder.Services.AddDbContext<MunicipaliteContextSQLServer>(options =>
            {
                options.UseSqlServer(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
#if DEBUG
                .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                .EnableSensitiveDataLogging()
#endif
                ;
            });

            builder.Services.AddScoped<IDepotMunicipalites, DepotMunicipalitesSQLServer>();
            builder.Services.Configure<DepotImportationMunicipaliteOptions>(builder.Configuration.GetSection("ImportationMunicipalites"));
            builder.Services.AddScoped<TraitementImporterDonneesMunicipalite>();

            DepotImportationMunicipaliteOptions depotImportationMunicipaliteOptions = builder.Configuration.GetSection("ImportationMunicipalites").Get<DepotImportationMunicipaliteOptions>() ?? throw new InvalidOperationException("ImportationMunicipalites section not found.");
            switch (Path.GetExtension(depotImportationMunicipaliteOptions.FilePath))
            {
                case ".csv":
                    builder.Services.AddScoped<IDepotImportationMunicipalites, DepotImportationMunicipaliteCSV>();
                    break;
                case ".json":
                    builder.Services.AddScoped<IDepotImportationMunicipalites, DepotImportationMunicipaliteJSON>();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Le fichier {depotImportationMunicipaliteOptions.FilePath} n'est pas un fichier CSV ou JSON.");
            }


            IHost host = builder.Build();

            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                IServiceProvider services = serviceScope.ServiceProvider;
                TraitementImporterDonneesMunicipalite tidm = services.GetRequiredService<TraitementImporterDonneesMunicipalite>();
                StatistiquesImportationDonnees sid = tidm.Executer();
                Console.Out.WriteLine(sid);
            }
#else // Sans injection : manuel
            DbContextOptionsBuilder<MunicipaliteContextSQLServer> dbContextOptionsBuilder =
            new DbContextOptionsBuilder<MunicipaliteContextSQLServer>();
            dbContextOptionsBuilder.UseSqlServer(Configuration.ChaineConnextion)
                                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
#if DEBUG
                                   .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                                   .EnableSensitiveDataLogging()
#endif
                                   ;

            using (MunicipaliteContextSQLServer municipaliteContext = new MunicipaliteContextSQLServer(dbContextOptionsBuilder.Options))
            {
                IDepotMunicipalites depotMunicipalites = new DepotMunicipalitesSQLServer(municipaliteContext);
                string? filePath = Configuration.MunicipaliteImportationFilePath;
                if (filePath is null)
                {
                    throw new InvalidOperationException("Fichier d'importation des municipalités non spécifié dans le fichier de configuration.");
                }
                IDepotImportationMunicipalites depotImportationMunicipalites = new DepotImportationMunicipaliteCSV(filePath);

                TraitementImporterDonneesMunicipalite tidm = new TraitementImporterDonneesMunicipalite(depotImportationMunicipalites, depotMunicipalites);
                StatistiquesImportationDonnees sid = tidm.Executer();
                Console.Out.WriteLine(sid);
            }
#endif
        }
    }
}
