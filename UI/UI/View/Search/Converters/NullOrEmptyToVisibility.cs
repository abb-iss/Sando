using System;
using System.Windows.Data;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class NullOrEmptyToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "Hidden";
            return (int)value == 0 ? "Hidden" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}