using CommunityToolkit.Mvvm.ComponentModel;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class DicomStudyPeriodOptions : ObservableObject
    {
        [ObservableProperty]
        public partial bool ForDay { get; set; }

        [ObservableProperty]
        public partial bool ForWeek { get; set; }

        [ObservableProperty]
        public partial bool ForMonth { get; set; }

        public bool UseCustomPeriod => !ForDay && !ForWeek && !ForMonth;

        partial void OnForDayChanged(bool value) => SelectOnly(value, clearWeek: true, clearMonth: true);

        partial void OnForWeekChanged(bool value) => SelectOnly(value, clearDay: true, clearMonth: true);

        partial void OnForMonthChanged(bool value) => SelectOnly(value, clearDay: true, clearWeek: true);

        public void Reset()
        {
            ForDay = false;
            ForWeek = false;
            ForMonth = false;
            OnPropertyChanged(nameof(UseCustomPeriod));
        }

        private void SelectOnly(bool selected, bool clearDay = false, bool clearWeek = false, bool clearMonth = false)
        {
            if (selected)
            {
                if (clearDay)
                    ForDay = false;
                if (clearWeek)
                    ForWeek = false;
                if (clearMonth)
                    ForMonth = false;
            }

            OnPropertyChanged(nameof(UseCustomPeriod));
        }
    }
}