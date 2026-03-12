using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.UI.Wpf.UserControls;
using DataBaseGenerator.UI.Wpf.View;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class  MainViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private DialogMessageWindow _dialogMessage = new DialogMessageWindow();
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private SpecificationWindow _specificationWindow = new SpecificationWindow();


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
        public partial string PathToIcon { get; set; }

        private string _resourceAudioDir = "MySqlGenerator\\Resources\\NoNo.mp3";
        private string _godFatherAudioDir = "MySqlGenerator\\Resources\\GodFatherAudio.mp3";
        private string _dialogMessageDir = "MySqlGenerator\\Resources\\333.jpg";
        private string _specificationWindowDir = "MySqlGenerator\\Resources\\Specification.jpg";
        private string _iconDir = "MySqlGenerator\\Resources\\DBGenerator.ico";

        public ICommand SwitchToMySqlCommand { get; }
        public ICommand SwitchToLiteDbCommand { get; }



        public MainViewModel()
        {
            SwitchToMySql();

            SwitchToMySqlCommand = new RelayCommand(SwitchToMySql);
            SwitchToLiteDbCommand = new RelayCommand(SwitchToLiteDb);

            InitializeDirectories();
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



        private void SwitchToMySql()
        {
            CurrentPage = new MySqlGeneratorUserControl();
            CurrentPageName = "MySql Generator";
        }

        private void SwitchToLiteDb()
        {
            CurrentPage = new LiteDbGeneratorUserControl();
            CurrentPageName = "LiteDb Generator";
        }


    }
}
