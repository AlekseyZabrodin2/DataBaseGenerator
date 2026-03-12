using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for LiteDbGeneratorUserControl.xaml
    /// </summary>
    public partial class LiteDbGeneratorUserControl : UserControl
    {
        public LiteDbGeneratorViewModel ViewModel { get; }

        public LiteDbGeneratorUserControl()
        {
            ViewModel = App.GetService<LiteDbGeneratorViewModel>();

            InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
