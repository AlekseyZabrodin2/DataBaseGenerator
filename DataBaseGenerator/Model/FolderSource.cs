using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace DataBaseGenerator.UI.Wpf.Model
{
    public partial class FolderSource : ObservableObject
    {
        [ObservableProperty]
        public partial string FolderName { get; set; }

        [ObservableProperty]
        public partial string FolderPath { get; set; }

        [ObservableProperty]
        public partial bool IsEnabled { get; set; }



        [RelayCommand]
        private void Browse()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                FolderPath = dialog.FolderName;
            }
        }
    }
}
