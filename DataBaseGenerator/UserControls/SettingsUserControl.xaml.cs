using System.Windows;
using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsUserControl.xaml
    /// </summary>
    public partial class SettingsUserControl : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsUserControl()
        {
            InitializeComponent();

            ViewModel = App.GetService<SettingsViewModel>();
            DataContext = ViewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
                viewModel.Settings.MySqlPassword = ((PasswordBox)sender).Password;
        }
    }
}
