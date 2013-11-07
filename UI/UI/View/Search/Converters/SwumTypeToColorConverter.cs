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
    public class SwumTypeToColorConverter : IMultiValueConverter
    {
        

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var listBoxItem = values[0];
                var value = (((System.Windows.Controls.ListBoxItem)listBoxItem).Content as Sando.Recommender.SwumQueriesSorter.InternalSwumRecommendedQuey).Type;
                var selected = (bool)values[1];
                if (selected)
                    return GetColorHistory();
                switch (value is SwumRecommnedationType ? (SwumRecommnedationType)value : SwumRecommnedationType.History)
                {
                    case SwumRecommnedationType.History:
                        return GetColorHistory();
                    default:
                        return GetColorNormal();
                }
            }
            catch (Exception e)
            {
                return GetColorNormal();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

  
        private Brush normalColor = null;
        private Brush historyColor = null;

        private Brush GetColorNormal()
        {
            if (normalColor == null)
                normalColor = SearchViewControl.GetNormalTextColor();
            return normalColor;
        }

        private Brush GetColorHistory()
        {
            if (historyColor == null)
                historyColor = SearchViewControl.GetHistoryTextColor();
            return historyColor;
        }

    }
}
