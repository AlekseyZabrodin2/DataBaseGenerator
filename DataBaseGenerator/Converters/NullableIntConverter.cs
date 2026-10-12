using System;
using System.Globalization;
using System.Windows.Data;

namespace DataBaseGenerator.UI.Wpf.Converters
{
    public sealed class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return null;

            return int.TryParse(text, NumberStyles.Integer, culture, out var number)
                ? number
                : Binding.DoNothing;
        }
    }
}
