using System;
using System.Globalization;
using System.Windows.Data;

namespace FCSModdingUtility
{
    public class BoolToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool)value ? 0 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (int)value == 0;
        }
    }
}

