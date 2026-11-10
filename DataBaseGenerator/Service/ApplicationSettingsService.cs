using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator.Data;
using DataBaseGenerator.UI.Wpf.Model;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DataBaseGenerator.UI.Wpf.Service
{
    public class ApplicationSettingsService
    {
        private readonly string _settingsPath;

        public ApplicationSettings Settings { get; }

        public ApplicationSettingsService()
        {
            _settingsPath = Path.Combine(UserDataPaths.SettingsDirectory, "userSettings.json");

            Settings = Load();
        }

        private ApplicationSettings Load()
        {
            if (!File.Exists(_settingsPath))
                return new ApplicationSettings();

            var json = File.ReadAllText(_settingsPath);

            return JsonSerializer.Deserialize<ApplicationSettings>(json) ?? new ApplicationSettings();
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_settingsPath, json);
        }

        public BaseGenerateContext CreateContext()
        {
            if (string.IsNullOrWhiteSpace(Settings.MySqlServer) ||
                string.IsNullOrWhiteSpace(Settings.MySqlDatabase) ||
                string.IsNullOrWhiteSpace(Settings.MySqlUser))
            {
                throw new InvalidOperationException("Заполните подключение MySQL в настройках.");
            }

            var builder = new MySqlConnectionStringBuilder
            {
                Server = Settings.MySqlServer,
                Port = Settings.MySqlPort,
                Database = Settings.MySqlDatabase,
                UserID = Settings.MySqlUser,
                Password = Settings.MySqlPassword
            };

            var options = new DbContextOptionsBuilder<BaseGenerateContext>()
                .UseMySql(builder.ConnectionString, new MySqlServerVersion(new Version(8, 0, 28)))
                .Options;

            return new BaseGenerateContext(options);
        }
    }
}
