using System.Collections.Generic;

namespace DataBaseGenerator.UI.Wpf.ViewModel.Options
{
    public class ParserSourcesConfig
    {
        public string DataBasePath { get; set; } = string.Empty;
        public List<ParserFolderConfig> Folders { get; set; } = [];
        public List<ParserPacsConfig> Pacs { get; set; } = [];
    }
}
