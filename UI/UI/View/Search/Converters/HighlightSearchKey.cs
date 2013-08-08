using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Sando.Core.Tools;
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

             int num = 0, preNum = 0;
             foreach (var line in lines)
             {
                 if (offsets.Count() > offsetIndex)
                 {
                     num = startLine + offsets.ElementAt(offsetIndex++);
                     preNum = num;
                 }
                 else
                 {
                     num = preNum + 1;
                     preNum = num;
                 }
                line.AddBeginning(CreateRun("\t", regularWeight));
                line.AddBeginning(CreateLineNumberHyperLink(num));
             }
             return lines;
         }

        private Inline CreateLineNumberHyperLink(int number)
        {
            var run = CreateRun(number.ToString(), regularWeight);
            run.Foreground = Brushes.CadetBlue;
            return run;
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


        private string RemoveHeadAndTailNewLine(string text)
        {
            while (text.Any() && (text.First().Equals('\r') || text.First().Equals('\n')))
            {
                text = text.Substring(1);
            }
            while (text.Any() && (text.Last().Equals('\r') || text.Last().Equals('\n')))
            {
                text = text.Substring(0, text.Length - 1);
            }
            return text;
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

                var input = value as string;
                string[] lines = input.SplitToLines().Select(RemoveHeadAndTailNewLine).ToArray();
                lines = RemoveHeadTailEmptyStrings(lines);
                var keyTemp = new List<string>();
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


                            if (!keyTemp.Contains(key_candidate))
                                keyTemp.Add(key_candidate);

                            //Remove the searched string
                            int lengthRemove = last - first + 2 * "|~S~|".Length;
                            findKey = findKey.Remove(first - "|~S~|".Length, lengthRemove);
                            if (removed)
                                findKey = findKey.Insert(first - "|~S~|".Length, "|~S~|");
                        }

                        string[] key = keyTemp.ToArray();
                        string[] temp = line.Split(new[] { "|~S~|", "|~E~|" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string item in temp)
                        {
                            span.Inlines.Add(IsSearchKey(item, key)
                                ? CreateRun(item, highlightedWeight, SearchViewControl.GetHistoryTextColor())
                                    : CreateRun(item, regularWeight));
                        }
                    }
                    else
                        span.Inlines.Add(CreateRun(line, regularWeight));
                    span.Inlines.Add(CreateRun(Environment.NewLine, regularWeight));
                }
                return ClearSpan((IHighlightRawInfo)inforValue, span, emptyLineOffsets);
            }
            catch (Exception e)
            {
                return span;
            }
        }

        FontWeight highlightedWeight = FontWeights.UltraBold;
        FontWeight regularWeight = FontWeights.Medium;

        private Run CreateRun(String text, FontWeight fontWeight, Brush color = null)
        {
            var brush = GetForeground();
            if (color != null)
                brush = color;            
            return new Run(text) { FontWeight = fontWeight, FontFamily = new FontFamily("Courier"), Foreground = brush};
        }

        private static Brush GetForeground()
        {
            var key = Microsoft.VisualStudio.Shell.VsBrushes.ToolWindowTextKey;
            var brush = (Brush) Application.Current.Resources[key];
            return brush;
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

            var lines = BreakToRunLines(terms).Select(l => l.RemoveEmptyRun()).ToArray();
            lines = lines.Where(l => !l.IsEmpty()).ToArray();
            lines = AddEmptyLines(lines.ToList(), emptyLineOffsets).ToArray();

            if (inforValue.IndOption == IndentionOption.NoIndention)
            {
                lines = lines.Select(l => l.RemoveHeadingWhiteSpace()).ToArray();
            }

            lines = AddLineNumber(inforValue, RemoveHeadingAndTrailingEmptyLines(AlignIndention(lines, inforValue)));
            foreach (var line in lines)
            {
                items.AddRange(line.GetItems());
                items.Add(CreateRun(Environment.NewLine, regularWeight));
            }
            return items;
        }

        private IEnumerable<InlineItemLine> AddEmptyLines(List<InlineItemLine> lines, IEnumerable<int> emptyLineOffsets)
        {
            var indexes = emptyLineOffsets.OrderBy(o => o);
            foreach (var index in indexes)
            {
                var line = new InlineItemLine();
                line.AddItem(CreateRun(String.Empty, regularWeight));
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
                    lines.First().AddBeginning(CreateRun(lastHead, regularWeight));
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

            public InlineItemLine RemoveHeadingWhiteSpace()
            {
                var newLine = new InlineItemLine();
                var startIndex = items.FindIndex(i => !String.IsNullOrWhiteSpace(GetItemText(i)));
                if (startIndex < items.Count)
                {
                    newLine.items.AddRange(items.GetRange(startIndex, items.Count - startIndex));
                    var first = newLine.items.First();
                    newLine.items.RemoveAt(0);
                    newLine.items.Insert(0, CloneFormat((Run)first, GetItemText(first).TrimStart()));
                }
                return newLine;
            }
        }

        private IEnumerable<InlineItemLine> BreakToRunLines(IEnumerable<Run> runs)
        {
            var lines = new List<InlineItemLine>();
            var currentLine = new InlineItemLine();
            foreach (var run in runs)
            {
                if (run.Text.Contains(Environment.NewLine))
                {
                    var parts = run.Text.SplitToLines();
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

        private static Run CloneFormat(Run original, string text)
        {
            return new Run(text){AllowDrop = original.AllowDrop, BaselineAlignment = original.BaselineAlignment,
                Background = original.Background, FontSize = original.FontSize, FontFamily = original.FontFamily,
                    Foreground = GetForeground()};
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}