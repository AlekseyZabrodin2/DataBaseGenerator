using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel.Options
{
    public sealed class GenerationOptionsStore
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _path;
        private bool _loading;

        public GenerationOptionsStore()
        {
            _path = Path.Combine(AppContext.BaseDirectory, "generation-options.json");

            var document = Read();
            MySql = Create("BirthdateMySql", document?.MySql);
            LiteDb = Create("BirthdateLiteDb", document?.LiteDb);
            PatientsDatabasePath = document?.PatientsDatabasePath ?? string.Empty;
            PlannedStudiesDatabasePath = document?.PlannedStudiesDatabasePath ?? string.Empty;
            DicomTestDataFolderPath = document?.DicomTestDataFolderPath ?? string.Empty;
            DicomSaveFolderPath = document?.DicomSaveFolderPath ?? string.Empty;
        }

        public PatientGenerationOptions MySql { get; }

        public PatientGenerationOptions LiteDb { get; }

        public string PatientsDatabasePath { get; private set; }

        public string PlannedStudiesDatabasePath { get; private set; }

        public string DicomTestDataFolderPath { get; private set; }

        public string DicomSaveFolderPath { get; private set; }

        public void SetPatientsDatabasePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PatientsDatabasePath, path, StringComparison.OrdinalIgnoreCase))
                return;

            PatientsDatabasePath = path;
            Save();
        }

        public void SetPlannedStudiesDatabasePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PlannedStudiesDatabasePath, path, StringComparison.OrdinalIgnoreCase))
                return;

            PlannedStudiesDatabasePath = path;
            Save();
        }

        public void SetDicomTestDataFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(DicomTestDataFolderPath, path, StringComparison.OrdinalIgnoreCase))
                return;

            DicomTestDataFolderPath = path;
            Save();
        }

        public void SetDicomSaveFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(DicomSaveFolderPath, path, StringComparison.OrdinalIgnoreCase))
                return;

            DicomSaveFolderPath = path;
            Save();
        }

        private PatientGenerationOptions Create(string groupName, GenerationOptionsData data)
        {
            var options = new PatientGenerationOptions(groupName);
            if (data != null)
            {
                _loading = true;
                options.CopyFrom(data);
                _loading = false;
            }

            options.PropertyChanged += (_, _) => Save();
            return options;
        }

        private GenerationOptionsDocument Read()
        {
            if (!File.Exists(_path))
                return null;

            try
            {
                return JsonSerializer.Deserialize<GenerationOptionsDocument>(File.ReadAllText(_path), JsonOptions);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Не удалось прочитать {0}", _path);
                return null;
            }
        }

        private void Save()
        {
            if (_loading)
                return;

            try
            {
                var document = new GenerationOptionsDocument
                {
                    MySql = MySql.ToData(),
                    LiteDb = LiteDb.ToData(),
                    PatientsDatabasePath = PatientsDatabasePath,
                    PlannedStudiesDatabasePath = PlannedStudiesDatabasePath,
                    DicomTestDataFolderPath = DicomTestDataFolderPath,
                    DicomSaveFolderPath = DicomSaveFolderPath
                };

                File.WriteAllText(_path, JsonSerializer.Serialize(document, JsonOptions));
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Не удалось сохранить {0}", _path);
            }
        }

        private sealed class GenerationOptionsDocument
        {
            public GenerationOptionsData MySql { get; set; }
            public GenerationOptionsData LiteDb { get; set; }
            public string PatientsDatabasePath { get; set; }
            public string PlannedStudiesDatabasePath { get; set; }
            public string DicomTestDataFolderPath { get; set; }
            public string DicomSaveFolderPath { get; set; }
        }
    }
}
