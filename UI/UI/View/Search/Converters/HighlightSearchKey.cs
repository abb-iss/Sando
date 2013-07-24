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
                          var span = new Span();
                          try
                          {
                              // return null;
                              if (value == null)
                              {
                                  return null;
                              }

                              string input = value as string;
                              string[] lines = input.Split('\n');
                              
                              List<string> key_temp = new List<string>();

                              foreach (string line in lines)
                              {

                                  if (line.Contains("|~S~|"))
                                  {

                                      string findKey = string.Copy(line);

                                      while (findKey.IndexOf("|~S~|") >= 0)
                                      {
                                          int first = findKey.IndexOf("|~S~|") + "|~S~|".Length;
                                          int last = findKey.IndexOf("|~E~|");

                                          string key_candidate = findKey.Substring(first, last - first);

                                          bool removed = false;
                                          if (key_candidate.StartsWith("|~S~|"))
                                          {
                                              removed = true;
                                              key_candidate = key_candidate.Remove("|~S~|".Length);
                                          }


                                          if (!key_temp.Contains(key_candidate))
                                              key_temp.Add(key_candidate);

                                          //Remove the searched string
                                          int lengthRemove = last - first + 2 * "|~S~|".Length;
                                          findKey = findKey.Remove(first - "|~S~|".Length, lengthRemove);
                                          if (removed)
                                              findKey = findKey.Insert(first - "|~S~|".Length, "|~S~|");
                                      }

                                      string[] key = key_temp.ToArray();
                                      string[] temp = line.Split(new[] { "|~S~|", "|~E~|" }, StringSplitOptions.RemoveEmptyEntries);

                                      foreach (string item in temp)
                                      {
                                          if (IsSearchKey(item, key))
                                              span.Inlines.Add(new Run(item) { FontWeight = FontWeights.Bold });
                                          else
                                              span.Inlines.Add(new Run(item));
                                      }
                                  }
                                  else
                                      span.Inlines.Add(new Run(line));
                                  span.Inlines.Add(new Run("\n"));
                              }
                              return span;
                          }
                          catch (Exception e)
                          {
                              return span;
                          }

        }

        private bool IsSearchKey(string input, string[] keyset) {
            foreach(string item in keyset) {
                if(input.Contains(item))
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
