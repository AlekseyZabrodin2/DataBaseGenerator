using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
using DataBaseGenerator.UI.Wpf.UserControls;
using DataBaseGenerator.UI.Wpf.View;
using DataBaseGenerator.UI.Wpf.ViewModel.LiteDbTabs;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class  MainViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private DialogMessageWindow _dialogMessage = new DialogMessageWindow();
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private SpecificationWindow _specificationWindow = new SpecificationWindow(); 
        private readonly IServiceProvider _serviceProvider;


        [ObservableProperty]
        public partial UserControl CurrentPage { get; set; }

        [ObservableProperty]
        public partial string CurrentPageName { get; set; }

        [ObservableProperty]
        public partial string ExePath { get; set; }

        [ObservableProperty]
        public partial string ExeDirectory { get; set; }

        [ObservableProperty]
        public partial string CurrentDirectory { get; set; }

        [ObservableProperty]
        public partial string PathToResourceAudio { get; set; }

        [ObservableProperty]
        public partial string PathToGodFatherAudio { get; set; }

        [ObservableProperty]
        public partial string PathToResourceForDialogMessage { get; set; }

        [ObservableProperty]
        public partial string PathToResourceForSpecificationWindow { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<PatientDatabaseTab> DatabaseTabs { get; set; }

        [ObservableProperty]
        public partial PatientDatabaseTab SelectedDatabaseTab { get; set; }

        [ObservableProperty]
        public partial bool IsMySqlMode { get; set; } = true;

        [ObservableProperty]
        public partial bool IsLiteDbMode { get; set; }

        [ObservableProperty]
        public partial bool IsLiteDbPlannedStudiesMode { get; set; }

        [ObservableProperty]
        public partial bool IsLiteDbViewerMode { get; set; }

        [ObservableProperty]
        public partial bool ShowPatients { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowStudies { get; set; }

        [ObservableProperty]
        public partial bool ShowSeries { get; set; }

        [ObservableProperty]
        public partial bool ShowImages { get; set; }

        [ObservableProperty]
        public partial string PathToIcon { get; set; }

        [ObservableProperty]
        public partial bool? IsWebHostOnline { get; set; }

        [ObservableProperty]
        public partial bool? IsMySqlOnline { get; set; }

        private readonly IConfiguration _configuration;
        private int _statusCheckRunning;
        private bool _startupCheckPending = true;

        private string _resourceAudioDir = "MySqlGenerator\\Resources\\NoNo.mp3";
        private string _godFatherAudioDir = "MySqlGenerator\\Resources\\GodFatherAudio.mp3";
        private string _dialogMessageDir = "MySqlGenerator\\Resources\\333.jpg";
        private string _specificationWindowDir = "MySqlGenerator\\Resources\\Specification.jpg";
        private string _iconDir = "MySqlGenerator\\Resources\\DBGenerator.ico";



        public MainViewModel(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            DatabaseTabs = new();
            _ = InitializeDatabaseAsync();

            SwitchToMySql();

            InitializeDirectories();
        }

        private void RequestServiceStatus()
        {
            var retryWebHost = _startupCheckPending;
            _startupCheckPending = false;
            _ = RefreshStatusAsync(retryWebHost);
        }

        private async Task RefreshStatusAsync(bool retryWebHost)
        {
            if (Interlocked.Exchange(ref _statusCheckRunning, 1) == 1)
                return;

            try
            {
                IsWebHostOnline = null;
                IsMySqlOnline = null;

                var webHostTask = WebHostRespondsAsync(retryWebHost);
                var mySqlTask = MySqlRespondsAsync();
                await Task.WhenAll(webHostTask, mySqlTask);

                IsWebHostOnline = await webHostTask;
                IsMySqlOnline = await mySqlTask;
            }
            finally
            {
                Interlocked.Exchange(ref _statusCheckRunning, 0);
            }
        }

        private async Task<bool> WebHostRespondsAsync(bool retry)
        {
            var attempts = retry ? 4 : 1;

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                if (Process.GetProcessesByName("DataBaseGenerator.Web").Length > 0)
                    return true;

                if (attempt < attempts)
                    await Task.Delay(1000);
            }

            return false;
        }

        private async Task<bool> MySqlRespondsAsync()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString)
                {
                    ConnectionTimeout = 2
                };

                await using var connection = new MySqlConnection(builder.ConnectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "MySQL status check failed");
                return false;
            }
        }


        private async Task InitializeDatabaseAsync()
        {
            var defaultDbPath = Path.Combine(AppContext.BaseDirectory, "LiteDataBase", "patients.db");

            var dataDir = Path.GetDirectoryName(defaultDbPath);
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            if (!File.Exists(defaultDbPath))
            {
                CreateEmptyDatabase(defaultDbPath);
            }

            var dbTab = new PatientDatabaseTab(defaultDbPath);
            DatabaseTabs.Add(dbTab);
            SelectedDatabaseTab = dbTab;

            await Task.CompletedTask;
        }

        private void CreateEmptyDatabase(string dbPath)
        {
            using var storage = new LiteDbStudyStorageModule(dbPath, false);
            _ = storage.Patients;
            _ = storage.Studies;
            _ = storage.Series;
            _ = storage.Images;

            _logger.Info($"Created new database at {dbPath}");
        }

        private void InitializeDirectories()
        {
            ExePath = Process.GetCurrentProcess().MainModule.FileName;
            _logger.Trace("ExePath - {0}", ExePath);

            ExeDirectory = Path.GetDirectoryName(ExePath) ?? string.Empty;
            _logger.Trace("Exe directory - {0}", ExeDirectory);

            PathToResourceAudio = Path.Combine(ExeDirectory, _resourceAudioDir);
            _logger.Trace("PathToResourceAudio - {0}", PathToResourceAudio);

            PathToGodFatherAudio = Path.Combine(ExeDirectory, _godFatherAudioDir);
            _logger.Trace("PathToGodFatherAudio - {0}", PathToGodFatherAudio);

            PathToResourceForDialogMessage = Path.Combine(ExeDirectory, _dialogMessageDir);
            _logger.Trace("PathToResourceForDialogMessage - {0}", PathToResourceForDialogMessage);

            PathToResourceForSpecificationWindow = Path.Combine(ExeDirectory, _specificationWindowDir);
            _logger.Trace("PathToResourceForSpecificationWindow - {0}", PathToResourceForSpecificationWindow);

            PathToIcon = Path.Combine(ExeDirectory, _iconDir);
            _logger.Trace("PathToIcon - {0}", PathToIcon);
        }



        [RelayCommand]
        public void AboutProgram()
        {
            _dialogMessage.DataContext = this;
            _dialogMessage.ShowDialog();
        }

        [RelayCommand]
        public void ClosingDialogWindow()
        {
            //_mediaPlayer.Open(new Uri(PathToResourceAudio));
            //_mediaPlayer.Play();
            _dialogMessage.Close();
        }

        [RelayCommand]
        public void HotkeyForDialogWindow()
        {
            MessageBox.Show("Отличная попытка ДРУЖИЩЕ", "ага )))", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        [RelayCommand]
        public void HotkeyExitFromProgram()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand]
        public void OpenSpecificationWindow()
        {
            _specificationWindow.DataContext = this;
            _specificationWindow.Show();
        }

        [RelayCommand]
        public void CloseSpecificationWindow()
        {
            _specificationWindow.Close();
        }

        [RelayCommand]
        public void ToolMessage()
        {
            MessageBox.Show("Ну я же просил !!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private MySqlGeneratorUserControl _mySqlPage;
        private LiteDbGeneratorUserControl _patientsPage;
        private StudiesLiteDbTableUserControl _studiesPage;
        private PlannedStudiesLiteDbUserControl _plannedPage;
        private LiteDbViewerUserControl _viewerPage;

        [RelayCommand]
        private void SwitchToMySql()
        {
            LeaveLiteDbViewer();
            _mySqlPage ??= new MySqlGeneratorUserControl();
            CurrentPage = _mySqlPage;
            CurrentPageName = "MySql Generator";
            IsMySqlMode = true;
            IsLiteDbMode = false;
            IsLiteDbPlannedStudiesMode = false;
            ShowPatients = false;
            ShowStudies = false;
            RequestServiceStatus();
        }
        
        [RelayCommand]
        private void ShowPatientsTable()
        {
            _patientsPage ??= new LiteDbGeneratorUserControl();

            ShowPatients = true;
            ShowStudies = false;
            ShowSeries = false;
            ShowImages = false;

            ShowTable("Patients", _patientsPage);
        }

        [RelayCommand]
        private void ShowStudiesTable()
        {
            _studiesPage ??= new StudiesLiteDbTableUserControl();

            ShowPatients = false;
            ShowStudies = true;
            ShowSeries = false;
            ShowImages = false;

            ShowTable("Studies", _studiesPage);
        }        

        [RelayCommand]
        private void SwitchToPlannedStudies()
        {
            LeaveLiteDbViewer();
            _plannedPage ??= new PlannedStudiesLiteDbUserControl();
            CurrentPage = _plannedPage;
            CurrentPageName = "LiteDb - Planned Studies";
            IsMySqlMode = false;
            IsLiteDbMode = false;
            IsLiteDbPlannedStudiesMode = true;
            ShowPatients = false;
            ShowStudies = false;
            RequestServiceStatus();
        }

        [RelayCommand]
        private void ShowLiteDbViewer()
        {
            if (IsLiteDbOperationRunning())
            {
                MessageBox.Show(
                    "Сейчас идёт генерация или другая операция с LiteDB. Просмотр можно открыть, когда она закончится.",
                    "Просмотр LiteDB",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                SwitchOpenDatabasesToReadOnly();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Не удалось перевести LiteDB в режим только для чтения");
                MessageBox.Show(
                    $"Не удалось перевести базы в режим только для чтения.\r\n{ex.Message}",
                    "Просмотр LiteDB",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _viewerPage ??= new LiteDbViewerUserControl();
            _viewerPage.ViewModel.LoadOpenFiles();
            CurrentPage = _viewerPage;
            CurrentPageName = "Просмотр LiteDB";
            IsMySqlMode = false;
            IsLiteDbMode = false;
            IsLiteDbPlannedStudiesMode = false;
            ShowPatients = false;
            ShowStudies = false;
            IsLiteDbViewerMode = true;
            RequestServiceStatus();
        }

        private void ShowTable(string tableName, UserControl control)
        {
            LeaveLiteDbViewer();
            CurrentPage = control;
            CurrentPageName = $"LiteDb - {tableName}";
            IsMySqlMode = false;
            IsLiteDbMode = true;
            IsLiteDbPlannedStudiesMode = false;
            RequestServiceStatus();
        }

        private bool IsLiteDbOperationRunning()
        {
            return _patientsPage?.ViewModel.IsBusy == true
                || _studiesPage?.ViewModel.IsBusy == true
                || _plannedPage?.ViewModel.IsBusy == true;
        }

        private void SwitchOpenDatabasesToReadOnly()
        {
            if (!App.IsSharedStorageReadOnly && App.SharedStorage != null)
                App.UpdateSharedStorage(App.SharedStorage.DatabasePath, true);

            var planned = _plannedPage?.ViewModel;
            if (planned != null && !planned.IsReadOnlyMode && App.PlannedStorage != null)
            {
                App.UpdatePlannedStorage(App.PlannedStorage.DatabasePath, true);
                planned.IsReadOnlyMode = true;
            }
        }

        private void LeaveLiteDbViewer()
        {
            _viewerPage?.ViewModel.ReleaseExternal();
            IsLiteDbViewerMode = false;
        }

    }
}
