using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = App.GetService<MainViewModel>();

            InitializeComponent();

            DataContext = ViewModel;
        }
    }
}
