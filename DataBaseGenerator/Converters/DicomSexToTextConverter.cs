using System;
using System.Globalization;
using System.Windows.Data;

namespace DataBaseGenerator.UI.Wpf.Converters
{
    /// <summary>Shows a DICOM PatientSex code (M/F/O) as a Russian word in tables (муж / жен / др).</summary>
    public sealed class DicomSexToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var code = value?.ToString()?.Trim().ToUpperInvariant();

            return code switch
            {
                "M" or "MAN" => "муж",
                "F" or "FEMALE" => "жен",
                "O" or "OTHER" => "др",
                _ => string.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString()?.Trim().ToLowerInvariant();

            return text switch
            {
                "муж" or "m" or "man" => "M",
                "жен" or "f" or "female" => "F",
                "др" or "o" or "other" => "O",
                _ => Binding.DoNothing
            };
        }
    }
}
