namespace DataBaseGenerator.UI.Wpf.Model
{
    public class DicomPatientInfo
    {
        public string PatientName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string StudyDate { get; set; } = string.Empty;
        public string StudyTime { get; set; } = string.Empty;
        public string StudyId { get; set; } = string.Empty;
        public string Modality { get; set; } = string.Empty;
        public string StudyDescription { get; set; } = string.Empty;

        public string SeriesDate { get; set; } = string.Empty;
        public string SeriesTime { get; set; } = string.Empty;
        public string SeriesDescription { get; set; } = string.Empty;
    }
}
