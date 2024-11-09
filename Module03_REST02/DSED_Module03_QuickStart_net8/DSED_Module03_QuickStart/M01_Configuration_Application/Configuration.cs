using System;
using System.IO;

using Microsoft.Extensions.Configuration;

namespace M01_Configuration_Application
{
    public static class Configuration
    {
        private static IConfigurationRoot? _configuration;
        private static IConfigurationRoot Settings
        {
            get
            {
                if (_configuration is null)
                {
                    _configuration =
                            new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)!.FullName)
                                    .AddJsonFile("appsettings.json", false)
                                    .Build();
                }

                return _configuration;
            }
        }

        public static string? ChaineConnextion
        {
            get
            {
                return Settings?.GetConnectionString("BDMunicipalites");
            }
        }

        public static string? MunicipaliteImportationFilePath
        {
            get
            {
                return Settings?.GetSection("ImportationMunicipalites:FilePath").Value;
            }
        }
    }
}
