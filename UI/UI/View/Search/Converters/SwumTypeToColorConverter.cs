using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Sando.Recommender;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(SwumRecommnedationType), typeof(Brush))] 
    public class SwumTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c;
            switch (value is SwumRecommnedationType ? (SwumRecommnedationType) value : SwumRecommnedationType.History)
            {
                case SwumRecommnedationType.History:
                    c = Colors.DeepSkyBlue;
                    break;
                default:
                    c = Colors.Black;
                    break;
            }
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
