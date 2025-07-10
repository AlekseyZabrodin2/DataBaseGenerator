using System.Windows.Controls;

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

        public MainWindow()
        {
            InitializeComponent();

            AllPatientView = ViewAllPatient;

            AllWorkListView = ViewAllWorkList;
        }
    }
}
