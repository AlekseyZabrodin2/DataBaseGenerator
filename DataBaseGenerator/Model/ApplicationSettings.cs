namespace DataBaseGenerator.UI.Wpf.Model
{
    public class ApplicationSettings
    {
        public string MySqlServer { get; set; } = "localhost";
        public string MySqlDatabase { get; set; } = string.Empty;
        public string MySqlUser { get; set; } = string.Empty;
        public string MySqlPassword { get; set; } = string.Empty;
        public uint MySqlPort { get; set; } = 3306;

        public string WebHostUrl { get; set; } = "http://localhost:5289";
        public string WebHostApiUrl { get; set; } = "http://localhost:5289/api/";
    }
}
