using System.Windows;

namespace DataBaseGenerator.UI.Wpf.Model
{
    public class DicomTagInfo
    {
        public string GroupElement { get; set; }
        public string TagDescription { get; set; }
        public string Value { get; set; }

        public int Level { get; set; }

        public Thickness Indent => new(10 + Level * 20, 0, 0, 0);
    }
}
