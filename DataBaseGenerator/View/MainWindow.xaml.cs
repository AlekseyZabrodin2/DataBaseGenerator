using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow
    {

        //Create ListView для возможности обновлять таблицу

        public static ListView AllPatientView;
        public static ListView AllWorkListView;

        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = App.GetService<MainViewModel>();

            InitializeComponent();

            DataContext = ViewModel;

            AllPatientView = ViewAllPatient;
            AllWorkListView = ViewAllWorkList;
        }
    }
}
