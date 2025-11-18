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
using Microsoft.Extensions.Options;

namespace DSED_M01_Fichiers_Texte
{
    class Program
    {
        static void Main(string[] args)
        {
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
            builder.Services.AddOptions<ConfigurationImportationMunicipalites>()
    .Bind(builder.Configuration.GetSection("ImportationMunicipalites"));
            builder.Services.AddScoped<TraitementImporterDonneesMunicipalite>();

            // Fournisseur de services temporaire pour récupérer le type de dépôt à utiliser
            ServiceProvider tempProvider = builder.Services.BuildServiceProvider();
            ConfigurationImportationMunicipalites depotImportationMunicipaliteOptions = tempProvider.GetRequiredService<IOptions<ConfigurationImportationMunicipalites>>().Value;
            if (depotImportationMunicipaliteOptions.Mode.ToLower() == "local")
            {

                switch (Path.GetExtension(depotImportationMunicipaliteOptions.Uri).ToLower())
                {
                    case ".csv":
                        builder.Services.AddScoped<IDepotImportationMunicipalites, DepotImportationMunicipaliteCSV>();
                        break;
                    case ".json":
                        builder.Services.AddScoped<IDepotImportationMunicipalites, DepotImportationMunicipaliteJSON>();
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Le fichier {depotImportationMunicipaliteOptions.Uri} n'est pas un fichier CSV ou JSON.");
                }
            }
            else
            {
                if (depotImportationMunicipaliteOptions.Mode.ToLower() == "http")
                {

                    builder.Services.AddScoped<IDepotImportationMunicipalites, DepotImportationMunicipaliteJSONHTTP>();
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Le mode {depotImportationMunicipaliteOptions.Mode} n'est pas supporté.");
                }
            }

            IHost host = builder.Build();

            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                IServiceProvider services = serviceScope.ServiceProvider;
                TraitementImporterDonneesMunicipalite tidm = services.GetRequiredService<TraitementImporterDonneesMunicipalite>();
                StatistiquesImportationDonnees sid = tidm.Executer();
                Console.Out.WriteLine(sid);
            }
        }
    }
}
