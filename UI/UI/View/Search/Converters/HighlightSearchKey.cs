using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.IO;
using System.Xml;
using System.Security;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace Sando.UI.View.Search.Converters {
     [ValueConversion(typeof(string), typeof(object))]
    class HighlightSearchKey : IValueConverter {
        public Object Convert(Object value, Type targetType,
                      object parameter, CultureInfo culture) {

            // return null;
             if(value == null) {
                 return null;
             }

             string input = value as string;
             string[] lines = input.Split('\n');

             var span = new Span();
            foreach(string line in lines){

                 if(line.Contains("|~S~|")) {
                     int first = line.IndexOf("|~S~|") + "|~S~|".Length;
                     int last = line.IndexOf("|~E~|");
                     string key = line.Substring(first, last - first);

                     string[] temp = line.Split(new[] { "|~S~|", "|~E~|" }, StringSplitOptions.RemoveEmptyEntries);

                     for(int j = 0; j < temp.Length; j++) {
                         if(temp[j].Contains(key))
                            span.Inlines.Add(new Run(temp[j]) { FontWeight = FontWeights.Bold });
                         else
                             span.Inlines.Add(new Run(temp[j]));
                     }
                 } else
                     span.Inlines.Add(new Run(line));
                 span.Inlines.Add(new Run("\n"));
             }

             return span;

        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
