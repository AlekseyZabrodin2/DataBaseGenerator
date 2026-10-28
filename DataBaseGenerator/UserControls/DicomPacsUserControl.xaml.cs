using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    public partial class DicomPacsUserControl : UserControl
    {
        public DicomPacsViewModel ViewModel { get; }

        public DicomPacsUserControl()
        {
            InitializeComponent();

            ViewModel = App.GetService<DicomPacsViewModel>();
            DataContext = ViewModel;
        }
    }
}
