using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Sando.ExtensionContracts.ProgramElementContracts;
using LocalSearch;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(ProgramElement), typeof(String))]
    [ValueConversion(typeof(CodeNavigationResult), typeof(String))]
    public class ProgramElementToRelationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var element = value as CodeNavigationResult;
            if (element == null)
                return "";
            else
                return element.RelationCodeAsString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
