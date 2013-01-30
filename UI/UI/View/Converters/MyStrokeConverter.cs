using System;
using System.Globalization;
using System.Windows.Data;

namespace Sando.UI
{
    public class MyStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var isSelected = Boolean.Parse(value.ToString());
            if (isSelected != null)
            {
                if (isSelected)
                {
                    return "2";
                }
                else
                {
                    return "0";
                }
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}