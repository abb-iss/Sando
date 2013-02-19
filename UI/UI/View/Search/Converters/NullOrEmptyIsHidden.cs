using System;
using System.Windows.Data;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class NullOrEmptyIsHidden : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "21";
            return (int)value == 0 ? "21" : "41";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}