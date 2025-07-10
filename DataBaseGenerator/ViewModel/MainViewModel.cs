using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using DataBaseGenerator.UI.Wpf.View;
using MySqlConnector;
using Prism.Commands;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {

        public MainViewModel()
        {
            var defaultAeTitle = new RandomModalityRule("DX");
            ModalityRules = new ObservableCollection<RandomModalityRule>
            {
                defaultAeTitle,
                new RandomModalityRule("MG")
            };

            SelectModality = defaultAeTitle;

            Gender = new List<string> { "Man", "Female", "Other" };
        }


        private PatientGeneratorParameters _patientGeneratorParameters;
        private readonly RandomModalityRule _modalityRule;
        private MySqlConnection _myConnection;
        private MySqlCommand _mySqlCommand;
        private MySqlDataReader _dataReader;
        private MySqlDataAdapter _adapter;
        private DataTable _tablet;
        private string _connect = "Server=localhost;DataBase=medxregistry;Uid=root;pwd=root;";
        private string _updateText;
        private int _setPatientCount;
        private int _setWorkListCount;
        private RandomModalityRule _modality;
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
        private DialogMessageWindow _dialogMessage = new DialogMessageWindow();
        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private MainViewModel _mainViewModel;
        private SpecificationWindow _specificationWindow = new SpecificationWindow();
        private static readonly Random _random = new Random();

        [ObservableProperty]
        private string _pathToResourceAudio = "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator.Core\\Resources\\NoNo.mp3";
        //private string _pathToResourceAudio = "C:\\Program Files (x86)\\DBGeneratorBroken\\Resources\\NoNo.mp3";

        [ObservableProperty]
        private string _pathToGodFatherAudio = "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator.Core\\Resources\\GodFatherAudio.mp3";
        //private string _pathToGodFatherAudio = "C:\\Program Files (x86)\\DBGeneratorBroken\\Resources\\GodFatherAudio.mp3";

        [ObservableProperty]
        private string _pathToResourceForDialogMessage = "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator.Core\\Resources\\333.jpg";
        //private string _pathToResourceForDialogMessage = "C:\\Program Files (x86)\\DBGeneratorBroken\\Resources\\333.jpg";

        [ObservableProperty]
        private string _pathToResourceForSpecificationWindow = "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator.Core\\Resources\\Specification.jpg";
        //private string _pathToResourceForSpecificationWindow = "C:\\Program Files (x86)\\DBGeneratorBroken\\Resources\\Specification.jpg";

        [ObservableProperty]
        private string _pathToIcon = "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator.Core\\Resources\\DBGenerator.ico";
        //private string _pathToIcon = "C:\\Program Files (x86)\\DBGeneratorBroken\\Resources\\DBGenerator.ico";



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

        //[ObservableProperty]
        //public DateTime _patientBirthDate = DateTime.Today.AddYears(-100);

        public DateTime _patientBirthDate = DateTime.Now;

        public DateTime PatientBirthDate
        {
            get => _patientBirthDate;
            set
            {
                if (SetProperty(ref _patientBirthDate, value))
                    UpdateText = value > DateTime.Now
                     ? "Привет из будущего, Вася !"
                     : string.Empty;

                var age = DateTime.Now.Year - value.Year;
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


        private DelegateCommand _connectDB;
        public ICommand ConnectDB => _connectDB = new DelegateCommand(PerformConnectDB);

        private void PerformConnectDB()
        {
            MessageBox.Show("Ну да конечно, хахахах !!!");
            MessageBox.Show("Попробуй еще раз !!!");
            MessageBox.Show("Не останавливайся, ты уже так далеко зашел !!!");
            MessageBox.Show("У тебя все получится ;) !!!");
        }

        private List<Patient> _allPatients = DataBaseCommand.GetAllPatients();

        public List<Patient> AllPatients
        {
            get
            {
                return _allPatients;
            }
            set
            {
                SetProperty(ref _allPatients, value);
            }
        }


        private List<WorkList> _allWorkLists = DataBaseCommand.GetAllWorkLists();

        public List<WorkList> AllWorkLists
        {
            get
            {
                return _allWorkLists;
            }
            set
            {
                SetProperty(ref _allWorkLists, value);
            }
        }



        private DelegateCommand refreshPatients;
        public ICommand RefreshPatients => refreshPatients ??= new DelegateCommand(PerformRefreshPatients);

        private void PerformRefreshPatients()
        {
            AllPatients = DataBaseCommand.GetAllPatients();
            MainWindow.AllPatientView.ItemsSource = null;
            MainWindow.AllPatientView.Items.Clear();
            MainWindow.AllPatientView.ItemsSource = AllPatients;
            MainWindow.AllPatientView.Items.Refresh();
            UpdateText = "Patient table is update";
        }



        private DelegateCommand refreshWorkList;
        public ICommand RefreshWorkList => refreshWorkList ??= new DelegateCommand(PerformRefreshWorkList);

        private void PerformRefreshWorkList()
        {
            AllWorkLists = DataBaseCommand.GetAllWorkLists();
            MainWindow.AllWorkListView.ItemsSource = null;
            MainWindow.AllWorkListView.Items.Clear();
            MainWindow.AllWorkListView.ItemsSource = AllWorkLists;
            MainWindow.AllWorkListView.Items.Refresh();
            UpdateText = "WorkList table is update";
        }
        


        private DelegateCommand _addPatient;
        public ICommand AddPatient => _addPatient ??= new DelegateCommand(PerformAddPatient);

        private void PerformAddPatient()
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
                    PatientCount = SetPatientCount
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

                var addPatient = DataBaseCommand.GeneratePatientDateBase(newPatient);

                PerformRefreshPatients();
                LolMessageForPatientCount(SetPatientCount);

                UpdateText = "Пациент успешно добавлен";
            }
            catch (Exception ex)
            {
                UpdateText = "Пациент не добавлен";
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

        private DelegateCommand _addWorkList;
        public ICommand AddWorkList => _addWorkList ??= new DelegateCommand(PerformAddWorkList);

        private void PerformAddWorkList()
        {
            try
            {
                var newWorkList = new WorkListGeneratorParameters(
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

                var addWorkList = DataBaseCommand.GenerateWorkListBase(newWorkList);

                PerformRefreshWorkList();

                UpdateText = "WorkList added";
            }

            catch (Exception e)
            {
                UpdateText = "WorkList not added";
            }
        }



        private DelegateCommand _deleteFirstPatient;
        public ICommand DeleteFirstPatient => _deleteFirstPatient ??= new DelegateCommand(PerformDeleteFirstPatient);

        private void PerformDeleteFirstPatient()
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

                var deletePatient = DataBaseCommand.DeleteFirstPatient(patient);

                PerformRefreshPatients();

                UpdateText = "First Patient is Delete";
            }
            catch (Exception ex)
            {
                UpdateText = "Patient is not Deleted";
            }
        }


        private DelegateCommand _deleteAllPatient;
        public ICommand DeleteAllPatient => _deleteAllPatient ??= new DelegateCommand(PerformDeleteAllPatient);

        private void PerformDeleteAllPatient()
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

                var deletePatient = DataBaseCommand.DeleteAllPatients(patient);

                PerformRefreshPatients();

                UpdateText = "Patient Table Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Patient Table is not Deleted";
            }
        }


        private DelegateCommand _deleteFirstWorkList;
        public ICommand DeleteFirstWorkList => _deleteFirstWorkList ??= new DelegateCommand(PerformDeleteFirstWorkList);

        private void PerformDeleteFirstWorkList()
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

                var deleteWorkList = DataBaseCommand.DeleteFirstWorkList(workList);

                PerformRefreshWorkList();

                UpdateText = "First in WorkList Delete";
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList is not Deleted";
            }
        }


        private DelegateCommand _deleteAllWorkList;
        public ICommand DeleteAllWorkList => _deleteAllWorkList ??= new DelegateCommand(PerformDeleteAllWorkList);

        private void PerformDeleteAllWorkList()
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

                var deletePatient = DataBaseCommand.DeleteAllWorkList(workList);

                PerformRefreshWorkList();

                UpdateText = "WorkList Table Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList is not Deleted";
            }
        }


        private DelegateCommand _deleteAllTables;
        public ICommand DeleteAllTables => _deleteAllTables ??= new DelegateCommand(PerformDeleteAllTables);

        private void PerformDeleteAllTables()
        {
            try
            {
                PerformDeleteAllPatient();
                PerformRefreshPatients();

                PerformDeleteAllWorkList();
                PerformRefreshWorkList();

                ShakeWindow(Application.Current.MainWindow);
                ShowDeleteAllTablesEasterEgg();

                UpdateText = "All Tables Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Tables is not Deleted";
            }
        }

        public void ShakeWindow(Window window, int times = 15, int amplitude = 15, int delay = 20)
        {
            var originalLeft = window.Left;
            var originalTop = window.Top;

            var random = new Random();

            for (int i = 0; i < times; i++)
            {
                window.Left = originalLeft + random.Next(-amplitude, amplitude);
                window.Top = originalTop + random.Next(-amplitude, amplitude);

                Thread.Sleep(delay);
            }

            window.Left = originalLeft;
            window.Top = originalTop;
        }

        private readonly List<string> _deleteAllTableQuotes = new()
        {
            "Ты удалил всё. Красиво. Жестоко. Необратимо.",
            "Это была база... была.",
            "Пациенты? Какие пациенты? Их больше нет.",
            "Никто не выжил. Даже логирование.",
            "Поздравляю. Теперь у тебя идеальная чистота и ни одного бага.",
            "Реинкарнация системы началась. Удачи, Neo."
        };

        public void ShowDeleteAllTablesEasterEgg()
        {
            var message = _deleteAllTableQuotes[_random.Next(_deleteAllTableQuotes.Count)];

            MessageBox.Show(
                message,
                "💥 База уничтожена",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }


        /// <summary>
        /// Dialog Window Commands
        /// </summary>

        private DelegateCommand _aboutProgram;
        public ICommand AboutProgram => _aboutProgram = new DelegateCommand(InformationMessage);

        private void InformationMessage()
        {
            _dialogMessage.DataContext = this;
            _dialogMessage.ShowDialog();
        }

        private DelegateCommand _dialog;
        public ICommand ClosingDialogWindow => _dialog = new DelegateCommand(ClosingDialog);

        private void ClosingDialog()
        {
            _mediaPlayer.Open(new Uri(PathToResourceAudio));
            _mediaPlayer.Play();
            _dialogMessage.Close();
        }

        private DelegateCommand _hotkey;
        public ICommand HotkeyForDialogWindow => _hotkey = new DelegateCommand(HotkeyForDialog);

        private void HotkeyForDialog()
        {
            MessageBox.Show("Отличная попытка ДРУЖИЩЕ", "ага )))", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private DelegateCommand _hotkeyExit;
        public ICommand HotkeyExitFromProgram => _hotkeyExit = new DelegateCommand(HotkeyExit);

        private void HotkeyExit()
        {
            Application.Current.Shutdown();
        }

        private DelegateCommand _specification;
        public ICommand OpenSpecificationWindow => _specification = new DelegateCommand(PerformSpecification);

        private void PerformSpecification()
        {
            _specificationWindow.DataContext = this;
            _specificationWindow.Show();
        }

        private DelegateCommand _closeSpecification;
        public ICommand CloseSpecificationWindow => _closeSpecification = new DelegateCommand(CloseSpecification);

        private void CloseSpecification()
        {
            _specificationWindow.Close();
        }

        private DelegateCommand _tools;
        public ICommand ToolMessage => _tools = new DelegateCommand(ToolMessageBox);

        private void ToolMessageBox()
        {
            MessageBox.Show("Ну я же просил !!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private DelegateCommand _cancelAddPatient;
        public ICommand CancelAddPatient => _cancelAddPatient ??= new DelegateCommand(PerformCancelAddPatient);

        private void PerformCancelAddPatient()
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

        private DelegateCommand _addOnePatient;
        public ICommand AddOnePatient => _addOnePatient ??= new DelegateCommand(PerformAddOnePatient);

        private void PerformAddOnePatient()
        {
            var messageToUpdateText = string.Empty;

            try
            {
                messageToUpdateText = PlayIntroAndShowMessage();

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

                var addPatient = DataBaseCommand.AddPatientInDateBase(newPatient);

                PerformRefreshPatients();
                CleareFields();

                if (_random.Next(0, 100) < 30)
                {
                    MessageBox.Show(
                        "Unhandled Exception: DeveloperWasLazyException",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

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

        private string PlayIntroAndShowMessage()
        {
            _mediaPlayer.Open(new Uri(PathToGodFatherAudio));
            _mediaPlayer.Play();

            var result = MessageBox.Show(
                "  Ты просишь меня добавить пациента, \n\rно делаешь это без должного уважения !\n\nТы по-прежнему хочешь продолжить?",
                "Don Father",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            _mediaPlayer.Stop();

            if (result == MessageBoxResult.No)
            {
                return "Уважение восстановлено. Операция отменена.";
            }

            return string.Empty;
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
