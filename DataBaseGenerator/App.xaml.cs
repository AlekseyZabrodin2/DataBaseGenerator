using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
using DataBaseGenerator.Core.LiteDbGenerator.Services;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.Data;
using DataBaseGenerator.UI.Wpf.View;
using DataBaseGenerator.UI.Wpf.ViewModel;
using Microsoft.EntityFrameworkCore;
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
        private readonly BaseGenerateContext _context;
        const string _exeName = "DataBaseGenerator.Web.exe";
        const string _webHostPath = "WebHost\\DataBaseGenerator.Web.exe";
        private string _exeToRun;


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

                    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                    _logger.Info($"ConnectionString = {connectionString}");

                    services.AddDbContext<BaseGenerateContext>(options =>
                    options.UseMySql(
                        context.Configuration.GetConnectionString("DefaultConnection"),
                        new MySqlServerVersion(new Version(8, 0, 28))));

                    services.AddScoped<PatientService>();
                    services.AddScoped<WorklistService>();

                    services.AddSingleton<IStudyStorageModule>(sp =>
                    {
                        //var dataDirectory = "D:\\Develop\\UniExpert\\Build\\Debug\\Data";

                        //if (!Directory.Exists(dataDirectory))
                        //{
                            var dataDirectory = Path.Combine(AppContext.BaseDirectory, "LiteDataBase");
                            Directory.CreateDirectory(dataDirectory);
                        //}

                        var dbPath = Path.Combine(dataDirectory, "patients.db");
                        return new LiteDbStudyStorageModule(dbPath, false);
                    })
                    .AddSingleton<IStudyApplicationService, StudyApplicationService>();

                    services.AddSingleton(this);
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MySqlGeneratorViewModel>();
                    services.AddSingleton<LiteDbGeneratorViewModel>();
                    services.AddTransient<DialogMessageWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<SpecificationWindow>();

                    services.AddHttpClient("DBGeneratorApi", client =>
                    {
                        var webHostApiUrl = context.Configuration["WebHost:ApiUri"] ?? "http://localhost:5289/api/";

                        client.BaseAddress = new Uri(webHostApiUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    });
                });

            _host = hostBuilder.Build();

            EnsureSingleWebHostProcessRunning();
        }

        private void EnsureSingleWebHostProcessRunning()
        {
            StopWebHost();
            StartWebHost();
        }

        private void StartWebHost()
        {
            bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_exeName)).Any();
            if (!isRunning)
            {
                try
                {
                    var exeDir = GetExeDirectory();
                    var pathNearExe = GetPathNearExe(exeDir);
                    var pathFromDebug = GetPathFromDebug(exeDir);

                    if (File.Exists(pathNearExe))
                    {
                        _exeToRun = pathNearExe;
                    }
                    else if (File.Exists(pathFromDebug))
                    {
                        _exeToRun= pathFromDebug;
                    }
                    if (_exeToRun != null)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = _exeToRun,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                        _logger.Info("WebHost started");
                    }
                    else
                    {
                        string allPaths = $"{pathNearExe} и {pathFromDebug}";
                        _logger.Warn($"Не найден файл: {allPaths}, Ошибка запуска веб-хоста");
                        MessageBox.Show($"Не найден файл: {allPaths}, Ошибка запуска веб-хоста");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Ошибка запуска веб-сервера");
                    MessageBox.Show($"Ошибка запуска веб-сервера: {ex.Message}");
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var mainWindow = new MainWindow();
                mainWindow.Show();
                _logger.Info("Main window is started");

                mainWindow.Closed += (_, _) => Shutdown();
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Нет подключения к БД");
                MessageBox.Show($"Нет подключения к БД.\r\n Некоторые функции будут недоступны.\r\n {ex.Message}",
                                "Ошибка подключения",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                base.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            StopWebHost();

            _host.Dispose();

            base.OnExit(e);
        }

        private void StopWebHost()
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_exeName));
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Can`t stop process {processId}", process.Id);
                }
                _logger.Info("WebHost stoped");
            }
        }

        private string GetExeDirectory()
        {
            var exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            _logger.Trace("exeDir - {0}", exeDir);

            return exeDir;
        }

        private string GetPathNearExe(string exeDir)
        {
            var pathNearExe = Path.Combine(exeDir, _webHostPath);
            _logger.Trace("pathNearExe - {0}", pathNearExe);

            return pathNearExe;
        }

        private string GetPathFromDebug(string exeDir)
        {
            var pathFromDebug = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..", _webHostPath));
            _logger.Trace("pathFromDebug - {0}", pathFromDebug);

            return pathFromDebug;
        }

    }
}
