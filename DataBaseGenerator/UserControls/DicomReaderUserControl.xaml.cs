using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    public partial class DicomReaderUserControl : UserControl
    {
        public DicomReaderViewModel ViewModel { get; }

        public DicomReaderUserControl()
        {
            InitializeComponent();

            ViewModel = App.GetService<DicomReaderViewModel>();
            DataContext = ViewModel;
        }
    }
}
