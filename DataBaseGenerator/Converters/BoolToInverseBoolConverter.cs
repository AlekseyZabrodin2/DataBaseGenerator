using System;
using System.Globalization;
using System.Windows.Data;

namespace DataBaseGenerator.UI.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToInverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InvertBool(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return InvertBool(value);
        }


        private bool InvertBool(object value)
        {
            return !(value != null && (bool)value);
        }
    }
}
