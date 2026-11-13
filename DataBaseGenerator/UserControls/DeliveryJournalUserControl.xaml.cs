using System.Windows.Controls;
using DataBaseGenerator.UI.Wpf.ViewModel;

namespace DataBaseGenerator.UI.Wpf.UserControls;

public partial class DeliveryJournalUserControl : UserControl
{
    public DeliveryJournalViewModel ViewModel { get; }

    public DeliveryJournalUserControl()
    {
        ViewModel = App.GetService<DeliveryJournalViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
