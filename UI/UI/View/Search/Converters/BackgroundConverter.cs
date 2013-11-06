using Sando.DependencyInjection;
using Sando.UI.View;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
namespace Sando.UI.View.Search.Converters
{
    public class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {            
            if (value==null || string.IsNullOrEmpty(value.ToString()))
                return Brushes.MistyRose;
            else
            {
                var view = ServiceLocator.Resolve<SearchViewControl>();
                if (view != null)
                    return view.GetToolBackgroundcolor();
                else
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
