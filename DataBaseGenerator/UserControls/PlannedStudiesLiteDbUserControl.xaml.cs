using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for PlannedStudiesLiteDbUserControl.xaml
    /// </summary>
    public partial class PlannedStudiesLiteDbUserControl : UserControl
    {
        public LiteDbPlannedStudiesViewModel ViewModel { get; private set; }

        public PlannedStudiesLiteDbUserControl()
        {
            ViewModel = App.GetService<LiteDbPlannedStudiesViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
