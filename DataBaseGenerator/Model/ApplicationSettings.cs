using CommunityToolkit.Mvvm.ComponentModel;

namespace DataBaseGenerator.UI.Wpf.Model
{
    public partial class ApplicationSettings : ObservableObject
    {
        public string MySqlServer { get; set; } = "localhost";
        public string MySqlDatabase { get; set; } = string.Empty;
        public string MySqlUser { get; set; } = string.Empty;
        public string MySqlPassword { get; set; } = string.Empty;
        public uint MySqlPort { get; set; } = 3306;

        [ObservableProperty]
        public partial string OrthancBaseUrl { get; set; } = "http://localhost:8042";

        [ObservableProperty]
        public partial string OrthancUsername { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string OrthancPassword { get; set; } = string.Empty;
    }
}
