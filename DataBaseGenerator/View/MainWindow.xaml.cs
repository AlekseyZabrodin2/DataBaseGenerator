using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
