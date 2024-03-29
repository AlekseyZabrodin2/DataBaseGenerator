﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using DataBaseGenerator.Core.GeneratorRules.Patient;
using DataBaseGenerator.Core.GeneratorRules.WorkList;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using Prism.Commands;
using Prism.Mvvm;

namespace DataBaseGenerator.UI.Wpf
{
    public class MainViewModel : BindableBase
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
        private int _patientCount;
        private int _workListCount;
        private RandomModalityRule _modality;
        private string _aeTitle;





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
                return _patientCount;
            }

            set
            {
                SetProperty(ref _patientCount, value);
            }
        }


        public int SetWorkListCount
        {
            get
            {
                return _workListCount;
            }

            set
            {
                SetProperty(ref _workListCount, value);
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


        private DelegateCommand _connectDB;
        public ICommand ConnectDB => _connectDB = new DelegateCommand(PerformConnectDB);

        private void PerformConnectDB()
        {
            try
            {
                _myConnection = new MySqlConnection(_connect);

                _myConnection.Open();

                UpdateText = "DB connect";

            }
            catch (Exception e)
            {
                UpdateText = "DB not connect";
            }
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
                    PatientCount = _patientCount
                };

                var addPatient = DataBaseCommand.GeneratePatientDateBase(newPatient);

                PerformRefreshPatients();

                UpdateText = "Patient added";
            }

            catch (Exception e)
            {
                UpdateText = "Patient not added";
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
                    WorkListCount = _workListCount
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



        private DelegateCommand _deletePatient;
        public ICommand DeletePatient => _deletePatient ??= new DelegateCommand(PerformDeletePatient);

        private void PerformDeletePatient()
        {
            try
            {
                _myConnection = new MySqlConnection(_connect);

                _myConnection.Open();

                _adapter = new MySqlDataAdapter();

                _mySqlCommand = new MySqlCommand($"DROP TABLE medxregistry.patient, medxregistry.worklist;", _myConnection);

                _dataReader = _mySqlCommand.ExecuteReader();

                UpdateText = "DataBases DELETE";

                while (_dataReader.Read())
                {
                    UpdateText = _dataReader[0].ToString();
                }

                _dataReader.Close();

                _myConnection.Close();

                PerformRefreshPatients();

                PerformRefreshWorkList();

                UpdateText = "Deletion completed";
            }
            catch (Exception e)
            {
                UpdateText = "DataBase is not Deleted";
            }
        }
    }
}
