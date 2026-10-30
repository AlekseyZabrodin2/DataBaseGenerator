using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
            DicomReaderDirPath = document?.DicomReaderDirPath ?? string.Empty;
            PacsServerIp = document?.PacsServerIp ?? string.Empty;
            PacsServerPort = document?.PacsServerPort ?? string.Empty;
            PacsCallingAe = document?.PacsCallingAe ?? string.Empty;
            PacsCalledAe = document?.PacsCalledAe ?? string.Empty;
            PacsFolderPath = document?.PacsFolderPath ?? string.Empty;
            ParserConfig = NormalizeParserConfig(document?.Parser);
        }

        public PatientGenerationOptions MySql { get; }

        public PatientGenerationOptions LiteDb { get; }

        public string PatientsDatabasePath { get; private set; }

        public string PlannedStudiesDatabasePath { get; private set; }

        public string DicomTestDataFolderPath { get; private set; }

        public string DicomSaveFolderPath { get; private set; }

        public string DicomReaderDirPath { get; private set; }

        public string PacsServerIp { get; private set; }

        public string PacsServerPort { get; private set; }

        public string PacsCallingAe { get; private set; }

        public string PacsCalledAe { get; private set; }

        public string PacsFolderPath { get; private set; }

        public ParserAppConfig ParserConfig { get; private set; }

        public Task<ParserAppConfig> LoadParserConfigAsync()
        {
            return Task.FromResult(ParserConfig);
        }

        public Task SaveParserConfigAsync(ParserAppConfig config)
        {
            ParserConfig = NormalizeParserConfig(config);
            Save();
            return Task.CompletedTask;
        }

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

        public void SetDicomReaderDirPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(DicomReaderDirPath, path, StringComparison.OrdinalIgnoreCase))
                return;

            DicomReaderDirPath = path;
            Save();
        }

        public void SetPacsServerIp(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PacsServerIp, path, StringComparison.OrdinalIgnoreCase))
                return;

            PacsServerIp = path;
            Save();
        }

        public void SetPacsServerPort(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PacsServerPort, path, StringComparison.OrdinalIgnoreCase))
                return;

            PacsServerPort = path;
            Save();
        }

        public void SetPacsCallingAe(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PacsCallingAe, path, StringComparison.OrdinalIgnoreCase))
                return;

            PacsCallingAe = path;
            Save();
        }

        public void SetPacsCalledAe(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PacsCalledAe, path, StringComparison.OrdinalIgnoreCase))
                return;

            PacsCalledAe = path;
            Save();
        }

        public void SetPacsFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(PacsFolderPath, path, StringComparison.OrdinalIgnoreCase))
                return;

            PacsFolderPath = path;
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
                    DicomSaveFolderPath = DicomSaveFolderPath,
                    DicomReaderDirPath = DicomReaderDirPath,
                    PacsServerIp = PacsServerIp,
                    PacsServerPort = PacsServerPort,
                    PacsCallingAe = PacsCallingAe,
                    PacsCalledAe = PacsCalledAe,
                    PacsFolderPath = PacsFolderPath,
                    Parser = ParserConfig
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
            public string DicomReaderDirPath { get; set; }
            public string PacsServerIp { get; set; }
            public string PacsServerPort { get; set; }
            public string PacsCallingAe { get; set; }
            public string PacsCalledAe { get; set; }
            public string PacsFolderPath { get; set; }
            public ParserAppConfig Parser { get; set; }
        }

        private static ParserAppConfig NormalizeParserConfig(ParserAppConfig config)
        {
            config ??= new ParserAppConfig();
            config.Sources ??= new ParserSourcesConfig();
            config.Sources.Folders ??= [];
            config.Sources.Pacs ??= [];
            return config;
        }
    }
}
