using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Documents;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.View.Search.Converters {
    [ValueConversion(typeof(IHighlightRawInfo), typeof(object))]
    public class HighlightSearchKey : IValueConverter {

         private RunsLine[] AddLineNumber(IHighlightRawInfo infor, RunsLine[] lines)
         {
             var startLine = infor.StartLineNumber;
             int i = 0;
             var offsets = infor.Offsets ?? lines.Select(n => i ++).ToArray();
             var offsetIndex = 0;

             foreach (var line in lines)
             {
                var num = startLine + offsets.ElementAt(offsetIndex ++);
                line.AddRunFromBeginning(CreateRun(num.ToString() + ":\t", FontWeights.Medium));
             }
             return lines;
         }

        private string[] RemoveHeadTailEmptyStrings(IEnumerable<string> lines)
        {
            var list = lines.ToList();
            while (list.Any() && string.IsNullOrWhiteSpace(list.First()))
            {
                list.RemoveAt(0);
            }

            while (list.Any() && string.IsNullOrWhiteSpace(list.Last()))
            {
                list.RemoveAt(list.Count - 1);
            }
            return list.ToArray();
        }

        public Object Convert(Object inforValue, Type targetType, object parameter, CultureInfo culture)
        {
            var emptyLineOffsets = new List<int>();
            var value = ((IHighlightRawInfo)inforValue).Text;
            var span = new Span();
            try
            {
                // return null;
                if (value == null)
                {
                    return null;
                }

                string input = value as string;
                string[] lines = input.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
                lines = RemoveHeadTailEmptyStrings(lines);
                List<string> key_temp = new List<string>();
                var offset = 0;

                foreach (string line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        emptyLineOffsets.Add(offset);
                    }
                    offset++;

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
                                ? CreateRun(item, FontWeights.Bold)
                                    : CreateRun(item, FontWeights.Medium));
                        }
                    }
                    else
                        span.Inlines.Add(CreateRun(line, FontWeights.Medium));
                    span.Inlines.Add(CreateRun(Environment.NewLine, FontWeights.Medium));
                }
                return ClearSpan((IHighlightRawInfo)inforValue, span, emptyLineOffsets);
            }
            catch (Exception e)
            {
                return ClearSpan((IHighlightRawInfo)inforValue, span, emptyLineOffsets);
            }
        }


        private Run CreateRun(String text, FontWeight fontWeight)
        {
            return new Run(text) {FontWeight = fontWeight};
        }

        private bool IsSearchKey(string input, string[] keyset) {
            foreach(string item in keyset) {
                if(input.Contains(item))
                    return true;
            }
            return false;
        }

        private Span ClearSpan(IHighlightRawInfo inforValue, Span span, IEnumerable<int> emptyLineOffsets)
        {
            var runs = RemoveEmptyLines(inforValue, span.Inlines.Cast<Run>(), emptyLineOffsets).ToArray();
            span.Inlines.Clear();
            span.Inlines.AddRange(runs);
            return span;
        }

        private IEnumerable<Run> RemoveEmptyLines(IHighlightRawInfo inforValue, IEnumerable<Run> terms, 
            IEnumerable<int> emptyLineOffsets)
        {
            var runs = new List<Run>();
            var lines = BreakToRunLines(terms).Select(l => l.RemoveEmptyRun()).Where(l => !l.IsEmpty()).ToArray();
            lines = AddEmptyLines(lines.ToList(), emptyLineOffsets).ToArray();

            lines = AddLineNumber(inforValue, RemoveHeadingAndTrailingEmptyLines(AlignIndention(lines, inforValue)));
            foreach (var line in lines)
            {
                runs.AddRange(line.GetRuns());
                runs.Add(CreateRun(Environment.NewLine, FontWeights.Medium));
            }
            return runs.Any() && runs.Last().Text.Equals(Environment.NewLine)
                ? runs.GetRange(0, runs.Count() - 1)
                    : runs;
        }

        private IEnumerable<RunsLine> AddEmptyLines(List<RunsLine> lines, IEnumerable<int> emptyLineOffsets)
        {
            var indexes = emptyLineOffsets.OrderBy(o => o);
            foreach (var index in indexes)
            {
                var line = new RunsLine();
                line.AddRun(new Run(String.Empty));
                lines.Insert(index, line);
            }
            return lines;
        }

        private RunsLine[] RemoveHeadingAndTrailingEmptyLines(IEnumerable<RunsLine> lines)
        {
            var list = lines.ToList();
            while (list.Any() && String.IsNullOrWhiteSpace(list.First().GetLine()))
            {
                list.RemoveAt(0);
            }
            while (list.Any() && String.IsNullOrWhiteSpace(list.Last().GetLine()))
            {
                list.RemoveAt(list.Count - 1);
            }
            return list.ToArray();
        }


        private IEnumerable<RunsLine> AlignIndention(RunsLine[] lines, IHighlightRawInfo info)
        {
            if (lines.Any())
            {
                var lastHead = lines.Last().GetHeadingWhiteSpace();
                var firstHead = lines.First().GetHeadingWhiteSpace();
                if (firstHead.Equals(string.Empty) && !lastHead.Equals(string.Empty))
                {
                    lines.First().AddRunFromBeginning(CreateRun(lastHead, FontWeights.Medium));
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
                if (Environment.NewLine.ToCharArray().Any(c => run.Text.Contains(c)))
                {
                    var parts = run.Text.Split(Environment.NewLine.ToCharArray(), 
                        StringSplitOptions.None);
                    foreach (var part in parts)
                    {
                        if(!string.IsNullOrEmpty(part))
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

   
}
