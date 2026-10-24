using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    public partial class LiteDbViewerUserControl : UserControl
    {
        public LiteDbViewerViewModel ViewModel { get; private set; }

        public LiteDbViewerUserControl()
        {
            ViewModel = App.GetService<LiteDbViewerViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
