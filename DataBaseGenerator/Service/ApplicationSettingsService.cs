using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DataBaseGenerator.UI.Wpf.Model;

namespace DataBaseGenerator.UI.Wpf.Service
{
    public class ApplicationSettingsService
    {
        private readonly string _settingsPath;

        public ApplicationSettings Settings { get; }

        public ApplicationSettingsService()
        {
            var directory = Path.Combine(AppContext.BaseDirectory, "DataBaseGeneratorSettings");

            Directory.CreateDirectory(directory);

            _settingsPath = Path.Combine(directory, "userSettings.json");

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
    }
}
