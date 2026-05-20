using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;

namespace DataBaseGenerator.Core.MySqlGenerator
{
    public static class AppConfiguration
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private static readonly string _configDirectory;

        private static readonly string _webConfigDirectory;


        static AppConfiguration()
        {
#if DEBUG

            _configDirectory = Path.Combine(AppContext.BaseDirectory,"../../../AppData");
#else
            
            _configDirectory = Path.Combine(AppContext.BaseDirectory,"AppData");
#endif
            
            _webConfigDirectory = Path.Combine(AppContext.BaseDirectory,"../AppData");
        }


        public static IConfigurationRoot LoadConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configPath = Path.Combine(_configDirectory, $"appsettings.{environment}.json");

            _logger.Info($"ENVIRONMENT = {environment}");
            _logger.Info($"Config path = {configPath}");
            _logger.Info($"File exists = {File.Exists(configPath)}");

            if (File.Exists(configPath))
            {
                var content = File.ReadAllText(configPath);
                _logger.Info($"File content: {content}");
            }

            if (!Directory.Exists(_configDirectory))
                Directory.CreateDirectory(_configDirectory);

            if (!File.Exists(configPath))
                CreateDefaultConfig(configPath);

            return new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();
        }

        public static IConfigurationRoot LoadWebConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configPath = Path.Combine(_webConfigDirectory, $"appsettings.{environment}.json");

            _logger.Info($"ENVIRONMENT = {environment}");
            _logger.Info($"Config path = {configPath}");
            _logger.Info($"File exists = {File.Exists(configPath)}");

            if (File.Exists(configPath))
            {
                var content = File.ReadAllText(configPath);
                _logger.Info($"File content: {content}");
            }

            if (!Directory.Exists(_webConfigDirectory))
                Directory.CreateDirectory(_webConfigDirectory);

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
