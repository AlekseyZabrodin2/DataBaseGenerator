using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Data;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Image;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LiteDbStudiesTableViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IStudyStorageModule _storage => App.SharedStorage;
        private readonly IServiceProvider _serviceProvider;
        private string _gender;
        private string _databasePath = string.Empty;
        private CancellationTokenSource _cancellationTokenSource;
        private PatientLiteDb _patientLiteDb;
        private StudyLiteDb _studyLiteDb;
        private SeriesLiteDb _seriesLiteDb;
        private ImageLiteDb _imageLiteDb;
        private readonly List<ImageLiteDb> _imagesBatch = new();
        private readonly List<SeriesLiteDb> _seriesBatch = new();
        private readonly int _studyBatchSize = 5000;
        private readonly int _batchSize = 10000;
        private List<PatientLiteDb> _cachedPatients;
        private readonly Random _random = new Random();

        [ObservableProperty]
        public partial ObservableCollection<StudyLiteDb> AllStudies { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<SeriesLiteDb> AllSeries { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<ImageLiteDb> AllImages { get; set; }

        [ObservableProperty]
        public partial StudyLiteDb SelectedStudy { get; set; }

        [ObservableProperty]
        public partial string UpdateText { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        [ObservableProperty]
        public partial bool PercentShow { get; set; }

        [ObservableProperty]
        public partial string AddIdPatient { get; set; }

        [ObservableProperty]
        public partial string AddFamily { get; set; }

        [ObservableProperty]
        public partial string AddName { get; set; }

        [ObservableProperty]
        public partial string AddMiddleName { get; set; }

        [ObservableProperty]
        public partial string AddFullName { get; set; }

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

        [ObservableProperty]
        public partial string AddAdress { get; set; }

        [ObservableProperty]
        public partial string AddWorkPlase { get; set; }

        [ObservableProperty]
        public partial string MedicalInsuranceNumber { get; set; }

        [ObservableProperty]
        public partial string AddInfo { get; set; }

        [ObservableProperty]
        public partial string BirthDateToolTip { get; set; }

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

        [ObservableProperty]
        public partial PatientLiteDb SelectedPatient { get; set; }

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
        public partial int SetStudiesCount { get; set; }

        [ObservableProperty]
        public partial int SetSeriesCount { get; set; } = 1;

        [ObservableProperty]
        public partial int SetImageCount { get; set; } = 1;

        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                SetProperty(ref _databasePath, value);
                BuildShortPath(_databasePath);
            }
        }

        [ObservableProperty]
        public partial string DatabasePathShort { get; set; }

        [ObservableProperty]
        public partial bool IsReadOnlyMode { get; set; } = true;



        public LiteDbStudiesTableViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            //_storage = App.SharedStorage;            

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            AllStudies = new();
            AllSeries = new();
            AllImages = new();

            DatabasePath = _storage.DatabasePath;

            await GetDataFromAllDbAsync();

            _ = Task.Run(() => GetCachedPatients());            
        }


        [RelayCommand]
        public async Task AddStudiesAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            _cancellationTokenSource = new CancellationTokenSource();
            var generationCancelled = false;

            try
            {
                StartBusy("Генерация исследований ...");

                var progress = new Progress<int>(percent =>
                {
                    CurrentProgress = percent;
                    BusyMessage = $"Генерация исследований ...";
                });

                var studyDto = CreateStudyDto();

                await GenerateStudiesByBulkAsync(studyDto, progress, _cancellationTokenSource.Token);

                await RefreshDataBaseAsync();

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    generationCancelled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                UpdateText = "Исследование не добавлено";
                _logger.Error(ex, "Error in study generation");
                MessageBox.Show($"{ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed;
                var timeString = FormatTimeSpan(elapsedTime);

                StopBusy();

                if (generationCancelled)
                {
                    UpdateText = $"Генерация исследований было прервано! Всего - [{AllStudies.Count}]. Время выполнения: {timeString}";
                }
                else
                {
                    UpdateText = $"Исследование успешно добавлено! Всего - [{AllStudies.Count}]-[{AllSeries.Count}]-[{AllImages.Count}]. Время выполнения: {timeString}";
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public async Task GenerateStudiesByBulkAsync(StudyGeneratorDto inputParameters, IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                var totalStudies = SetStudiesCount;
                var totalSeries = SetSeriesCount;
                var totalImages = SetImageCount;
                var studyCounter = 0;
                var studiesBatch = new List<StudyLiteDb>(_studyBatchSize);                

                await Task.Run(() =>
                {
                    for (var studyIndex = 0; studyIndex < totalStudies; studyIndex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.Info("Cancellation requested, stopping generation...");
                            break;
                        }

                        var study = GenerateLiteDbStudies(inputParameters, cancellationToken);
                        studiesBatch.Add(study);
                        studyCounter++;

                        if (studiesBatch.Count >= _studyBatchSize || studyIndex == totalStudies - 1)
                        {
                            _storage.Studies.InsertBulk(studiesBatch);
                            studiesBatch.Clear();

                            progress?.Report((int)((double)(studyIndex +1) / totalStudies * 100));
                        }
                    }
                }, cancellationToken);

                FlushRemainingBatches();

                _logger.Info($"Created {totalStudies} studies with {SetSeriesCount} series and {SetImageCount} images");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Studies not generated");
                throw;
            }
        }

        private StudyLiteDb GenerateLiteDbStudies(StudyGeneratorDto study, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.Now;
                _patientLiteDb = GetRandomPatientFromCache();

                if (_patientLiteDb == null)
                {
                    _logger.Warn("GenerateLiteDbStudies: No patient found, creating empty study");
                    _patientLiteDb = CreateDefaultPatient();
                }

                _studyLiteDb = new StudyLiteDb
                {
                    Id = ObjectId.NewObjectId(),
                    StudyInstanceUid = $"2.25.{GenerateRandomNumber(36)}",
                    PatientId = new ObjectId(_patientLiteDb.Id),

                    StudyId = $"STD-{GenerateRandomNumber(12)}t",

                    SnapshotLastName = _patientLiteDb.LastName,
                    SnapshotFirstName = _patientLiteDb.FirstName,
                    SnapshotPatronymic = _patientLiteDb.MiddleName,
                    SnapshotPatientId = _patientLiteDb.PatientID,
                    PatientBirthDate = _patientLiteDb.BirthDate,

                    AccessionNumber = study.RandomAccessionNumber.Generate(),
                    BodyParts = study.RandomBodyParts.Generate(),
                    StudyDateTime = study.RandomStudyDateTime.Generate(),
                    EffectiveDosemSv = study.RandomEffectiveDose.Generate(),
                    Status = study.RandomStatus.Generate()
                };

                GenerateLiteDbSeries(cancellationToken);

                return _studyLiteDb;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Studies not generated");
                throw;
            }           
        }

        private void GenerateLiteDbSeries(CancellationToken cancellationToken)
        {
            try
            {
                var total = SetSeriesCount;
                var random = new Random();
                var seriesCounter = 0;

                for (int seriesIndex = 0; seriesIndex < total; seriesIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.Info("Cancellation requested, stopping generation...");
                        break;
                    }

                    _seriesLiteDb = new SeriesLiteDb
                    {
                        Id = ObjectId.NewObjectId(),

                        SeriesInstanceUid = $"2.25.{GenerateRandomNumber(36)}",

                        StudyInstanceUid = _studyLiteDb.StudyInstanceUid,

                        OperatorName = "OPERATOR",
                        Modality = "DX",
                        BodyPartExamined = _studyLiteDb.BodyParts[random.Next(0, _studyLiteDb.BodyParts.Length)],

                        SeriesDateTime = DateTime.Now
                    };

                    _seriesBatch.Add(_seriesLiteDb);
                    seriesCounter++;

                    if (_seriesBatch.Count >= _batchSize)
                    {
                        _storage.Series.InsertBulk(_seriesBatch);
                        _seriesBatch.Clear();
                    }

                    GenerateLiteDbImages(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Series not generated");
                throw;
            }
        }

        private void GenerateLiteDbImages(CancellationToken cancellationToken)
        {
            try
            {
                var total = SetImageCount;
                var imageCounter = 0;

                for (int imageIndex = 0; imageIndex < total; imageIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.Info("Cancellation requested, stopping generation...");
                        break;
                    }

                    var image = CreateImageDto();

                    _imageLiteDb = new ImageLiteDb
                    {
                        Id = ObjectId.NewObjectId(),

                        SopInstanceUid = $"2.25.{GenerateRandomNumber(36)}",

                        SeriesInstanceUid = _seriesLiteDb.SeriesInstanceUid,
                        StudyInstanceUid = _seriesLiteDb.StudyInstanceUid,

                        BodyPart = _seriesLiteDb.BodyPartExamined,
                        Projection = image.RandomProjection.Generate(),
                        Laterality = image.RandomLaterality.Generate(),
                        InstanceDose = 0.21,
                        AcquisitionTime = DateTime.Now
                    };

                    _imagesBatch.Add(_imageLiteDb);
                    imageCounter++;

                    if (_imagesBatch.Count >= _batchSize)
                    {
                        _storage.Images.InsertBulk(_imagesBatch);
                        _imagesBatch.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Images not generated");
                throw;
            }
        }

        private void FlushRemainingBatches()
        {
            if (_imagesBatch.Any())
            {
                _storage.Images.InsertBulk(_imagesBatch);
                _imagesBatch.Clear();
            }

            if (_seriesBatch.Any())
            {
                _storage.Series.InsertBulk(_seriesBatch);
                _seriesBatch.Clear();
            }
        }

        private PatientLiteDb GetRandomPatientFromCache()
        {
            var patients = GetCachedPatients();

            if (patients == null || !patients.Any())
                return null;

            var index = _random.Next(patients.Count);
            return patients[index];
        }

        private List<PatientLiteDb> GetCachedPatients()
        {
            if (_cachedPatients == null || !_cachedPatients.Any())
            {
                _cachedPatients = _storage.Patients.GetAllPatients().ToList();
                _logger.Info($"Cached {_cachedPatients.Count} patients");
            }

            return _cachedPatients;
        }

        private StudyGeneratorDto CreateStudyDto()
        {
            return new StudyGeneratorDto(
                new RandomAccessionNumberRule(),
                new RandomBodyPartsRule(),
                new RandomStudyDateTimeRule(),
                new RandomEffectiveDoseRule(),
                new RandomStatusRule());
        }

        private ImageGeneratorDto CreateImageDto()
        {
            return new ImageGeneratorDto(
                new RandomProjectionRule(),
                new RandomLateralityRule());
        }

        private PatientLiteDb CreateDefaultPatient()
        {
            var patient = new PatientLiteDb
            {
                Id = ObjectId.NewObjectId().ToString(),
                PatientID = "MXE0000001",
                LastName = "Неизвестный",
                FirstName = "Пациент",
                MiddleName = "Пустой",
                BirthDate = DateTime.Now.AddYears(-30),
                Sex = PatientSex.Other,
                Address = "Не указан",
                Comments = "Автоматически создан для генерации исследований",
                Phone = "+344543215",
                Age = "30"
            };

            _storage.Patients.Upsert(patient);
            _logger.Info($"Created and saved default patient: {patient.PatientID}");

            return patient;
        }

        private string GenerateRandomNumber(int length)
        {
            var chars = "0123456789";
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        [RelayCommand]
        public async Task RefreshDataBaseAsync()
        {
            try
            {
                StartBusy("Обновление списка исследований ...");

                await Task.Run(() =>
                {
                    _storage?.Dispose();
                    App.UpdateSharedStorage(DatabasePath, IsReadOnlyMode);
                });

                await GetDataFromAllDbAsync();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка обновления БД");
                UpdateText = $"Ошибка обновления: {ex.Message}";
            }
            finally
            {
                StopBusy();
            }
        }

        public async Task GetDataFromAllDbAsync()
        {
            StartBusy("Получение данных ...", false);

            await GetAllStudiesAsync();
            await GetAllSeriesAsync();
            await GetAllImagesAsync();

            UpdateStatistics();

            StopBusy();
        }

        public async Task GetAllStudiesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    AllStudies = _storage.Studies.GetAllStudies();
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке исследований");
            }
        }

        public async Task GetAllSeriesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    AllSeries = _storage.Series.GetAllSeries();
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке серий");
            }
        }

        public async Task GetAllImagesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    AllImages = _storage.Images.GetAllImage();
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка при загрузке изображений");
            }
        }

        private void UpdateStatistics()
        {
            UpdateText = $"Всего - [{AllStudies.Count}] исследований, [{AllSeries.Count}] серий, [{AllImages.Count}] изображений.";
        }

        [RelayCommand]
        public void BrowseDatabase()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "LiteDB files (*.db)|*.db|All files (*.*)|*.*",
                Title = "Выберите файл базы данных"
            };

            if (dialog.ShowDialog() == true)
            {
                DatabasePath = dialog.FileName;
                _ = RefreshDataBaseAsync();
            }
        }

        [RelayCommand]
        public async Task ChangeDatadaseMode(object? readOnlyFromToggle)
        {
            // Состояние с ToggleButton (после клика); object — чтобы не зависеть от того, как WPF боксит bool/bool?.
            var readOnly = readOnlyFromToggle switch
            {
                bool b => b,
                _ => IsReadOnlyMode
            };

            if (string.IsNullOrWhiteSpace(DatabasePath))
            {
                MessageBox.Show("Не задан путь к файлу базы.", "LiteDB", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var previous = _storage;
            try
            {
                previous?.Dispose();
                App.UpdateSharedStorage(DatabasePath, readOnly);

                await GetDataFromAllDbAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Не удалось переключить режим LiteDB");
                MessageBox.Show(ex.Message, "Ошибка LiteDB", MessageBoxButton.OK, MessageBoxImage.Error);
                try
                {
                    App.UpdateSharedStorage(DatabasePath, false);
                    AllStudies = _storage.Studies.GetAllStudies();
                }
                catch (Exception ex2)
                {
                    _logger.Error(ex2, "Не удалось восстановить подключение к LiteDB");
                    MessageBox.Show(
                        "Не удалось восстановить подключение: " + ex2.Message,
                        "Ошибка LiteDB",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }


        public void LoadData()
        {
            try
            {
                var studies = _storage.Studies.GetAllStudies();
                AllStudies = new ObservableCollection<StudyLiteDb>(studies);
                UpdateText = $"Исследований: {AllStudies.Count}";
                _logger.Info($"Loaded {AllStudies.Count} studies");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading studies");
                UpdateText = $"Ошибка загрузки: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task DeleteAll()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                StartBusy("Удаление исследований...", false);

                await Task.Run(() =>
                {
                    _storage.Images.DeleteAll();
                    _storage.Series.DeleteAll();
                    _storage.Studies.DeleteAll();
                });

                await RefreshDataBaseAsync();
            }
            catch (Exception ex)
            {
                UpdateText = $"Ошибка удаления: {ex.Message}";
                _logger.Error(ex, "Error deleting all studies");
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                stopwatch.Stop();
                var timeString = FormatTimeSpan(stopwatch.Elapsed);
                UpdateText += $" Время выполнения: {timeString}";
                StopBusy();
            }
        }

        [RelayCommand]
        public void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            BusyMessage = "Отмена операции...";

            _imagesBatch.Clear();
            _seriesBatch.Clear();
        }

        private void StartBusy(string message = "Загрузка...", bool percentShow = true)
        {
            BusyMessage = message;
            CurrentProgress = 0;
            IsBusy = true;
            PercentShow = percentShow;
        }

        private void StopBusy()
        {
            IsBusy = false;
            BusyMessage = string.Empty;
            PercentShow = false;
            CurrentProgress = 0;
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

        private string BuildShortPath(string fullPath, int keepFolders = 3)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            // Нормализуем разделители и убираем хвостовые '\'
            var normalized = fullPath.Replace('/', '\\').TrimEnd('\\');

            // Для коротких путей ничего не делаем
            var parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= keepFolders + 1) // +1 = имя файла
            {
                DatabasePathShort = normalized;
                return normalized;
            }

            var fileName = parts[^1];
            var startIndex = Math.Max(0, parts.Length - (keepFolders + 1));
            var tail = string.Join("\\", parts[startIndex..]);

            // Если есть диск (C:) или UNC, префикс всё равно делаем через "..."
            DatabasePathShort = $@"...\{tail}";

            return DatabasePathShort;
        }


        [RelayCommand]
        public void ConnectDB()
        {
            var connectingState = true; //await _patientService.ConnectingEchoAsync();
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
        public void DeleteAllTables()
        {
            try
            {
                //await DeleteAllPatientAsync();
                _ = RefreshDataBaseAsync();

                UpdateText = "All Tables Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Tables is not Deleted";
            }
        }

        [RelayCommand]
        public void CancelGeneration()
        {
            _cancellationTokenSource?.Cancel();
            BusyMessage = "Отмена генерации...";
        }
    }
}
