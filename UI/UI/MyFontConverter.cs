using System;
using System.Globalization;
using System.Windows.Data;

namespace Sando.UI
{
    public class MyFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var isSelected = Boolean.Parse(value.ToString());
            if (isSelected != null)
            {
                if (isSelected)
                {
                    return "Bold";
                }
                else
                {
                    return "Normal";
                }
            }
            return "Normal";
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}