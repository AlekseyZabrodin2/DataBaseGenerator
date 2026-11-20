using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls;

public partial class OrthancWorklistUserControl : UserControl
{
    public OrthancWorklistViewModel ViewModel { get; }

    public OrthancWorklistUserControl()
    {
        ViewModel = App.GetService<OrthancWorklistViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
