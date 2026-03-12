using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class LiteDbGeneratorViewModel : ObservableObject
    {

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        private IStudyStorageModule _studyStorageModule;
        private string _gender;
        private PatientLiteDb _patientLiteDb;


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
        public partial Patient SelectedPatient { get; set; }

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
        public partial int SetPatientCount { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<PatientLiteDb> AllPatients { get; set; }





        public LiteDbGeneratorViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _studyStorageModule = _serviceProvider.GetService<IStudyStorageModule>();

            Gender = new List<string> { "Man", "Female", "Other" };

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            AllPatients = new();
            UseRandomBirthdate = true;

            AllPatients = _studyStorageModule.Patients.GetAllPatients();
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

                // await _patientService.AddOneAsync(newPatient);
                var patientLiteDb = ConvertPatientToPatientLiteDb(newPatient);
                _studyStorageModule.Patients.Upsert(patientLiteDb);

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

                var patientDto = CreatePatientDto();

                await GenerateLiteDbPatientAsync(patientDto);

                await RefreshPatientsAsync();

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

                //_studyStorageModule.Patients.Delete(patient);
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

                //await _patientService.DeleteAllAsync();

                //_studyStorageModule.Patients.Delete();
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
                await DeleteAllPatientAsync();
                await RefreshPatientsAsync();

                UpdateText = "All Tables Deletion completed";
            }
            catch (Exception ex)
            {
                UpdateText = "Tables is not Deleted";
            }
        }



        [RelayCommand]
        public async Task RefreshPatientsAsync()
        {
            AllPatients = _studyStorageModule.Patients.GetAllPatients();
            //MainWindow.AllPatientView.ItemsSource = null;
            //MainWindow.AllPatientView.Items.Clear();
            //MainWindow.AllPatientView.ItemsSource = AllPatients;
            //MainWindow.AllPatientView.Items.Refresh();
            UpdateText = "Patient table is update";
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
                var patients = _studyStorageModule.Patients.GetAllPatients();

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


        public async Task GenerateLiteDbPatientAsync(PatientGeneratorDto inputParameters)
        {
            try
            {
                for (var patientindex = 0; patientindex < inputParameters.PatientCount; patientindex++)
                {
                    await CreateLiteDbPatientAsync(patientindex, inputParameters);
                }
                _logger.Info($"Created {inputParameters.PatientCount} patients");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Patient not generated");
            }
        }

        public async Task CreateLiteDbPatientAsync(int patientIndex, PatientGeneratorDto patientGeneratorParameters)
        {
            _patientLiteDb = GenerateLiteDbPatients(patientIndex, patientGeneratorParameters);

            //bool checkIsExist = _studyStorageModule.Patients.Exists(element => element.PatientId == _patientLiteDb.PatientId);

            //if (checkIsExist)
            //    return;

            //var patients = ConvertPatientLiteDbToLitePatient(_patientLiteDb);
            _studyStorageModule.Patients.Upsert(_patientLiteDb);
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




    }
}
