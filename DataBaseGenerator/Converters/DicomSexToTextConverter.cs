using System;
using System.Globalization;
using System.Windows.Data;

namespace DataBaseGenerator.UI.Wpf.Converters
{
    /// <summary>Shows a DICOM PatientSex code (M/F/O) as a word in tables (male / female / other).</summary>
    public sealed class DicomSexToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var code = value?.ToString()?.Trim().ToUpperInvariant();

            return code switch
            {
                "M" or "MAN" => "Male",
                "F" or "FEMALE" => "Female",
                "O" or "OTHER" => "Other",
                _ => string.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString()?.Trim().ToLowerInvariant();

            return text switch
            {
                "male" or "муж" or "m" or "man" => "M",
                "female" or "жен" or "f" => "F",
                "other" or "др" or "o" => "O",
                _ => Binding.DoNothing
            };
        }
    }
}
