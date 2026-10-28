using CommunityToolkit.Mvvm.ComponentModel;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class DicomPatientIdOptions : ObservableObject
    {
        [ObservableProperty]
        public partial bool UseRandomPrefix { get; set; }

        [ObservableProperty]
        public partial int PrefixLength { get; set; } = 4;

        [ObservableProperty]
        public partial int IdLength { get; set; } = 7;

        [ObservableProperty]
        public partial bool OmitPatientSex { get; set; }

        public void Reset()
        {
            UseRandomPrefix = false;
            PrefixLength = 4;
            IdLength = 7;
            OmitPatientSex = false;
        }
    }
}
