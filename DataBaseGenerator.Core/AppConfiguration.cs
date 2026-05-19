using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DataBaseGenerator.Core
{
    public static class AppConfiguration
    {
        private static readonly string ConfigDirectory = Path.Combine(AppContext.BaseDirectory,
        "../../../AppData");


        public static IConfigurationRoot LoadConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configPath = Path.Combine(ConfigDirectory, $"appsettings.{environment}.json");


            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);
                        
            if (!File.Exists(configPath))
                CreateDefaultConfig(configPath);

            return new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();
        }

        private static void CreateDefaultConfig(string configPath)
        {
            var defaultConfig = @"{
                ""ConnectionStrings"": {
                ""DefaultConnection"": ""server = localhost; database = medxregistry; user = root; password = root; port = 3306""
                },
                ""WebHost"": {
                ""Url"": ""http://localhost:5289"",
                ""ApiUrl"": ""http://localhost:5289/api/""
                }
            }";
            File.WriteAllText(configPath, defaultConfig);
        }

    }
}
