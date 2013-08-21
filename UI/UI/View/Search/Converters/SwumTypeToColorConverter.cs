using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Resources;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using Sando.Recommender;
using Sando.DependencyInjection;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(SwumRecommnedationType), typeof(Brush))] 
    public class SwumTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush c;
            switch (value is SwumRecommnedationType ? (SwumRecommnedationType) value : SwumRecommnedationType.History)
            {
                case SwumRecommnedationType.History:
                    c = GetColorHistory();
                    break;
                default:
                    c = GetColorNormal();
                    break;
            }
            return c;
        }

        private Brush normalColor = null;
        private Brush historyColor = null;

        private Brush GetColorNormal()
        {
            if(normalColor == null)
                normalColor = ServiceLocator.Resolve<SearchViewControl>().GetNormalTextColor();
            return normalColor;
        }

        private Brush GetColorHistory()
        {
            if (historyColor == null)
                historyColor = ServiceLocator.Resolve<SearchViewControl>().GetHistoryTextColor();
            return historyColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
