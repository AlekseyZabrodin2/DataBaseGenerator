using System;
using System.IO;
using System.Windows;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
using DataBaseGenerator.Core.LiteDbGenerator.Services;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.UI.Wpf.Licensing;
using DataBaseGenerator.UI.Wpf.Service;
using DataBaseGenerator.UI.Wpf.UserControls;
using DataBaseGenerator.UI.Wpf.View;
using DataBaseGenerator.UI.Wpf.ViewModel;
using DataBaseGenerator.UI.Wpf.ViewModel.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace DataBaseGenerator.UI.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IHost _host;
        private readonly ApplicationSettingsService _settingsService;

        public static IStudyStorageModule SharedStorage { get; set; }
        public static IPlannedStudyStorageModule PlannedStorage { get; private set; }
        public static bool IsSharedStorageReadOnly { get; private set; } = true;
        public static event Action<bool>? SharedStorageModeChanged;


        public static T GetService<T>()
        where T : class
        {
            if ((App.Current as App)!._host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        public App()
        {
            this.InitializeComponent();

            var configuration = AppConfiguration.LoadConfiguration();

            _settingsService = new ApplicationSettingsService();
            _settingsService.ApplyConnectionStringIfEmpty(configuration.GetConnectionString("DefaultConnection"));

            var dataDirectory = Path.Combine(AppContext.BaseDirectory, "LiteDataBase");
            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            var dbPath = Path.Combine(dataDirectory, "patients.db");
            var plannedDbPath = Path.Combine(dataDirectory, "planned_studies.db");

            SharedStorage = new LiteDbStudyStorageModule(dbPath, true);
            PlannedStorage = new LiteDbPlannedStudyStorageModule(plannedDbPath, true);

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddConfiguration(configuration);
                })
                .ConfigureServices((context, services) =>
                {

                    var root = (IConfigurationRoot)context.Configuration;

                    foreach (var provider in root.Providers)
                    {
                        if (provider.TryGet("ConnectionStrings:DefaultConnection", out var value))
                        {
                            _logger.Info($"PROVIDER: {provider}");
                            _logger.Info($"VALUE: {value}");
                        }
                    }

                    services.AddSingleton<IStudyApplicationService, StudyApplicationService>();
                    services.AddSingleton(this);
                    services.AddSingleton<GenerationOptionsStore>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MySqlGeneratorViewModel>();
                    services.AddSingleton<LiteDbGeneratorViewModel>();
                    services.AddSingleton<LiteDbStudiesTableViewModel>();
                    services.AddSingleton<LiteDbPlannedStudiesViewModel>();
                    services.AddSingleton<LiteDbViewerViewModel>();
                    services.AddSingleton<FileGeneratorViewModel>();
                    services.AddSingleton<DicomReaderViewModel>();
                    services.AddSingleton<DicomPacsViewModel>();
                    services.AddSingleton<DicomFileParserViewModel>();
                    services.AddTransient<DialogMessageWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<SpecificationWindow>();
                    services.AddSingleton(_settingsService);
                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<SettingsUserControl>();
                    });

            _host = hostBuilder.Build();
            ApplySavedLiteDbPaths();
        }

        private void ApplySavedLiteDbPaths()
        {
            if (_host.Services.GetService(typeof(GenerationOptionsStore)) is not GenerationOptionsStore store)
                return;

            if (File.Exists(store.PatientsDatabasePath))
                UpdateSharedStorage(store.PatientsDatabasePath, true);

            if (File.Exists(store.PlannedStudiesDatabasePath))
                UpdatePlannedStorage(store.PlannedStudiesDatabasePath, true);
        }
                
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var licenseService = new LicenseService();
                var licenseResult = licenseService.Check();

                var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
                mainViewModel.InitializeApplication(licenseResult);

                if (licenseResult.IsValid)
                {
                    _logger.Info($"Лицензия проверена: {licenseResult.State}");
                }
                else
                {
                    _logger.Warn($"Лицензия недействительна: {licenseResult.State}");
                }

                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                mainWindow.Closed += (_, _) => Shutdown();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при запуске приложения.");
                MessageBox.Show($"Ошибка при запуске приложения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }

        public static void UpdateSharedStorage(string dbPath, bool readOnly)
        {
            SharedStorage?.Dispose();
            SharedStorage = new LiteDbStudyStorageModule(dbPath, readOnly);
            IsSharedStorageReadOnly = readOnly;
            _logger.Info($"SharedStorage updated: {dbPath}, ReadOnly: {readOnly}");

            SharedStorageModeChanged?.Invoke(readOnly);
        }

        public static IStudyStorageModule GetSharedStorage()
        {
            if (SharedStorage == null)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "LiteDataBase", "patients.db");
                SharedStorage = new LiteDbStudyStorageModule(dbPath, true);
            }
            return SharedStorage;
        }

        public static void UpdatePlannedStorage(string dbPath, bool readOnly)
        {
            PlannedStorage?.Dispose();
            PlannedStorage = new LiteDbPlannedStudyStorageModule(dbPath, readOnly);
            _logger.Info($"PlannedStorage updated: {dbPath}, ReadOnly: {readOnly}");
        }

        public static IPlannedStudyStorageModule GetPlannedStorage()
        {
            if (PlannedStorage == null)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "LiteDataBase", "planned_studies.db");
                PlannedStorage = new LiteDbPlannedStudyStorageModule(dbPath, true);
            }
            return PlannedStorage;
        }
    }
}
