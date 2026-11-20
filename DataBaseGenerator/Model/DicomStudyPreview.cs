using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DataBaseGenerator.UI.Wpf.Model
{
    public partial class DicomStudyPreview : ObservableObject
    {
        public string StudyInstanceUid { get; init; } = string.Empty;

        public string PatientName { get; init; } = string.Empty;

        public string PatientId { get; init; } = string.Empty;

        public string StudyDate { get; init; } = string.Empty;

        public string StudyDescription { get; init; } = string.Empty;

        public List<string> FilePaths { get; init; } = [];

        public int FileCount => FilePaths.Count;

        [ObservableProperty]
        public partial bool IsSelected { get; set; } = true;
    }
}
