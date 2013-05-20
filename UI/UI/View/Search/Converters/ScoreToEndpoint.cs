using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class ScoreToEndpoint : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return 0;
            try
            {
                var element = (double)value;
                return 1 * element;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}