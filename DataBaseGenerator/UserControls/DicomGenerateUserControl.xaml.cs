using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    public partial class DicomGenerateUserControl : UserControl
    {
        public FileGeneratorViewModel ViewModel { get; }

        public DicomGenerateUserControl()
        {
            InitializeComponent();

            ViewModel = App.GetService<FileGeneratorViewModel>();
            DataContext = ViewModel;
            GenerationOptionsHost.DataContext = ViewModel.GenerationOptions;
            IdOptionsHost.DataContext = ViewModel.IdOptions;
            StudyPeriodHost.DataContext = ViewModel.StudyPeriod;
            SeriesTimeHost.DataContext = ViewModel.SeriesTime;
        }
    }
}
