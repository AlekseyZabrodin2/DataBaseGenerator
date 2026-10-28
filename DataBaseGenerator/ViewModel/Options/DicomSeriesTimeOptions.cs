using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DataBaseGenerator.UI.Wpf.ViewModel
{
    public partial class DicomSeriesTimeOptions : ObservableObject
    {
        [ObservableProperty]
        public partial bool Morning { get; set; }

        [ObservableProperty]
        public partial bool Day { get; set; }

        [ObservableProperty]
        public partial bool Evening { get; set; }

        public bool UseCustomTime => !Morning && !Day && !Evening;

        public TimeSpan Start
        {
            get
            {
                if (Morning)
                    return new TimeSpan(8, 0, 0);
                if (Day)
                    return new TimeSpan(12, 0, 0);
                if (Evening)
                    return new TimeSpan(17, 0, 0);
                return TimeSpan.Zero;
            }
        }

        public TimeSpan End
        {
            get
            {
                if (Morning)
                    return new TimeSpan(12, 0, 0);
                if (Day)
                    return new TimeSpan(17, 0, 0);
                if (Evening)
                    return new TimeSpan(21, 0, 0);
                return TimeSpan.Zero;
            }
        }

        partial void OnMorningChanged(bool value) => SelectOnly(value, clearDay: true, clearEvening: true);

        partial void OnDayChanged(bool value) => SelectOnly(value, clearMorning: true, clearEvening: true);

        partial void OnEveningChanged(bool value) => SelectOnly(value, clearMorning: true, clearDay: true);

        public void Reset()
        {
            Morning = false;
            Day = false;
            Evening = false;
            OnPropertyChanged(nameof(UseCustomTime));
        }

        private void SelectOnly(bool selected, bool clearMorning = false, bool clearDay = false, bool clearEvening = false)
        {
            if (selected)
            {
                if (clearMorning)
                    Morning = false;
                if (clearDay)
                    Day = false;
                if (clearEvening)
                    Evening = false;
            }

            OnPropertyChanged(nameof(UseCustomTime));
        }
    }
}