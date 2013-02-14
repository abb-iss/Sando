using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class FileTypeToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }

            string type = (string)value;
            string resourceName = string.Format("../Resources/Code_{0}.png", type.Substring(type.LastIndexOf('.') + 1));
            return new BitmapImage(new Uri(resourceName, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}