using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    public partial class PatientGenerationOptionsControl : UserControl
    {
        public PatientGenerationOptionsControl()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                if (DataContext is PatientGenerationOptions options)
                    options.RefreshBirthdate();
            };
        }
    }
}
