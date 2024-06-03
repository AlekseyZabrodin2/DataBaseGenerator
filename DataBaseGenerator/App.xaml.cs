using System;
using System.Windows;
using DataBaseGenerator.UI.Wpf.View;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var mainWindow = new MainWindow();
                mainWindow.DataContext = new MainViewModel();
                mainWindow.Show();
            }

            catch(Exception ex)
            {
                MessageBox.Show("Check your connection settings !!!");                
                base.Shutdown();
            }

        }

    }
}
