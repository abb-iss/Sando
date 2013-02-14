using System;
using System.Windows.Data;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(bool?), typeof(bool))]
    public class NullableBoolToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool?)value).HasValue && ((bool?)value).Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}