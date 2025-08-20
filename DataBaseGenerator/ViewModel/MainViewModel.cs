using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using DataBaseGenerator.UI.Wpf.View;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Random _random = new Random();
        private readonly BaseGenerateContext _context;
        private readonly IHttpClientFactory _httpClient;
        private readonly PatientService _patientService;
        private readonly WorklistService _worklistService;
        private RandomModalityRule _modality;
        private DialogMessageWindow _dialogMessage = new DialogMessageWindow();
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private SpecificationWindow _specificationWindow = new SpecificationWindow();
        //private List<Patient> _allPatients = new List<Patient>();
        private List<WorkList> _allWorkLists = new List<WorkList>();
        private string _updateText;
        private int _setPatientCount;
        private int _setWorkListCount;
        private string _aeTitle;
        private string _gender;
        private string _addIdPatient;
        private string _addFamily;
        private string _addName;
        private string _addMiddleName;
        private string _addAdress;
        private string _addWorkPlase;
        private string _addInfo;
        private string _medInsurNumber;
        private string _resourceAudioDir = "Resources\\NoNo.mp3";
        private string _godFatherAudioDir = "Resources\\GodFatherAudio.mp3";
        private string _dialogMessageDir = "Resources\\333.jpg";
        private string _specificationWindowDir = "Resources\\Specification.jpg";
        private string _iconDir = "Resources\\DBGenerator.ico";


        [ObservableProperty]
        private string _exePath;

        [ObservableProperty]
        private string _exeDirectory;

        [ObservableProperty]
        private string _currentDirectory;

        [ObservableProperty]
        private string _pathToResourceAudio;

        [ObservableProperty]
        private string _pathToGodFatherAudio;

        [ObservableProperty]
        private string _pathToResourceForDialogMessage;

        [ObservableProperty]
        private string _pathToResourceForSpecificationWindow;

        [ObservableProperty]
        private string _pathToIcon;

        public string UpdateText
        {
            get
            {
                return _updateText;
            }
            set
            {
                SetProperty(ref _updateText, value);
            }
        }

        public int SetPatientCount
        {
            get
            {
                return _setPatientCount;
            }

            set
            {
                SetProperty(ref _setPatientCount, value);
            }
        }

        public int SetWorkListCount
        {
            get
            {
                return _setWorkListCount;
            }

            set
            {
                SetProperty(ref _setWorkListCount, value);
            }
        }

        public ObservableCollection<RandomModalityRule> ModalityRules { get; }

        public RandomModalityRule SelectModality
        {
            get
            {
                return _modality;
            }

            set
            {
                SetProperty(ref _modality, value);
            }
        }

        public string SetAeTitle
        {
            get
            {
                return _aeTitle;
            }

            set
            {
                SetProperty(ref _aeTitle, value);
            }
        }

        public string AddIdPatient
        {
            get => _addIdPatient;
            set
            {
                SetProperty(ref _addIdPatient, value);
            }
        }

        public string AddFamily
        {
            get => _addFamily;
            set
            {
                SetProperty(ref _addFamily, value);
            }
        }

        public string AddName
        {
            get => _addName;
            set
            {
                SetProperty(ref _addName, value);
            }
        }

        public string AddMiddleName
        {
            get => _addMiddleName;
            set
            {
                SetProperty(ref _addMiddleName, value);
            }
        }

        public List<string> Gender { get; }

        public string SelecedGender
        {
            get => _gender;

            set
            {
                if (value == Gender[0])
                {
                    value = "M";
                }
                else if (value == Gender[1])
                {
                    value = "O";
                }
                else if (value == Gender[2])
                {
                    value = "F";
                }
                SetProperty(ref _gender, value);
            }
        }

        public string AddAdress
        {
            get => _addAdress;
            set
            {
                SetProperty(ref _addAdress, value);
            }
        }

        public string AddWorkPlase
        {
            get => _addWorkPlase;
            set
            {
                SetProperty(ref _addWorkPlase, value);
            }
        }

        public string AddInfo
        {
            get => _addInfo;
            set
            {
                SetProperty(ref _addInfo, value);
            }
        }

        public string MedicalInsuranceNumber
        {
            get => _medInsurNumber;
            set => SetProperty(ref _medInsurNumber, value);
        }

        public DateTime? _patientBirthDate = DateTime.Now;

        public DateTime? PatientBirthDate
        {
            get => _patientBirthDate;
            set
            {
                if (SetProperty(ref _patientBirthDate, value))
                    UpdateText = value > DateTime.Now
                     ? "Привет из будущего, Вася !"
                     : string.Empty;

                var age = DateTime.Now.Year - value?.Year;
                BirthDateToolTip = age > 120
                    ? "Похоже, это участник съёмок Титаника"
                    : null;
            }
        }

        private string _birthDateToolTip;
        public string BirthDateToolTip
        {
            get => _birthDateToolTip;
            set => SetProperty(ref _birthDateToolTip, value);
        }

        [ObservableProperty]
        public partial ObservableCollection<Patient> AllPatients { get; set; }

        public List<WorkList> AllWorkLists
        {
            get => _allWorkLists;
            set => SetProperty(ref _allWorkLists, value);
        }

        [ObservableProperty]
        public partial Patient SelectedPatient {  get; set; }

        [ObservableProperty]
        public partial bool UseEngNames { get; set; }

        [ObservableProperty]
        public partial bool UseRusNames { get; set; }

        [ObservableProperty]
        public partial bool UseChinaNames { get; set; }

        [ObservableProperty]
        public partial bool UseAge0_17 { get; set; }

        [ObservableProperty]
        public partial bool UseAge18_60 { get; set; }

        [ObservableProperty]
        public partial bool UseAge61_120 { get; set; }

        [ObservableProperty]
        public partial bool UseRandomBirthdate { get; set; }

        [ObservableProperty]
        public partial bool UseMissingBirthdate { get; set; }

        [ObservableProperty]
        public partial bool UseFutureBirthdate { get; set; }

        [ObservableProperty]
        public partial bool UseEmptyStrings { get; set; }

        [ObservableProperty]
        public partial bool UseLongValues { get; set; }

        [ObservableProperty]
        public partial bool UseSpecialChars { get; set; }



        public MainViewModel(BaseGenerateContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _httpClient = clientFactory;

            var defaultAeTitle = new RandomModalityRule("DX");
            ModalityRules = new ObservableCollection<RandomModalityRule>
            {
                defaultAeTitle,
                new RandomModalityRule("MG")
            };

            SelectModality = defaultAeTitle;

            Gender = new List<string> { "Man", "Female", "Other" };

            _patientService = new PatientService(_httpClient);
            _worklistService = new WorklistService(_httpClient);

            InitializeDirectories();
            _ = InitializeAsync();
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

        private async Task InitializeAsync()
        {
            AllPatients = new();
            UseRandomBirthdate = true;

            AllPatients = await _patientService.GetAllAsync();
            AllWorkLists = await _worklistService.GetAllAsync();
        }


        [RelayCommand]
        public async Task AddPatientAsync()
        {
            try
            {
                var newPatient = new PatientGeneratorParameters(
                    new OrderIdPatientRule(),
                    new RandomLastNameRule(),
                    new RandomFirstNameRule(),
                    new RandomMiddleNameRule(),
                    new OrderPatientIdRule(),
                    new RandomBirthDateRule(new DateTime()),
                    new RandomSexRule(),
                    new RandomAddressRule(),
                    new RandomAddInfoRule(),
                    new RandomOccupationRule())
                {
                    PatientCount = SetPatientCount,

                    NamesRusGeneratorRule = UseRusNames,
                    NamesEngGeneratorRule = UseEngNames,
                    NamesChinaGeneratorRule = UseChinaNames,

                    RandomBirthdateGeneratorRule = UseRandomBirthdate,
                    MissingBirthdateGeneratorRule = UseMissingBirthdate,
                    FutureBirthdateGeneratorRule = UseFutureBirthdate,
                    Age0_17_GeneratorRule = UseAge0_17,
                    Age18_60_GeneratorRule = UseAge18_60,
                    Age61_120_GeneratorRule = UseAge61_120,

                    EmptyStringsGeneratorRule = UseEmptyStrings,
                    LongValuesGeneratorRule = UseLongValues,
                    SpecialCharsGeneratorRule = UseSpecialChars
                };

                if (SetPatientCount is 404 or 500 or 777)
                {
                    StopGenerateMessage(SetPatientCount);
                    SetPatientCount = 0;

                    return;
                }

                if (SetPatientCount == 13)
                {
                    RandomMessageFor13Patient();
                    SetPatientCount = 0;

                    return;
                }

                await _patientService.GenerateAsync(newPatient);

                await RefreshPatientsAsync();
                LolMessageForPatientCount(SetPatientCount);

                UpdateText = "Пациент успешно добавлен";
            }
            catch (Exception ex)
            {
                UpdateText = "Пациент не добавлен";
                _logger.Error(ex, "Error in patient generation");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task SavePatientsAsync()
        {
            try
            {
                var updatePatients = new ObservableCollection<Patient>();
                foreach (var patient in AllPatients)
                {
                    updatePatients.Add(patient);
                }

                await _patientService.EditeAsync(updatePatients);
                _logger.Info("All changes saved to database");
                UpdateText = "All changes saved to database";
            }
            catch (Exception ex)
            {
                UpdateText = "Error saving changes";
                _logger.Error(ex, "Error saving changes");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task RefreshPatientsAsync()
        {
            AllPatients = await _patientService.GetAllAsync();
            MainWindow.AllPatientView.ItemsSource = null;
            MainWindow.AllPatientView.Items.Clear();
            MainWindow.AllPatientView.ItemsSource = AllPatients;
            MainWindow.AllPatientView.Items.Refresh();
            UpdateText = "Patient table is update";
        }

        [RelayCommand]
        public void CleanCheckboxes()
        {
            UseEngNames = false;
            UseRusNames = false;
            UseChinaNames = false;
            UseAge0_17 = false;
            UseAge18_60 = false;
            UseAge61_120 = false;
            UseRandomBirthdate = true;
            UseMissingBirthdate = false;
            UseFutureBirthdate = false;
            UseEmptyStrings = false;
            UseLongValues = false;
            UseSpecialChars = false;
        }

        [RelayCommand]
        public async Task DeleteFirstPatientAsync()
        {
            try
            {
                var patient = new PatientGeneratorParameters(
                   new OrderIdPatientRule(),
                   new RandomLastNameRule(),
                   new RandomFirstNameRule(),
                   new RandomMiddleNameRule(),
                   new OrderPatientIdRule(),
                   new RandomBirthDateRule(new DateTime()),
                   new RandomSexRule(),
                   new RandomAddressRule(),
                   new RandomAddInfoRule(),
                   new RandomOccupationRule())
                {
                    PatientCount = SetPatientCount
                };

                await _patientService.DeleteFirstAsync();
                await RefreshPatientsAsync();

                UpdateText = "First Patient is Delete";
            }
            catch (Exception ex)
            {
                UpdateText = "Patient is not Deleted";
                _logger.Error(ex, "Patient is not Deleted");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteAllPatientAsync()
        {
            try
            {
                var patient = new PatientGeneratorParameters(
                   new OrderIdPatientRule(),
                   new RandomLastNameRule(),
                   new RandomFirstNameRule(),
                   new RandomMiddleNameRule(),
                   new OrderPatientIdRule(),
                   new RandomBirthDateRule(new DateTime()),
                   new RandomSexRule(),
                   new RandomAddressRule(),
                   new RandomAddInfoRule(),
                   new RandomOccupationRule())
                {
                    PatientCount = SetPatientCount
                };

                await _patientService.DeleteAllAsync();
                await RefreshPatientsAsync();

                UpdateText = "Patient Table Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Patient Table is not Deleted";
                _logger.Error(ex, "Patient Table is not Deleted");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task AddWorkListAsync()
        {
            try
            {
                var newWorkList = new WorkListGeneratorDto(
                    new OrderIdWorklistRule(),
                    new RandomCreateDateRule(),
                    new RandomCreateTimeRule(),
                    new RandomCompleteDateRule(),
                    new RandomCompleteTimeRule(),
                    new OrderIdPatientWlRule(),
                    new RandomStateRule(),
                    new RandomSOPInstanceUIDRule(),
                    SelectModality,
                    new RandomStationAeTitleRule(_aeTitle),
                    new RandomProcedureStepStartDateTimeRule(),
                    new RandomPerformingPhysiciansNameRule(),
                    new RandomStudyDescriptionRule(),
                    new RandomReferringPhysiciansNameRule(),
                    new RandomRequestingPhysicianRule())
                {
                    WorkListCount = SetWorkListCount
                };

                await _worklistService.GenerateAsync(newWorkList);

                await RefreshWorkListAsync();

                UpdateText = "WorkList added";
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList not added";
                _logger.Error(ex, "WorkList not added");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task RefreshWorkListAsync()
        {
            AllWorkLists = await _worklistService.GetAllAsync();
            MainWindow.AllWorkListView.ItemsSource = null;
            MainWindow.AllWorkListView.Items.Clear();
            MainWindow.AllWorkListView.ItemsSource = AllWorkLists;
            MainWindow.AllWorkListView.Items.Refresh();
            UpdateText = "WorkList table is update";
        }

        [RelayCommand]
        public async Task DeleteFirstWorkListAsync()
        {
            try
            {
                var workList = new WorkListGeneratorParameters(
                    new OrderIdWorklistRule(),
                    new RandomCreateDateRule(),
                    new RandomCreateTimeRule(),
                    new RandomCompleteDateRule(),
                    new RandomCompleteTimeRule(),
                    new OrderIdPatientWlRule(),
                    new RandomStateRule(),
                    new RandomSOPInstanceUIDRule(),
                    SelectModality,
                    new RandomStationAeTitleRule(_aeTitle),
                    new RandomProcedureStepStartDateTimeRule(),
                    new RandomPerformingPhysiciansNameRule(),
                    new RandomStudyDescriptionRule(),
                    new RandomReferringPhysiciansNameRule(),
                    new RandomRequestingPhysicianRule())
                {
                    WorkListCount = SetWorkListCount
                };

                await _worklistService.DeleteFirstAsync();
                await RefreshWorkListAsync();

                UpdateText = "First in WorkList Delete";
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList is not Deleted";
                _logger.Error(ex, "WorkList is not Deleted");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteAllWorkListAsync()
        {
            try
            {
                var workList = new WorkListGeneratorParameters(
                    new OrderIdWorklistRule(),
                    new RandomCreateDateRule(),
                    new RandomCreateTimeRule(),
                    new RandomCompleteDateRule(),
                    new RandomCompleteTimeRule(),
                    new OrderIdPatientWlRule(),
                    new RandomStateRule(),
                    new RandomSOPInstanceUIDRule(),
                    SelectModality,
                    new RandomStationAeTitleRule(_aeTitle),
                    new RandomProcedureStepStartDateTimeRule(),
                    new RandomPerformingPhysiciansNameRule(),
                    new RandomStudyDescriptionRule(),
                    new RandomReferringPhysiciansNameRule(),
                    new RandomRequestingPhysicianRule())
                {
                    WorkListCount = SetWorkListCount
                };

                await _worklistService.DeleteAllAsync();
                await RefreshWorkListAsync();

                UpdateText = "WorkList Table Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList Table is not Deleted";
                _logger.Error(ex, "WorkList Table is not Deleted");
                MessageBox.Show($"{ex.Message}");
            }
        }

        [RelayCommand]
        public async Task ConnectDB()
        {
            var connectingState = await _patientService.ConnectingEchoAsync();
            if (connectingState)
            {
                MessageBox.Show("Соединение с БД установлено",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к БД",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        public async Task DeleteAllTablesAsync()
        {
            try
            {
                await DeleteAllPatientAsync();
                await RefreshPatientsAsync();

                await DeleteAllWorkListAsync();
                await RefreshWorkListAsync();

                UpdateText = "All Tables Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Tables is not Deleted";
            }
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
            _mediaPlayer.Open(new Uri(PathToResourceAudio));
            _mediaPlayer.Play();
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

        [RelayCommand]
        public void CancelAddPatient()
        {
            AddIdPatient = string.Empty;

            AddFamily = string.Empty;

            AddName = string.Empty;

            AddMiddleName = string.Empty;

            AddAdress = string.Empty;

            AddWorkPlase = string.Empty;

            AddInfo = string.Empty;

            SelecedGender = null;

            MedicalInsuranceNumber = string.Empty;

            UpdateText = "Как скажете, отменяю !";
        }        

        [RelayCommand]
        public async Task AddOnePatientAsync()
        {
            var messageToUpdateText = string.Empty;

            try
            {
                var newPatient = new PatientInputParameters(
                    1,
                    AddFamily,
                    AddName,
                    AddMiddleName,
                    AddIdPatient,
                    PatientBirthDate,
                    SelecedGender,
                    AddAdress,
                    AddInfo,
                    AddWorkPlase)
                {
                    PatientCount = SetPatientCount
                };

                await _patientService.AddOneAsync(newPatient);

                await RefreshPatientsAsync();
                CleareFields();                

                UpdateText = !string.IsNullOrEmpty(messageToUpdateText)
                    ? messageToUpdateText
                    : "Patient added";
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(AddFamily) || string.IsNullOrEmpty(AddName) || string.IsNullOrEmpty(AddMiddleName))
                {
                    UpdateText = "Создан пациент-призрак. Поздравляю!";
                    CleareFields();

                    return;
                }

                CleareFields();
                UpdateText = !string.IsNullOrEmpty(messageToUpdateText)
                    ? messageToUpdateText
                    : "Пациент не добавлен";
            }
        }

        [RelayCommand]
        public void OpenWebPage()
        {
            var url = "http://localhost:5289"; // или любой нужный маршрут
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // важно для Windows — откроет в браузере
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть веб-страницу: " + ex.Message);
            }
        }

        private readonly List<string> _thirteenPhrases = new()
        {
            "Ты серьёзно выбрал 13? Прямо просишь багов.",
            "Пациент №13 отказался проходить регистрацию. У него совещание в потустороннем.",
            "Тринадцатый? Только если ты хочешь активировать скрытый режим CHAOS.",
            "Ошибка: пациент 13 оказался иллюзией.",
            "О, нет. Не снова это число. Попробуй ещё раз.",
            "Пациент №13 уже давно исчез... вместе с тестировщиком.",
            "Ты на волоске от пробуждения древнего бага.",
            "Система отказывается генерировать 13. Потому что… ну, 13."
        };

        public void RandomMessageFor13Patient()
        {
            var randomIndex = _random.Next(_thirteenPhrases.Count);
            var message = _thirteenPhrases[randomIndex];

            MessageBox.Show(
                message,
                "🛑 Нечёткое предчувствие",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public void LolMessageForPatientCount(int count)
        {
            if (count == 42)
            {
                MessageBox.Show(
                    "42 — ответ на главный вопрос жизни, Вселенной и всего такого.",
                    "😎 Deep Thought",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (count == 69)
            {
                MessageBox.Show(
                    "Хватит хихикать. Это просто число… наверное. Психолог тебе позвонит позже.",
                    "😏",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (count >= 100)
            {
                MessageBox.Show(
                    "🎉 Ачивка разблокирована: 'Мастер регистрации' \n\r        🏆 Сотый пациент, мои поздравления 🏆",
                    "Congratulations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        public void StopGenerateMessage(int count)
        {
            if (count == 404)
            {
                MessageBox.Show(
                    "Ошибка 404: Пациенты не найдены.",
                    "404 — Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            if (count == 500)
            {
                MessageBox.Show(
                    "Что ты натворил? Код 500 пошёл гулять.",
                    "500 — Internal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            if (count == 777)
            {
                MessageBox.Show(
                    "🎰 Джекпот! 777 пациентов. Выиграл выходной!",
                    "777 — Слот-машина",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private void CleareFields()
        {
            AddIdPatient = string.Empty;
            AddFamily = string.Empty;
            AddName = string.Empty;
            AddMiddleName = string.Empty;
            SelecedGender = null;
            AddAdress = string.Empty;
            AddWorkPlase = string.Empty;
            AddInfo = string.Empty;
            MedicalInsuranceNumber = string.Empty;
        }

    }
}
