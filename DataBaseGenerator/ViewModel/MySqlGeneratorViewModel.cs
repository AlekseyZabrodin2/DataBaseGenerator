using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.Data;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList;
using DataBaseGenerator.UI.Wpf.View;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class MySqlGeneratorViewModel : ObservableObject
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
        private List<WorkList> _workLists = [];
        private List<Patient> _patients = [];
        private string _workListPatientIdFilter = string.Empty;
        private string _patientIdFilter = string.Empty;
        private string _updateText;
        private int _setPatientCount;
        private int _setWorkListCount;
        private string _aeTitle;
        private string _gender = string.Empty;
        private string _addIdPatient = string.Empty;
        private string _addFamily = string.Empty;
        private string _addName = string.Empty;
        private string _addMiddleName = string.Empty;
        private string _addFullName = string.Empty;
        private string _addAdress = string.Empty;
        private string _addTelephone = string.Empty;
        private string _addWorkPlase = string.Empty;
        private string _addInfo = string.Empty;
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

        public string AddFullName
        {
            get => _addFullName;
            set
            {
                SetProperty(ref _addFullName, value);
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
                    value = "F";
                }
                else if (value == Gender[2])
                {
                    value = "O";
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

        public string AddTelephone
        {
            get => _addTelephone;
            set
            {
                SetProperty(ref _addTelephone, value);
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

        public string WorkListPatientIdFilter
        {
            get => _workListPatientIdFilter;
            set
            {
                if (SetProperty(ref _workListPatientIdFilter, value))
                    ApplyWorkListFilter();
            }
        }

        public string PatientIdFilter
        {
            get => _patientIdFilter;
            set
            {
                if (SetProperty(ref _patientIdFilter, value))
                    ApplyPatientFilter();
            }
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

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        private CancellationTokenSource _cancellationTokenSource;

        [ObservableProperty]
        public partial bool OptimisationIsEnabled { get; set; } = false;

        [ObservableProperty]
        public partial bool OptimisIsVisibility { get; set; } = false;

        public IReadOnlyList<int> PatientIds { get; private set; } = [];

        [ObservableProperty]
        public partial int NextPatientId { get; set; } = 1;



        public MySqlGeneratorViewModel(BaseGenerateContext context, IHttpClientFactory clientFactory)
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
            UseRandomBirthdate = true;

            var refreshPat = await RefreshPatientsAsync();
            var patientsText = UpdateText;

            var refreshWl = await RefreshWorkListAsync();

            if (refreshPat && refreshWl)
                UpdateText = $"Пациентов - [{AllPatients.Count}]. Рабочий список - [{AllWorkLists.Count}]";
            else if (!refreshPat && refreshWl)
                UpdateText = patientsText;
            else if (!refreshPat && !refreshWl)
                UpdateText = $"{patientsText} {UpdateText}";
        }


        [RelayCommand]
        public async Task AddPatientAsync()
        {
            _logger.Info(">>> AddPatientAsync: START");

            var stopwatch = Stopwatch.StartNew();

            _cancellationTokenSource = new CancellationTokenSource();
            var generationCancelled = false;
            var generationSkipped = false;
            var generationFailed = false;

            try
            {
                StartBusy("Генерация пациентов...");

                var newPatient = new PatientGeneratorParameters(
                    new OrderIdPatientRule(),
                    new RandomLastNameRule(),
                    new RandomFirstNameRule(),
                    new RandomMiddleNameRule(),
                    new OrderPatientIdRule(),
                    new RandomBirthDateRule(new DateTime()),
                    new RandomSexRule(),
                    new RandomAddressRule(),
                    new RandomPhoneRule(),
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
                    generationSkipped = true;

                    return;
                }

                if (SetPatientCount == 13)
                {
                    RandomMessageFor13Patient();
                    SetPatientCount = 0;
                    generationSkipped = true;

                    return;
                }

                _logger.Info("AddPatientAsync: Calling PatientService.GenerateAsync...");                

                await _patientService.GenerateAsync(newPatient, _cancellationTokenSource.Token);

                _logger.Info("AddPatientAsync: GenerateAsync completed successfully");

                generationFailed = !await RefreshPatientsAsync();
                //LolMessageForPatientCount(SetPatientCount);

                if (!generationFailed)
                    _logger.Info("AddPatientAsync: SUCCESS, UpdateText = {UpdateText}", UpdateText);
            }
            catch (OperationCanceledException)
            {
                _logger.Info("AddPatientAsync: Generation was cancelled by user");
                await RefreshPatientsAsync();
                generationCancelled = true;
            }
            catch (DbUpdateException dbEx)
            {
                generationFailed = true;
                _logger.Error(dbEx, "AddPatientAsync: DB ERROR - {Message}", dbEx.InnerException?.Message ?? dbEx.Message);
                UpdateText = $"Ошибка БД: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (HttpRequestException httpEx)
            {
                generationFailed = true;
                _logger.Error(httpEx, "AddPatientAsync: HTTP ERROR - {Message}", httpEx.Message);
                UpdateText = $"Ошибка API: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                generationFailed = true;
                _logger.Error(ex, "AddPatientAsync: UNEXPECTED ERROR - {Message}", ex.Message);
                UpdateText = $"Пациент не добавлен: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                var timeString = FormatTimeSpan(stopwatch.Elapsed);

                _logger.Info("<<< AddPatientAsync: END");
                StopBusy();

                if (generationCancelled)
                {
                    UpdateText = $"Генерация пациентов была прервана! Всего - [{AllPatients?.Count ?? 0}], Время выполнения: {timeString}";
                }
                else if (!generationFailed && !generationSkipped)
                {
                    UpdateText = $"Пациент успешно добавлен! Всего - [{AllPatients?.Count ?? 0}]. Время выполнения: {timeString}";
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        public async Task SavePatientsAsync()
        {
            try
            {
                var updatePatients = new ObservableCollection<Patient>();
                foreach (var patient in _patients)
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
        public async Task<bool> RefreshPatientsAsync()
        {
            _logger.Info(">>> RefreshPatientsAsync: START");

            var patientsRefreshed = false;

            try
            {
                _logger.Info("RefreshPatientsAsync: Calling PatientService.GetAllAsync...");

                var patients = await _patientService.GetAllAsync() ?? [];
                _patients = patients.ToList();

                PatientIds = _patients.Select(patient => patient.ID_Patient).ToList();
                NextPatientId = _patients.Count == 0
                    ? 1
                    : _patients.Max(patient => patient.ID_Patient) + 1;

                ApplyPatientFilter();

                UpdateText = $"Patient table is update! Всего пациентов - [{_patients.Count}]";

                _logger.Info($"RefreshPatientsAsync: SUCCESS, Retrieved {_patients.Count} patients");

                patientsRefreshed = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RefreshPatientsAsync: ERROR - {Message}", ex.Message);
                UpdateText = $"Ошибка обновления: {ex.Message}";
            }
            finally
            {
                _logger.Info("<<< RefreshPatientsAsync: END");
            }
            return patientsRefreshed;
        }

        private void ApplyPatientFilter()
        {
            IEnumerable<Patient> patients = _patients.OrderBy(patient => patient.ID_Patient);
            var filter = PatientIdFilter?.Trim();

            if (!string.IsNullOrEmpty(filter))
            {
                var hasPatientId = int.TryParse(filter, out var patientId);
                patients = patients.Where(patient =>
                    (patient.PatientID?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (hasPatientId && patient.ID_Patient == patientId));
            }

            AllPatients = new ObservableCollection<Patient>(patients);
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
            _logger.Info(">>> DeleteFirstPatientAsync: START");

            try
            {
                _logger.Info("DeleteFirstPatientAsync: Calling PatientService.DeleteFirstAsync...");

                await _patientService.DeleteFirstAsync();

                _logger.Info("DeleteFirstPatientAsync: DeleteFirstAsync completed");

                if (!await RefreshPatientsAsync())
                    return;

                UpdateText = "First Patient is Delete";
                _logger.Info("DeleteFirstPatientAsync: SUCCESS");
            }
            catch (Exception ex)
            {
                UpdateText = "Patient is not Deleted";
                _logger.Error(ex, "DeleteFirstPatientAsync: ERROR - {Message}", ex.Message);
                MessageBox.Show($"{ex.Message}");
            }
            finally
            {
                _logger.Info("<<< DeleteFirstPatientAsync: END");
            }
        }

        [RelayCommand]
        public async Task<bool> DeleteAllPatientAsync()
        {
            _logger.Info(">>> DeleteAllPatientAsync: START");

            StartBusy("Удаление пациентов...");

            try
            {
                _logger.Info("DeleteAllPatientAsync: Calling PatientService.DeleteAllAsync...");

                await _patientService.DeleteAllAsync();

                _logger.Info("DeleteAllPatientAsync: DeleteAllAsync completed");

                if (!await RefreshPatientsAsync())
                    return false;

                UpdateText = "Patient Table Deletion completed";
                _logger.Info("DeleteAllPatientAsync: SUCCESS");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.Error(dbEx, "DeleteAllPatientAsync: DB ERROR - {Message}", dbEx.InnerException?.Message ?? dbEx.Message);
                UpdateText = $"Ошибка БД при удалении: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                UpdateText = "Patient Table is not Deleted";
                _logger.Error(ex, "DeleteAllPatientAsync: ERROR - {Message}", ex.Message);
                MessageBox.Show($"{ex.Message}");
                return false;
            }
            finally
            {
                _logger.Info("<<< DeleteAllPatientAsync: END");
                StopBusy();
            }
        }

        [RelayCommand]
        public async Task AddWorkListAsync()
        {
            _logger.Info(">>> AddWorkListAsync: START");

            var stopwatch = Stopwatch.StartNew();
            _cancellationTokenSource = new CancellationTokenSource();
            var generationCancelled = false;
            var generationSkipped = false;
            var generationFailed = false;

            try
            {
                StartBusy("Генерация рабочего списка ...");

                if (PatientIds.Count == 0)
                {
                    generationSkipped = true;
                    UpdateText = "Генерация невозможна: список пациентов пуст.";
                    _logger.Warn("AddWorkListAsync: PatientIds is empty, generation skipped");
                    return;
                }

                var newWorkList = new WorkListGeneratorDto(
                    new OrderIdWorklistRule(),
                    new RandomCreateDateRule(),
                    new RandomCreateTimeRule(),
                    new RandomCompleteDateRule(),
                    new RandomCompleteTimeRule(),
                    new OrderIdPatientWlRule(PatientIds),
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

                _logger.Info("AddWorkListAsync: Calling WorklistService.GenerateAsync...");

                await _worklistService.GenerateAsync(newWorkList, _cancellationTokenSource.Token);

                _logger.Info("AddWorkListAsync: GenerateAsync completed");

                generationFailed = !await RefreshWorkListAsync();

                if (!generationFailed)
                    _logger.Info("AddWorkListAsync: SUCCESS");
            }
            catch (OperationCanceledException)
            {
                _logger.Info("AddWorkListAsync: Generation was cancelled by user");
                generationCancelled = true;
                await RefreshWorkListAsync();
            }
            catch (Exception ex)
            {
                generationFailed = true;
                UpdateText = "WorkList not added";
                _logger.Error(ex, "AddWorkListAsync: ERROR - {Message}", ex.Message);
                MessageBox.Show($"{ex.Message}");
            }
            finally
            {
                _logger.Info("<<< AddWorkListAsync: END");

                stopwatch.Stop();
                var timeString = FormatTimeSpan(stopwatch.Elapsed);

                StopBusy();

                if (generationCancelled)
                {
                    UpdateText = $"Генерация списка была прервана! Всего - [{AllWorkLists?.Count ?? 0}], Время выполнения: {timeString}";
                }
                else if (!generationFailed && !generationSkipped)
                {
                    UpdateText = $"Рабочий список успешно добавлен! Всего - [{AllWorkLists?.Count ?? 0}]. Время выполнения: {timeString}";
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        public async Task<bool> RefreshWorkListAsync()
        {
            _logger.Info(">>> RefreshWorkListAsync: START");

            var workListRefreshed = false;

            try
            {
                _logger.Info("RefreshWorkListAsync: Calling WorklistService.GetAllAsync...");

                _workLists = await _worklistService.GetAllAsync() ?? [];
                ApplyWorkListFilter();

                UpdateText = "WorkList table is update";

                _logger.Info($"RefreshWorkListAsync: SUCCESS, Retrieved {_workLists.Count} worklists");

                workListRefreshed = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RefreshWorkListAsync: ERROR - {Message}", ex.Message);
                UpdateText = $"Ошибка обновления WorkList: {ex.Message}";                
            }
            finally
            {
                _logger.Info("<<< RefreshWorkListAsync: END");
            }

            return workListRefreshed;
        }

        private void ApplyWorkListFilter()
        {
            IEnumerable<WorkList> workLists = _workLists.OrderBy(workList => workList.ID_Patient);

            if (int.TryParse(WorkListPatientIdFilter, out var patientId))
                workLists = workLists.Where(workList => workList.ID_Patient == patientId);

            AllWorkLists = workLists.ToList();
        }

        [RelayCommand]
        public async Task DeleteFirstWorkListAsync()
        {
            _logger.Info(">>> DeleteFirstWorkListAsync: START");

            try
            {
                _logger.Info("DeleteFirstWorkListAsync: Calling WorklistService.DeleteFirstAsync...");

                await _worklistService.DeleteFirstAsync();

                _logger.Info("DeleteFirstWorkListAsync: DeleteFirstAsync completed");

                if (!await RefreshWorkListAsync())
                    return;

                UpdateText = "First in WorkList Delete";
                _logger.Info("DeleteFirstWorkListAsync: SUCCESS");
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList is not Deleted";
                _logger.Error(ex, "DeleteFirstWorkListAsync: ERROR - {Message}", ex.Message);
                MessageBox.Show($"{ex.Message}");
            }
            finally
            {
                _logger.Info("<<< DeleteFirstWorkListAsync: END");
            }
        }

        [RelayCommand]
        public async Task<bool> DeleteAllWorkListAsync()
        {
            _logger.Info(">>> DeleteAllWorkListAsync: START");

            StartBusy("Удаление рабочего списка...");

            try
            {
                _logger.Info("DeleteAllWorkListAsync: Calling WorklistService.DeleteAllAsync...");

                await _worklistService.DeleteAllAsync();

                _logger.Info("DeleteAllWorkListAsync: DeleteAllAsync completed");

                if (!await RefreshWorkListAsync())
                    return false;

                UpdateText = "WorkList Table Deletion completed";
                _logger.Info("DeleteAllWorkListAsync: SUCCESS");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.Error(dbEx, "DeleteAllWorkListAsync: DB ERROR - {Message}", dbEx.InnerException?.Message ?? dbEx.Message);
                UpdateText = $"Ошибка БД при удалении: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return false;
            }
            catch (Exception ex)
            {
                UpdateText = "WorkList Table is not Deleted";
                _logger.Error(ex, "DeleteAllWorkListAsync: ERROR - {Message}", ex.Message);
                MessageBox.Show($"{ex.Message}");
                return false;
            }
            finally
            {
                _logger.Info("<<< DeleteAllWorkListAsync: END");
                StopBusy();
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
            _logger.Info(">>> DeleteAllTablesAsync: START");

            try
            {
                _logger.Info("DeleteAllTablesAsync: Deleting patients...");
                var patientsDeleted = await DeleteAllPatientAsync();
                var patientsText = UpdateText;

                _logger.Info("DeleteAllTablesAsync: Deleting worklists...");
                var worklistsDeleted = await DeleteAllWorkListAsync();

                if (patientsDeleted && worklistsDeleted)
                {
                    UpdateText = "All Tables Deletion completed";
                    _logger.Info("DeleteAllTablesAsync: SUCCESS - All tables cleared");
                }
                else if (!patientsDeleted && worklistsDeleted)
                {
                    UpdateText = patientsText;
                }
                else if (!patientsDeleted && !worklistsDeleted)
                {
                    UpdateText = $"{patientsText} {UpdateText}";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteAllTablesAsync: ERROR - {Message}", ex.Message);
                UpdateText = "Tables is not Deleted";
            }
            finally
            {
                _logger.Info("<<< DeleteAllTablesAsync: END");
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

        [RelayCommand]
        public void CancelAddPatient()
        {
            AddIdPatient = string.Empty;

            AddFamily = string.Empty;

            AddName = string.Empty;

            AddMiddleName = string.Empty;

            AddAdress = string.Empty;

            AddTelephone = string.Empty;

            AddWorkPlase = string.Empty;

            AddInfo = string.Empty;

            SelecedGender = null;

            MedicalInsuranceNumber = string.Empty;

            UpdateText = "Как скажете, отменяю !";
        }        

        [RelayCommand]
        public async Task AddOnePatientAsync()
        {
            _logger.Info(">>> AddOnePatientAsync: START");

            try
            {
                StartBusy("Генерация пациента...");

                var assignedPatientId = NextPatientId;

                _logger.Info($"AddOnePatientAsync: Family={AddFamily}, " +
                    $"Name={AddName}, MiddleName={AddMiddleName}, Id={AddIdPatient}, " +
                    $"BirthDate={PatientBirthDate:yyyy-MM-dd}, Gender={SelecedGender}," +
                    $"Adress ={AddAdress}, Info={AddInfo}, WorkPlase={AddWorkPlase}");

                AddFullName = $"{AddFamily} {AddName} {AddMiddleName}";

                var newPatient = new PatientInputParameters(
                    assignedPatientId,
                    AddFamily,
                    AddName,
                    AddMiddleName,
                    AddFullName,
                    AddIdPatient,
                    PatientBirthDate,
                    SelecedGender ?? string.Empty,
                    AddAdress,
                    AddTelephone,
                    AddInfo,
                    AddWorkPlase)
                {
                    PatientCount = SetPatientCount
                };
                _logger.Info("AddOnePatientAsync: Calling PatientService.AddOneAsync...");

                await _patientService.AddOneAsync(newPatient);

                _logger.Info("AddOnePatientAsync: AddOneAsync completed successfully");

                if (!await RefreshPatientsAsync())
                    return;

                CleareFields();

                UpdateText = "Пациент добавлен";

                _logger.Info("AddOnePatientAsync: SUCCESS, UpdateText = {UpdateText}", UpdateText);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.Error(httpEx, "AddOnePatientAsync: HTTP ERROR - {Message}", httpEx.Message);
                UpdateText = httpEx.Message;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "AddOnePatientAsync: UNEXPECTED ERROR - {Message}", ex.Message);
                UpdateText = $"Пациент не добавлен {ex.Message}";
            }
            finally
            {
                _logger.Info("<<< AddOnePatientAsync: END");
                StopBusy();
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

        [RelayCommand]
        public async Task OptimisationDataBase()
        { }

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
            AddTelephone = string.Empty;
            AddWorkPlase = string.Empty;
            AddInfo = string.Empty;
            MedicalInsuranceNumber = string.Empty;
        }

        [RelayCommand]
        public void CancelGeneration()
        {
            _cancellationTokenSource?.Cancel();
            BusyMessage = "Отмена генерации...";
        }

        private string FormatTimeSpan(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return $"{time.Hours} ч {time.Minutes} мин {time.Seconds} сек";
            }                
            if (time.TotalMinutes >= 1)
            {
                return $"{time.Minutes} мин {time.Seconds} сек";
            }               
            return $"{time.TotalSeconds:F1} сек";
        }

        private void StartBusy(string message = "Загрузка...")
        {
            BusyMessage = message;
            IsBusy = true;
        }

        private void StopBusy()
        {
            IsBusy = false;
            BusyMessage = string.Empty;

        }
    }
}
