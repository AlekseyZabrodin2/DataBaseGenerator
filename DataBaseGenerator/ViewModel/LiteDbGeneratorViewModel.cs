using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Contracts;
using DataBaseGenerator.Core.LiteDbGenerator.Enums;
using DataBaseGenerator.Core.LiteDbGenerator.LiteDbModels;
using DataBaseGenerator.Core.LiteDbGenerator.Models;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient;
using LiteDB;
using Microsoft.Win32;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LiteDbGeneratorViewModel : ObservableObject
    {

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        private IStudyStorageModule _storage => App.SharedStorage;
        private string _gender;
        private PatientLiteDb _patientLiteDb;
        private string _databasePath = string.Empty;
        private readonly object _dbLock = new object();


        [ObservableProperty]
        public partial string UpdateText { get; set; }

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
        public partial bool UseRandomBirthdate { get; set; } = true;

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
        public partial int SetPatientCount { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<PatientLiteDb> AllPatients { get; set; }

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

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial bool PercentShow { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        [ObservableProperty]
        public partial bool ShowPatients { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowStudies { get; set; }

        [ObservableProperty]
        public partial bool ShowSeries { get; set; }

        [ObservableProperty]
        public partial bool ShowImages { get; set; }

        //[ObservableProperty]
        //private partial PatientsTableViewModel PatientsVM { get; set; }

        [ObservableProperty]
        public partial LiteDbStudiesTableViewModel StudiesVM { get; set; }

        //[ObservableProperty]
        //private partial SeriesTableViewModel SeriesVM { get; set; }

        //[ObservableProperty]
        //private partial ImagesTableViewModel ImagesVM { get; set; }

        private CancellationTokenSource _cancellationTokenSource; 
        
        [ObservableProperty]
        public partial bool OptimisationIsEnabled { get; set; } = true;



        public LiteDbGeneratorViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
            Gender = new List<string> { "Man", "Female", "Other" };

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            AllPatients = new();
            IsReadOnlyMode = true;

            DatabasePath = _storage.DatabasePath;

            await GetAllPatientsAsync();
        }

        public async Task GetAllPatientsAsync()
        {
            try
            {
                StartBusy("Обновление списка пациентов...");

                await Task.Run(() =>
                {
                    AllPatients = _storage.Patients.GetAllPatients();
                });

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

        private void UpdateStatistics()
        {
            UpdateText = $"Всего - [{AllPatients.Count}] пациентов.";
        }


        [RelayCommand]
        public void AddOnePatient()
        {
            var messageToUpdateText = string.Empty;
            try
            {
                StartBusy("Генерация пациента...");

                AddFullName = $"{AddFamily} {AddName} {AddMiddleName}";

                var newPatient = new PatientInputParameters(                    
                    1,
                    AddFamily,
                    AddName,
                    AddMiddleName,
                    AddFullName,
                    AddIdPatient,
                    PatientBirthDate,
                    SelecedGender,
                    AddAdress,
                    AddInfo,
                    AddWorkPlase)
                {
                    PatientCount = SetPatientCount
                };

                // await _patientService.AddOneAsync(newPatient);
                var patientLiteDb = ConvertPatientToPatientLiteDb(newPatient);
                _storage.Patients.Upsert(patientLiteDb);

                _ = RefreshDataBaseAsync();
                CleareFields();

                UpdateText = !string.IsNullOrEmpty(messageToUpdateText)
                    ? messageToUpdateText
                    : "Patient added";
            }
            catch (Exception ex)
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
                    : $"Пациент не добавлен {ex.Message}";
            }
            finally { StopBusy(); }
        }

        [RelayCommand]
        public void CancelAddPatient()
        {
            AddIdPatient = string.Empty;
            AddFamily = string.Empty;
            AddName = string.Empty;
            AddMiddleName = string.Empty;
            AddFullName = string.Empty;
            AddAdress = string.Empty;
            AddWorkPlase = string.Empty;
            AddInfo = string.Empty;
            SelecedGender = null;
            MedicalInsuranceNumber = string.Empty;
            UpdateText = "Как скажете, отменяю !";
        }

        [RelayCommand]
        public async Task AddPatientAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            _cancellationTokenSource = new CancellationTokenSource();
            var generationCancelled = false;

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

                var progress = new Progress<int>(percent =>
                {
                    CurrentProgress = percent;
                    BusyMessage = $"Генерация пациентов ...";
                });

                var patientDto = CreatePatientDto();

                await GeneratePatientByInsertBulkAsync(patientDto, progress, _cancellationTokenSource.Token);

                await RefreshDataBaseAsync();

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    generationCancelled = true;
                    return;
                }

                UpdateText = $"Пациент успешно добавлен! Всего - [{AllPatients.Count}]";
            }
            catch (Exception ex)
            {
                UpdateText = "Пациент не добавлен";
                _logger.Error(ex, "Error in patient generation");
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
                    UpdateText = $"Генерация пациентов была прервана! Всего - [{AllPatients.Count}]. Время выполнения: {timeString}";
                }
                else
                {
                    UpdateText = $"Пациент успешно добавлен! Всего - [{AllPatients.Count}]. Время выполнения: {timeString}";
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        public void SavePatients()
        {
            try
            {
                var updatePatients = new ObservableCollection<PatientLiteDb>();
                foreach (var patient in AllPatients)
                {
                    updatePatients.Add(patient);
                }

                //await _patientService.EditeAsync(updatePatients);
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
        public void DeleteFirstPatient()
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

                //_studyStorageModule.Patients.Delete(patient);
                _ = RefreshDataBaseAsync();

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
        public async Task DeleteAllPatient()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                StartBusy("Удаление пациентов...", false);

                //await _patientService.DeleteAllAsync();

                await Task.Run(() =>
                {
                    _storage.Patients.DeleteAll();
                });               

                await RefreshDataBaseAsync();
            }
            catch (Exception ex)
            {
                UpdateText = $"Ошибка удаления: - [{ex.Message}]";
                _logger.Error(UpdateText);
            }
            finally
            {
                stopwatch.Stop();
                var timeString = FormatTimeSpan(stopwatch.Elapsed);

                _logger.Info("<<< DeleteAllPatient: END");
                StopBusy();

                if (UpdateText?.Contains("Ошибка") != true)
                {
                    UpdateText = $"Пациенты успешно удалены! Всего - [{AllPatients.Count}]. Время выполнения: {timeString}";
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
                await DeleteAllPatient();
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
                _storage?.Dispose();

                AllPatients = new ObservableCollection<PatientLiteDb>();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Thread.Sleep(200);

                lock (_dbLock)
                {
                    using var db = new LiteDatabase(DatabasePath);
                    db.Rebuild();
                }

                CleanupTempFiles();

                App.UpdateSharedStorage(DatabasePath, IsReadOnlyMode);
                await GetAllPatientsAsync();
            }
            catch (Exception ex)
            {
                UpdateText = $"Error: - [{ex.Message}]";
                _logger.Error(UpdateText);
            }
            finally
            {
                if (!string.IsNullOrEmpty(UpdateText) && !UpdateText.StartsWith("Error"))
                {
                    UpdateText = "Optimisation successful";
                }
            }
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
                    catch { }
                }
            }
            catch { }
        }

        [RelayCommand]
        public async Task RefreshDataBaseAsync()
        {
            try
            {
                StartBusy("Обновление списка пациентов...");

                _storage?.Dispose();
                App.UpdateSharedStorage(DatabasePath, IsReadOnlyMode);                              

                await Task.Run(() =>
                {
                    AllPatients = _storage.Patients.GetAllPatients();
                });

                UpdateText = $"База обновлена. Найдено пациентов: {AllPatients.Count}";
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

        private PatientLiteDb ConvertPatientToPatientLiteDb(PatientInputParameters patient, string patientId = null)
        {
            return new PatientLiteDb()
            {
                Id = patientId,
                PatientID = patient.PatientID,
                LastName = patient.LastName,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                BirthDate = patient.BirthDate,
                Sex = patient.Sex switch
                {
                    "Мужской" => PatientSex.Male,
                    "Женский" => PatientSex.Female,
                    "Не определен" => PatientSex.Other,
                    _ => PatientSex.Other
                },
                //Phone = patient.Telephone,
                Address = patient.Address,
                Comments = patient.AddInfo
                //Age = patient.Age
            };
        }

        private PatientLiteDb ConvertPatientToPatientLiteDb(PatientGeneratorParameters patient, string patientId = null)
        {
            PatientSex parsedSex = Enum.TryParse<PatientSex>(patient.Sex.ToString(), out var sex) ? sex : PatientSex.Other;

            return new PatientLiteDb()
            {
                Id = patientId,
                PatientID = patient.PatientID.ToString(),
                LastName = patient.LastName.ToString(),
                FirstName = patient.FirstName.ToString(),
                MiddleName = patient.MiddleName.ToString(),
                BirthDate = DateTime.Parse(patient.BirthDate.ToString()),
                Sex = parsedSex,
                //Phone = patient.Telephone,
                Address = patient.Address.ToString(),
                Comments = patient.AddInfo.ToString()
                //Age = patient.Age
            };
        }

        public ObservableCollection<PatientLiteDb> GetAllLiteDbPatients()
        {
            try
            {
                var patientCollection = new ObservableCollection<PatientLiteDb>();
                var patients = _storage.Patients.GetAllPatients();

                foreach (var patient in patients)
                {
                    patientCollection.Add(patient);
                }

                Console.WriteLine($"Найдено пациентов: {patients.Count}");
                return patientCollection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пациентов: {ex.Message}");
                return new ObservableCollection<PatientLiteDb>();
            }
        }

        private PatientLiteDb ConvertPatientToPatientLiteDb(LitePatient patient, string patientId = null)
        {
            return new PatientLiteDb()
            {
                Id = patientId,
                PatientID = patient.PatientId,
                LastName = patient.LastName,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                BirthDate = patient.BirthDate,
                Sex = patient.Sex,
                Phone = patient.Phone,
                Address = patient.Address,
                Comments = patient.Comments
            };
        }

        //public async Task UpsertLiteDbPatient(PatientLiteDb patient)
        //{
        //    var entity = PatientMapper.ToLite(patient);

        //    // Preserve existing document by PatientId if no internal id is set
        //    if (entity.Id == ObjectId.Empty)
        //    {
        //        var existing = _studyStorageModule.Patients.FindOne(p => p.PatientId == entity.PatientId);
        //        if (existing is not null)
        //        {
        //            entity.Id = existing.Id;
        //        }
        //    }

        //    _studyStorageModule.Patients.Upsert(entity);
        //}


        public async Task GeneratePatientByInsertBulkAsync(PatientGeneratorDto inputParameters, IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                var total = inputParameters.PatientCount;
                var batchSize = 5000;
                var patientsBatch = new List<PatientLiteDb>(batchSize);
                var prefix = "MXE";
                var digitsCount = 7;
                var format = new string('0', digitsCount);


                var nextPatientId = GenerateNextPatientId(prefix, digitsCount, 1);
                var currentNumber = uint.Parse(new Regex(@"[^\d]").Replace(nextPatientId, string.Empty));

                await Task.Run(() =>
                {
                    for (var patientindex = 0; patientindex < total; patientindex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.Info("Cancellation requested, stopping generation...");
                            break;
                        }

                        var patientId = $"{prefix}{(currentNumber + patientindex).ToString(format)}";
                        var patient = GenerateLiteDbPatients(patientindex, inputParameters);
                        patient.PatientID = patientId;
                        patientsBatch.Add(patient);

                        if (patientsBatch.Count >= batchSize || patientindex == total - 1)
                        {
                            _storage.Patients.InsertBulk(patientsBatch);
                            patientsBatch.Clear();

                            progress?.Report((int)((double)(patientindex + 1) / total * 100));
                        }
                    }
                }, cancellationToken);

                _logger.Info($"Created {inputParameters.PatientCount} patients");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Patient not generated");
                throw;
            }
        }

        public async Task GeneratePatientByOneAsync(PatientGeneratorDto inputParameters, IProgress<int> progress, CancellationToken cancellationToken)
        {
            try
            {
                var total = inputParameters.PatientCount;

                var prefix = "MXE";
                var digitsCount = 7;
                var format = new string('0', digitsCount);


                var nextPatientId = GenerateNextPatientId(prefix, digitsCount, 1);
                var currentNumber = uint.Parse(new Regex(@"[^\d]").Replace(nextPatientId, string.Empty));

                await Task.Run(() =>
                {
                    for (var patientindex = 0; patientindex < total; patientindex++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.Info("Cancellation requested, stopping generation...");
                            break;
                        }

                        var patientId = $"{prefix}{(currentNumber + patientindex).ToString(format)}";

                        CreateLiteDbPatient(patientindex, inputParameters, patientId);
                        progress?.Report((int)((double)(patientindex + 1) / total * 100));
                    }
                }, cancellationToken);

                _logger.Info($"Created {inputParameters.PatientCount} patients");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Patient not generated");
                throw;
            }
        }

        public void CreateLiteDbPatient(int patientIndex, PatientGeneratorDto patientGeneratorParameters, string newPatientId)
        {
            _patientLiteDb = GenerateLiteDbPatients(patientIndex, patientGeneratorParameters);
            _patientLiteDb.PatientID = newPatientId;
            _storage.Patients.Upsert(_patientLiteDb);
        }

        private PatientLiteDb GenerateLiteDbPatients(int patientIndex, PatientGeneratorDto patientGeneratorParameters)
        {
            return new PatientLiteDb
            {
                Id = patientGeneratorParameters.ID_Patient.Generate(patientIndex).ToString(),
                LastName = patientGeneratorParameters.LastName.GenerateLastName(patientGeneratorParameters),
                FirstName = patientGeneratorParameters.FirstName.GenerateFirstName(patientGeneratorParameters),
                MiddleName = patientGeneratorParameters.MiddleName.GenerateMiddleNames(patientGeneratorParameters),
                PatientID = patientGeneratorParameters.PatientID.Generate(patientIndex),
                BirthDate = patientGeneratorParameters.BirthDate.GenerateBirthdate(patientGeneratorParameters),
                Sex = patientGeneratorParameters.Sex.Generate() switch
                {
                    "M" => PatientSex.Male,
                    "F" => PatientSex.Female,
                    "O" => PatientSex.Other,
                    _ => PatientSex.Other
                },
                Address = patientGeneratorParameters.Address.Generate(),
                Comments = patientGeneratorParameters.AddInfo.Generate(),
                Phone = string.Empty,
                Age = string.Empty
            };
        }

        private LitePatient ConvertPatientLiteDbToLitePatient(PatientLiteDb patient)
        {
            return new LitePatient()
            {
                PatientId = patient.PatientID,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                MiddleName = patient.MiddleName,
                BirthDate = patient.BirthDate,
                Sex = patient.Sex,
                Phone = patient.Phone,
                Address = patient.Address,
                Comments = patient.Comments
            };
        }


        private PatientGeneratorDto CreatePatientDto()
        {
            return new PatientGeneratorDto(
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
                AllPatients = _storage.Patients.GetAllPatients();

                await GetAllPatientsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Не удалось переключить режим LiteDB");
                MessageBox.Show(ex.Message, "Ошибка LiteDB", MessageBoxButton.OK, MessageBoxImage.Error);
                try
                {
                    App.UpdateSharedStorage(DatabasePath, false);
                    AllPatients = _storage.Patients.GetAllPatients();
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
        public void CancelGeneration()
        {
            _cancellationTokenSource?.Cancel();
            BusyMessage = "Отмена генерации...";
        }

        public string GenerateNextPatientId(string prefix, int digitsCount, int startsFrom = 1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digitsCount);

            var format = new string('0', digitsCount);
            
            var matchedIds = _storage.Patients.GetAllPatients()
                .Select(p => p.PatientID) 
                .Where(id => id.StartsWith(prefix))
                .ToList();

            if (matchedIds.Count == 0)
                return $"{prefix}{((uint)startsFrom).ToString(format)}";

            var nonDigitPattern = new Regex(@"[^\d]");

            var maxIdValue = matchedIds
                .Select(id =>
                {
                    var integerPart = nonDigitPattern.Replace(id, string.Empty);
                    return uint.TryParse(integerPart, out var result) ? result : 0;
                })
                .Max();

            var newIdValue = maxIdValue + 1 < (uint)startsFrom
                ? (uint)startsFrom
                : maxIdValue + 1;

            return $"{prefix}{newIdValue.ToString(format)}";
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
            PercentShow= false;
            CurrentProgress = 0;
        }
    }
}
