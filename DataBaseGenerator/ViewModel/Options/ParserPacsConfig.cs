using DataBaseGenerator.UI.Wpf.Enums;

namespace DataBaseGenerator.UI.Wpf.ViewModel.Options
{
    public class ParserPacsConfig
    {
        public string Name { get; set; } = string.Empty;
        public string CallingAe { get; set; } = string.Empty;
        public string PacsHost { get; set; } = string.Empty;
        public string PacsPort { get; set; } = string.Empty;
        public string CalledAe { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public PacsLoadingMode LoadingMode { get; set; }
    }
}
