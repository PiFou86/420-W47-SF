using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace M01_Configuration_Application
{
    public static class Configuration
    {
        private static IConfigurationRoot _configuration;
        private static IConfigurationRoot Settings
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration =
                            new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                                    .AddJsonFile("appsettings.json", false)
                                    .Build();
                }

                return _configuration;
            }
        }

        public static string ChaineConnextion
        {
            get
            {
                return Settings.GetConnectionString("BDMunicipalites");
            }
        }
    }
}
