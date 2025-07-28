using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using DataBaseGenerator.UI.Wpf.View;
using DataBaseGenerator.UI.Wpf.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataBaseGenerator.UI.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host; 
        private readonly BaseGenerateContext _context;
        const string _exeName = "DataBaseGenerator.Web.exe";
        const string _webHostPath = "..\\..\\..\\WebHost\\DataBaseGenerator.Web.exe";
        //const string _webHostPath = "C:\\Program Files (x86)\\DBGeneratorBroken\\WebHost\\DataBaseGenerator.Web.exe";


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

            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddDbContext<BaseGenerateContext>(options =>
                    options.UseMySql(
                        context.Configuration.GetConnectionString("DefaultConnection"),
                        new MySqlServerVersion(new Version(8, 0, 28))));

                    services.AddScoped<IPatientService, PatientService>();
                    services.AddScoped<IWorklistService, WorklistService>();

                    services.AddSingleton(this);
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<DialogMessageWindow>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<SpecificationWindow>();
                });

            _host = hostBuilder.Build();

            StartWebHost();
        }

        private void StartWebHost()
        {
            bool isRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_exeName)).Any();
            if (!isRunning)
            {
                try
                {
                    var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _webHostPath);
                    if (File.Exists(fullPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = fullPath,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Не найден файл: {fullPath}", "Ошибка запуска веб-хоста");
                    }
                }
                catch (Exception ex)
                {
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

                mainWindow.Closed += (_, _) => Shutdown();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Check your connection settings !!!");                
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
                process.Kill();
            }
        }


    }
}
