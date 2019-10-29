using System;
using System.Globalization;
using System.Windows.Data;
using FontAwesome5;

namespace FCSModdingUtility
{
    /// <summary>
    /// A converter that takes in a string and returns a <see cref="EFontAwesomeIcon"/>
    /// </summary>
    public class StringToFontAwesomeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EFontAwesomeIcon icon = EFontAwesomeIcon.None;
            if (value != null)
            {
                icon = (EFontAwesomeIcon)Enum.Parse(typeof(EFontAwesomeIcon), (string)value);
            }

            return icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
