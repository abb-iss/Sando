using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public interface IShapedWord
    {
        String Word { get; }
        int FontSize { get; }
    }

    public class TagCloudBuilder
    {
        private readonly IWordCoOccurrenceMatrix matrix;
        private const int MAX_WORD_COUNT = 200;
        private readonly int[] FONT_POOL = {10, 15, 20, 25, 30};

        private class WordWithShape : IShapedWord
        {
            public string Word { set; get; }
            public int Count { set; get; }
            public int FontSize { set; get; }

            public WordWithShape(String Word, int Count, int FontSize = 0)
            {
                this.Word = Word;
                this.Count = Count;
                this.FontSize = FontSize;
            }
        }

        public TagCloudBuilder(IWordCoOccurrenceMatrix matrix)
        {
            this.matrix = matrix;
        }
        
        public IShapedWord[] Build()
        {
            var wordsAndCount = matrix.GetAllWordsAndCount().Where(p => !SpecialWords.
                NonInformativeWords().Contains(p.Key));
            var list = wordsAndCount.OrderByDescending(p => p.Value).TrimIfOverlyLong
                (MAX_WORD_COUNT).Select(p => new WordWithShape(p.Key, p.Value)).ToArray();
            SetWordFont(list);
            return list.Cast<IShapedWord>().OrderBy(w => w.Word).ToArray();
        }

        private int[] DivideToRanges(int totalLength, int area)
        {
            int areaLength = totalLength/area;
            var list = new List<int>();
            int start = 0;
            for (int i = 0; i < area; i++)
            {
                list.Add(start);
                start += areaLength;
            }
            return list.ToArray();
        }


        private void SetWordFont(WordWithShape[] list)
        {
            list = list.OrderBy(w => w.Count).ToArray();
            var starts = DivideToRanges(list.Count(), FONT_POOL.Count());
            var map = new Dictionary<Predicate<int>, int>();

            for (int i = 0; i < starts.Count(); i++)
            {
                var start = starts.ElementAt(i);
                var end = i == starts.Count() - 1 ? list.Count() - 1 : starts.ElementAt(i + 1) - 1;
                map.Add(j => j >= start && j <= end, FONT_POOL.ElementAt(i));
            }
            for (int i = 0; i < list.Count(); i++)
            {
                var key = map.Keys.First(k => k.Invoke(i));
                var value = map[key];
                list.ElementAt(i).FontSize = value;
            }
        }
    }
}
