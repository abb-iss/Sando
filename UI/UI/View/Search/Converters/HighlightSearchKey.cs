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
                                          span.Inlines.Add(IsSearchKey(item, key)
                                            ? new Run(item) {FontWeight = FontWeights.Bold}
                                                : new Run(item));
                                      }
                                  }
                                  else
                                      span.Inlines.Add(new Run(line));
                                  span.Inlines.Add(new Run("\n"));
                              }
                              return ClearSpan(span);
                          }
                          catch (Exception e)
                          {
                              return ClearSpan(span);
                          }

        }

        private bool IsSearchKey(string input, string[] keyset) {
            foreach(string item in keyset) {
                if(input.Contains(item))
                    return true;
            }
            return false;
        }

         private Span ClearSpan(Span span)
         {
             var runs = RemoveEmptyLines(span.Inlines.Cast<Run>()).ToArray();
             span.Inlines.Clear();
             span.Inlines.AddRange(runs);
             return span;
         }


        private IEnumerable<Run> RemoveEmptyLines(IEnumerable<Run> terms)
        {
            var runs = new List<Run>();
            var lines = BreakToRunLines(terms).Select(l => l.RemoveEmptyRun()).
                Where(l => !l.IsEmpty()).ToArray();
            lines = AlignIndention(lines);


            foreach (var line in lines)
            {
                runs.AddRange(line.GetRuns());
                runs.Add(new Run(Environment.NewLine));
            }
            return runs.Any() && runs.Last().Text.Equals(Environment.NewLine)
                       ? runs.GetRange(0, runs.Count() - 1)
                       : runs;
        }

         private RunsLine[] AlignIndention(RunsLine[] lines)
         {
            if (lines.Any())
            {
                var lastHead = lines.Last().GetHeadingWhiteSpace();
                var firstHead = lines.First().GetHeadingWhiteSpace();
                if (firstHead.Equals(string.Empty) && !lastHead.Equals(string.Empty))
                {
                    lines.First().AddRunFromBeginning(new Run(lastHead));
                }
            }
            return lines;
         }


        private class RunsLine
        {
            private List<Run> runs;
             
            public RunsLine()
            {
                this.runs = new List<Run>();
            }
            
            public void AddRun(Run run)
            {
                this.runs.Add(run);
            }

            public RunsLine RemoveEmptyRun()
            {
                runs = runs.Where(r => !String.IsNullOrWhiteSpace(r.Text)).ToList();
                return this;
            }

            public IEnumerable<Run> GetRuns()
            {
                return runs;
            }

            public bool IsEmpty()
            {
                return !runs.Any();
            }

            public string GetLine()
            {
                var sb = new StringBuilder();
                foreach (var run in runs)
                {
                    sb.Append(run.Text);
                }
                return sb.ToString();
            }

            public string GetHeadingWhiteSpace()
            {
                var line = GetLine();
                var headSpaceLength = line.IndexOf(line.TrimStart(), StringComparison.InvariantCulture);
                return line.Substring(0, headSpaceLength);
            }

            public void AddRunFromBeginning(Run run)
            {
                runs.Insert(0, run);
            }
        }


        private IEnumerable<RunsLine> BreakToRunLines(IEnumerable<Run> runs)
        {
            var lines = new List<RunsLine>();
            var currentLine = new RunsLine();
            foreach (var run in runs)
            {
                if (run.Text.Contains('\n'))
                {
                    var parts = run.Text.Split('\n');
                    foreach (var part in parts)
                    {
                        currentLine.AddRun(CloneFormat(run, part));
                        lines.Add(currentLine);
                        currentLine = new RunsLine();
                    }
                }
                else
                {
                    currentLine.AddRun(run);
                }
            }
            lines.Add(currentLine);
            return lines.ToArray();
        }

        private Run CloneFormat(Run original, string text)
        {
            return new Run(text){AllowDrop = original.AllowDrop, BaselineAlignment = original.BaselineAlignment,
                Background = original.Background, FontSize = original.FontSize, FontFamily = original.FontFamily,
                    Foreground = original.Foreground};
        }

        public object ConvertBack(object value, Type targetType,
                                    object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
