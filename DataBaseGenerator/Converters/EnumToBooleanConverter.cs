using System;
using System.Globalization;
using System.Windows.Data;

namespace DataBaseGenerator.UI.Wpf.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is true && parameter != null)
                return Enum.Parse(targetType, parameter.ToString());

            return Binding.DoNothing;
        }
    }
}
