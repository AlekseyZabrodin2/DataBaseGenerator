using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.PlannedStudy;
using LiteDB;
using Microsoft.Win32;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LiteDbPlannedStudiesViewModel : ObservableObject
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private string _databasePath = string.Empty;
        private string _databasePatientPath = string.Empty;
        private CancellationTokenSource _cancellationTokenSource;
        private IStudyStorageModule _storage => App.SharedStorage;
        private IPlannedStudyStorageModule _plannedStudyStorage => App.PlannedStorage;
        private PlannedStudyLiteDb _plannedStudyLiteDb;
        private readonly object _dbLock = new object();
        private PatientLiteDb _patientLiteDb;
        private StudyLiteDb _studyLiteDb;
        private readonly int _studyBatchSize = 5000;
        private List<PatientLiteDb> _cachedPatients;
        private List<StudyLiteDb> _cachedStudies;
        private readonly Random _random = new Random();


        [ObservableProperty]
        public partial ObservableCollection<LitePlannedStudy> PlannedStudies { get; set; }

        [ObservableProperty]
        public partial PlannedStudyLiteDb SelectedPlannedStudy { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial string UpdateText { get; set; }

        [ObservableProperty]
        public partial string DatabasePathShort { get; set; }

        [ObservableProperty]
        public partial string DatabasePatientPathShort { get; set; }

        [ObservableProperty]
        public partial bool IsReadOnlyMode { get; set; } = true;

        [ObservableProperty]
        public partial bool PercentShow { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                SetProperty(ref _databasePath, value);
                BuildShortPath(_databasePath);
            }
        }

        public string DatabasePatientPath
        {
            get => _databasePatientPath;
            set
            {
                SetProperty(ref _databasePatientPath, value);
                BuildShortPatientPath(_databasePatientPath);
            }
        }

        [ObservableProperty]
        public partial int SetStudiesCount { get; set; }

        [ObservableProperty]
        public partial bool OptimisationIsEnabled { get; set; } = true;

        [ObservableProperty]
        public partial bool CancelButtonIsVisibil { get; set; } = true;



        public LiteDbPlannedStudiesViewModel()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            PlannedStudies = new();

            DatabasePatientPath = _storage.DatabasePath;

            await GetDataFromDbAsync();

            _logger.Info($"Create storage {DatabasePatientPath}");
        }



        [RelayCommand]
        public async Task LoadPlannedStudiesAsync()
        {
            try
            {
                StartBusy("Загрузка запланированных исследований...");

                await Task.Run(() =>
                {
                    // Загрузка данных из БД или другого источника
                    var studies = GetPlannedStudiesFromDatabase();
                    PlannedStudies = new ObservableCollection<LitePlannedStudy>(studies);
                });

                UpdateText = $"Загружено: {PlannedStudies.Count} исследований";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading planned studies");
                UpdateText = $"Ошибка: {ex.Message}";
            }
            finally
            {
                StopBusy();
            }
        }

        private List<LitePlannedStudy> GetPlannedStudiesFromDatabase()
        {
            // TODO: Реализовать загрузку из БД
            return new List<LitePlannedStudy>();
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

                var studyDto = CreatePlannedStudyDto();

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
                    UpdateText = $"Генерация исследований было прервано! Всего - [{PlannedStudies.Count}]. Время выполнения: {timeString}";
                }
                else
                {
                    UpdateText = $"Исследование успешно добавлено! Всего - [{PlannedStudies.Count}]. Время выполнения: {timeString}";
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public async Task GenerateStudiesByBulkAsync(PlannedStudyLiteDbDto plannedStudyDto, IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                var totalStudies = SetStudiesCount;
                var studyCounter = 0;
                var studiesBatch = new List<PlannedStudyLiteDb>(_studyBatchSize);

                await Task.Run(() =>
                {
                    for (var studyIndex = 0; studyIndex < totalStudies; studyIndex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.Info("Cancellation requested, stopping generation...");
                            break;
                        }

                        var study = GenerateLiteDbStudies(plannedStudyDto, cancellationToken);
                        studiesBatch.Add(study);
                        studyCounter++;

                        if (studiesBatch.Count >= _studyBatchSize || studyIndex == totalStudies - 1)
                        {
                            _plannedStudyStorage.PlannedStudies.InsertBulk(studiesBatch);
                            studiesBatch.Clear();

                            progress?.Report((int)((double)(studyIndex + 1) / totalStudies * 100));
                        }
                    }
                }, cancellationToken);

                _logger.Info($"Created {totalStudies} studies");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Studies not generated");
                throw;
            }
        }

        private PlannedStudyLiteDb GenerateLiteDbStudies(PlannedStudyLiteDbDto plannedStudyDto, CancellationToken cancellationToken)
        {
            try
            {
                GetCachedPatientsAndStudies();

                var now = DateTime.Now;
                _patientLiteDb = GetRandomPatientFromCache();

                if (_patientLiteDb == null)
                {
                    _logger.Warn("GenerateLiteDbStudies: No patient found, creating empty patient");
                    _patientLiteDb = CreateDefaultPatient();
                }

                _studyLiteDb = GetRandomStudiesFromPatient(_patientLiteDb);

                if (_studyLiteDb == null)
                {
                    _logger.Warn("GenerateLiteDbStudies: No patient found, creating empty study");
                    _studyLiteDb = CreateDefaultStudy();
                }

                _plannedStudyLiteDb = new PlannedStudyLiteDb
                {
                    Id = $"2.25.{GenerateRandomNumber(36)}",

                    PatientLastName = _patientLiteDb.LastName,
                    PatientFirstName = _patientLiteDb.FirstName,
                    PatientMiddleName = _patientLiteDb.MiddleName,
                    PatientId = _patientLiteDb.PatientID,
                    BirthDate = _patientLiteDb.BirthDate,
                    Gender = _patientLiteDb.Sex,
                    Telephone = _patientLiteDb.Phone,
                    Address = _patientLiteDb.Address,
                    Age = _patientLiteDb.Age,
                    Comments = _patientLiteDb.Comments,
                    PatientIdDataBase = _patientLiteDb.Id,

                    StudyID = _studyLiteDb.StudyId ,
                    AccessionNumber = _studyLiteDb.AccessionNumber,
                    StudyInstanceUid = _studyLiteDb.StudyInstanceUid,
                    Status = Enum.TryParse<PlannedStudyStatus>(_studyLiteDb.Status, out var status)
                        ? status
                        : PlannedStudyStatus.Planned,

                    StudyAreas = GenerateRandomStudyAreas(plannedStudyDto),
                    ProjectionPaths = plannedStudyDto.RandomProjectionPaths.Generate(),
                    ImagesCount = plannedStudyDto.RandomImagesCount.Generate()
                };

                return _plannedStudyLiteDb;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Studies not generated");
                throw;
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

        private StudyLiteDb GetRandomStudiesFromPatient(PatientLiteDb patient)
        {
            var patientStudies = GetCachedStudies()
                .Where(s => s.SnapshotPatientId == patient.PatientID)
                .ToList();

            if (!patientStudies.Any())
            {
                _logger.Warn($"No studies found for patient {patient.PatientID}");
                return null;
            }

            var index = _random.Next(patientStudies.Count);

            return patientStudies[index];
        }

        private void GetCachedPatientsAndStudies()
        {
            if (_cachedPatients == null || !_cachedPatients.Any())
            {
                _cachedPatients = _storage.Patients.GetAllPatients().ToList();
                _logger.Info($"Cached {_cachedPatients.Count} patients");
            }

            if (_cachedStudies == null || !_cachedStudies.Any())
            {
                _cachedStudies = _storage.Studies.GetAllStudies().ToList();
                _logger.Info($"Cached {_cachedStudies.Count} studies");
            }
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

        private List<StudyLiteDb> GetCachedStudies()
        {
            if (_cachedStudies == null || !_cachedStudies.Any())
            {
                _cachedStudies = _storage.Studies.GetAllStudies().ToList();
                _logger.Info($"Cached {_cachedStudies.Count} studies");
            }

            return _cachedStudies;
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

        private StudyLiteDb CreateDefaultStudy()
        {
            return new StudyLiteDb
            {
                StudyId = $"STD-{GenerateRandomNumber(12)}t",
                AccessionNumber = _random.Next(100000, 999999).ToString(),
                StudyInstanceUid = $"2.25.{GenerateRandomNumber(36)}",
                Status = "Planned"
            };
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
                    _plannedStudyStorage.PlannedStudies.DeleteAll();
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
        public async Task RefreshDataBaseAsync()
        {
            try
            {
                StartBusy("Обновление списка исследований ...");

                await Task.Run(() =>
                {
                    _plannedStudyStorage?.Dispose();
                    App.UpdatePlannedStorage(DatabasePath, IsReadOnlyMode);
                });

                await GetDataFromDbAsync();
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
        public void BrowsePatientDatabase()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "LiteDB files (*.db)|*.db|All files (*.*)|*.*",
                Title = "Выберите файл базы данных"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var newPath = dialog.FileName;

                if (DatabasePatientPath == newPath)
                {
                    UpdateText = "Эта БД уже открыта";
                    return;
                }

                DatabasePatientPath = newPath;

                App.UpdateSharedStorage(DatabasePatientPath, false);

                _cachedPatients = null;
                _cachedStudies = null;

                GetCachedPatientsAndStudies();

                UpdateText = $"БД переключена: {Path.GetFileName(newPath)}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error switching database");
                UpdateText = $"Ошибка: {ex.Message}";
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
                App.UpdatePlannedStorage(DatabasePath, readOnly);

                var anyPlannedStudies = _plannedStudyStorage.PlannedStudies.Any();

                IsReadOnlyMode = readOnly;
                UpdateText = $"Режим переключен: {(readOnly ? "Только чтение" : "Чтение/Запись")}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Не удалось переключить режим LiteDB");
                MessageBox.Show(ex.Message, "Ошибка LiteDB", MessageBoxButton.OK, MessageBoxImage.Error);
                try
                {
                    App.UpdatePlannedStorage(DatabasePath, false);

                    var anyPlannedStudies = _plannedStudyStorage.PlannedStudies.Any();

                    IsReadOnlyMode = false;
                    UpdateText = "Режим восстановлен: Чтение/Запись";
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

        [RelayCommand]
        public async Task ConnectDB()
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
        public async Task DeleteAllTablesAsync()
        {
            try
            {
                //await DeleteAllPatient();
                await RefreshDataBaseAsync();

                UpdateText = "All Tables Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Tables is not Deleted";
            }
        }

        [RelayCommand]
        public async Task OptimisationDataBase()
        {
            try
            {
                StartBusy("Оптимизация Базы данных ...", false);
                CancelButtonIsVisibil = false;

                _plannedStudyStorage?.Dispose();

                PlannedStudies = new();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Thread.Sleep(200);

                await Task.Run(() =>
                {
                    lock (_dbLock)
                    {
                        using var db = new LiteDatabase(DatabasePath);
                        db.Rebuild();
                    }
                });

                CleanupTempFiles();

                App.UpdatePlannedStorage(DatabasePath, IsReadOnlyMode);
                await GetDataFromDbAsync();
            }
            catch (Exception ex)
            {
                UpdateText = $"Error: - [{ex.Message}]";
                _logger.Error(UpdateText);
            }
            finally
            {
                StopBusy();
                CancelButtonIsVisibil = true;

                if (!string.IsNullOrEmpty(UpdateText) && !UpdateText.StartsWith("Error"))
                {
                    UpdateText = "Optimisation successful";
                }
            }
        }


        [RelayCommand]
        public void CancelGeneration()
        {
            _cancellationTokenSource?.Cancel();
            BusyMessage = "Отмена генерации...";
        }


        public async Task GetDataFromDbAsync()
        {
            StartBusy("Получение данных ...", false);

            await Task.Run(() =>
            {
                PlannedStudies = _plannedStudyStorage.PlannedStudies.GetAllStudies();
            });

            UpdateStatistics();

            StopBusy();
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

        private string BuildShortPatientPath(string fullPath, int keepFolders = 3)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            var normalized = fullPath.Replace('/', '\\').TrimEnd('\\');

            var parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= keepFolders + 1)
            {
                DatabasePatientPathShort = normalized;
                return normalized;
            }

            var fileName = parts[^1];
            var startIndex = Math.Max(0, parts.Length - (keepFolders + 1));
            var tail = string.Join("\\", parts[startIndex..]);

            DatabasePatientPathShort = $@"...\{tail}";

            return DatabasePatientPathShort;
        }

        private void CleanupTempFiles()
        {
            try
            {
                var dir = Path.GetDirectoryName(DatabasePath);
                if (string.IsNullOrEmpty(dir))
                    return;

                var baseName = Path.GetFileNameWithoutExtension(DatabasePath);
                var tempFiles = Directory.GetFiles(dir, $"{baseName}-backup*.db");

                foreach (var file in tempFiles)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.Info($"Deleted: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Cannot delete {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Cleanup error: {ex.Message}");
            }
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

        private void UpdateStatistics()
        {
            UpdateText = $"Всего - [{PlannedStudies.Count}] исследований.";
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

        private PlannedStudyLiteDbDto CreatePlannedStudyDto()
        {
            return new PlannedStudyLiteDbDto(
                new RandomBodyPartDataRule(),
                new RandomProjectionPathsRule(),
                new RandomImagesCountRule());
        }

        private PlannedStudyArea[] GenerateRandomStudyAreas(PlannedStudyLiteDbDto plannedStudyDto)
        {
            var areas = new List<PlannedStudyArea>();
            var count = _random.Next(1, 4);

            for (int i = 0; i < count; i++)
            {
                var data = plannedStudyDto.RandomBodyPart.Generate();

                areas.Add(new PlannedStudyArea
                {
                    BodyPartGuid = data.bodyPartGuid,
                    AnatomicRegionCodeValue = data.anatomicCode,
                    ViewCodeValue = data.viewCode,
                    ProjectionUid = GenerateRandomProjectionUid(),
                    LateralityCode = data.lateralityCode
                });
            }

            return areas.ToArray();
        }

        private string GenerateRandomProjectionUid()
        {
            // Можно сгенерировать случайный UID или вернуть пустую строку
            // return Guid.NewGuid().ToString("N");
            return string.Empty;
        }
    }
}
