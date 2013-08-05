using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Sando.ExtensionContracts.ResultsReordererContracts;


namespace Sando.UI.View.Search.Converters {
    [ValueConversion(typeof(IHighlightRawInfo), typeof(object))]
    public class HighlightSearchKey : IValueConverter {

         private InlineItemLine[] AddLineNumber(IHighlightRawInfo infor, InlineItemLine[] lines)
         {
             var startLine = infor.StartLineNumber;
             int i = 0;
             var offsets = infor.Offsets ?? lines.Select(n => i ++).ToArray();
             var offsetIndex = 0;

             foreach (var line in lines)
             {
                var num = startLine + offsets.ElementAt(offsetIndex ++);
                line.AddBeginning(CreateRun("\t", FontWeights.Medium));
                line.AddBeginning(CreateLineNumberHyperLink(num));
             }
             return lines;
         }

        private Inline CreateLineNumberHyperLink(int number)
        {
        /*    var link = new Hyperlink(CreateRun(number.ToString(), 
                FontWeights.Medium)) {Foreground = Brushes.CadetBlue};
            link.Click += ClickLineNumber;*/
            var run = CreateRun(number.ToString(), FontWeights.Medium);
            run.Foreground = Brushes.CadetBlue;
            return run;
        }

        private void ClickLineNumber(object sender, RoutedEventArgs routedEventArgs)
        {
            
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
                var run = CreateRun(((IHighlightRawInfo) inforValue).Text, FontWeights.Medium);
                return new Span(run);
            }
        }


        private Run CreateRun(String text, FontWeight fontWeight)
        {
            return new Run(text) { FontWeight = fontWeight, FontFamily = new FontFamily("Courier")};
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

        private IEnumerable<Inline> RemoveEmptyLines(IHighlightRawInfo inforValue, IEnumerable<Run> terms, 
            IEnumerable<int> emptyLineOffsets)
        {
            var items = new List<Inline>();
            var lines = BreakToRunLines(terms).Select(l => l.RemoveEmptyRun()).Where(l => !l.IsEmpty()).ToArray();
            lines = AddEmptyLines(lines.ToList(), emptyLineOffsets).ToArray();
            lines = AddLineNumber(inforValue, RemoveHeadingAndTrailingEmptyLines(AlignIndention(lines, inforValue)));
            foreach (var line in lines)
            {
                items.AddRange(line.GetItems());
                items.Add(CreateRun(Environment.NewLine, FontWeights.Medium));
            }
            return items;
        }

        private IEnumerable<InlineItemLine> AddEmptyLines(List<InlineItemLine> lines, IEnumerable<int> emptyLineOffsets)
        {
            var indexes = emptyLineOffsets.OrderBy(o => o);
            foreach (var index in indexes)
            {
                var line = new InlineItemLine();
                line.AddItem(new Run(String.Empty));
                lines.Insert(index, line);
            }
            return lines;
        }

        private InlineItemLine[] RemoveHeadingAndTrailingEmptyLines(IEnumerable<InlineItemLine> lines)
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


        private IEnumerable<InlineItemLine> AlignIndention(InlineItemLine[] lines, IHighlightRawInfo info)
        {
            if (lines.Any())
            {
                var lastHead = lines.Last().GetHeadingWhiteSpace();
                var firstHead = lines.First().GetHeadingWhiteSpace();
                if (firstHead.Equals(string.Empty) && !lastHead.Equals(string.Empty))
                {
                    lines.First().AddBeginning(CreateRun(lastHead, FontWeights.Medium));
                }
            }
            return lines;
        }


        private class InlineItemLine
        {
            private List<Inline> items;
             
            public InlineItemLine()
            {
                this.items = new List<Inline>();
            }

            public void AddItem(Inline run)
            {
                this.items.Add(run);
            }

            public InlineItemLine RemoveEmptyRun()
            {
                items = items.Where(r => !String.IsNullOrWhiteSpace(GetItemText(r))).ToList();
                return this;
            }

            public IEnumerable<Inline> GetItems()
            {
                return items;
            }

            public bool IsEmpty()
            {
                return !items.Any();
            }

            private string GetItemText(Inline item)
            {
                return item is Run ? (item as Run).Text : string.Empty;
            }

            public string GetLine()
            {
                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    sb.Append(GetItemText(item));
                }
                return sb.ToString();
            }

            public string GetHeadingWhiteSpace()
            {
                var line = GetLine();
                var headSpaceLength = line.IndexOf(line.TrimStart(), StringComparison.InvariantCulture);
                return line.Substring(0, headSpaceLength);
            }

            public void AddBeginning(Inline item)
            {
                items.Insert(0, item);
            }
        }

        private IEnumerable<InlineItemLine> BreakToRunLines(IEnumerable<Run> runs)
        {
            var lines = new List<InlineItemLine>();
            var currentLine = new InlineItemLine();
            foreach (var run in runs)
            {
                if (Environment.NewLine.ToCharArray().Any(c => run.Text.Contains(c)))
                {
                    var parts = run.Text.Split(Environment.NewLine.ToCharArray(), 
                        StringSplitOptions.None);
                    foreach (var part in parts)
                    {
                        if(!string.IsNullOrEmpty(part))
                            currentLine.AddItem(CloneFormat(run, part));
                        lines.Add(currentLine);
                        currentLine = new InlineItemLine();
                    }
                }
                else
                {
                    currentLine.AddItem(run);
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
