using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for StudiesLiteDbTableUserControl.xaml
    /// </summary>
    public partial class StudiesLiteDbTableUserControl : UserControl
    {
        public LiteDbStudiesTableViewModel ViewModel { get; private set; }

        public StudiesLiteDbTableUserControl()
        {
            ViewModel = App.GetService<LiteDbStudiesTableViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
