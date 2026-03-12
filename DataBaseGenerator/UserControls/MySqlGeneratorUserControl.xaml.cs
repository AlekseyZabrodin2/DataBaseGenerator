using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for MySqlGeneratorUserControl.xaml
    /// </summary>
    public partial class MySqlGeneratorUserControl : UserControl
    {
        public MySqlGeneratorViewModel ViewModel { get; }

        public MySqlGeneratorUserControl()
        {
            ViewModel = App.GetService<MySqlGeneratorViewModel>();

            InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
