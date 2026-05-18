using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


            // Создаём папку, если её нет
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);

            // Если файла нет — создаём из embedded ресурса или дефолта
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
