using System;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataBaseGenerator.Core.LiteDbGenerator.Data;

namespace DataBaseGenerator.UI.Wpf.ViewModel.LiteDbTabs
{
    public partial class PatientDatabaseTab : ObservableObject
    {
        public string FilePath { get; }
        public string TabHeader => System.IO.Path.GetFileName(FilePath);
        public string ToolTip => FilePath;

        public bool HasPatientsTable { get; set; }
        public bool HasStudiesTable { get; set; }
        public bool HasSeriesTable { get; set; }
        public bool HasImagesTable { get; set; }

        [ObservableProperty]
        public partial bool ShowPatients { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowStudies { get; set; }

        [ObservableProperty]
        public partial bool ShowSeries { get; set; }

        [ObservableProperty]
        public partial bool ShowImages { get; set; }

        public PatientDatabaseTab(string filePath)
        {
            FilePath = filePath;
            AnalyzeDatabase();
        }

        private void AnalyzeDatabase()
        {
            try
            {
                using var storage = new LiteDbStudyStorageModule(FilePath, true);

                var collections = storage.GetCollectionNames();

                HasPatientsTable = collections.Contains("patients");
                HasStudiesTable = collections.Contains("studies");
                HasSeriesTable = collections.Contains("series");
                HasImagesTable = collections.Contains("images");

                if (HasPatientsTable)
                    ShowPatients = true;
                else if (HasStudiesTable)
                    ShowStudies = true;
                else if (HasSeriesTable)
                    ShowSeries = true;
                else if (HasImagesTable)
                    ShowImages = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error analyzing database {FilePath}: {ex.Message}");
            }
        }
    }
}
