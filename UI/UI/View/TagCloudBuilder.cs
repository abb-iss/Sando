using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Sando.Core.Tools;

namespace Sando.UI.View
{
    public interface IShapedWord
    {
        String Word { get; }
        int FontSize { get; }
        Brush Color { get; }
    }

    public class TagCloudBuilder
    {
        private readonly IWordCoOccurrenceMatrix matrix;
        private const int MAX_WORD_COUNT = 200;

        // The count of font and color should be same.
        private readonly int[] FONT_POOL = {15, 20, 25, 30, 35};
        private readonly Brush[] colorPool = { Brushes.LightBlue, Brushes.LightSkyBlue, 
            Brushes.Blue, Brushes.Navy, Brushes.MidnightBlue};

        private readonly string rootWord;

        private class WordWithShape : IShapedWord
        {
            public string Word { set; get; }
            public int Count { set; get; }
            public int FontSize { set; get; }
            public Brush Color { get; set; }

            public WordWithShape(String Word, int Count, Brush Color=null, int FontSize = 0)
            {
                this.Word = Word;
                this.Count = Count;
                this.FontSize = FontSize;
                this.Color = Color;
            }
        }

        public TagCloudBuilder(IWordCoOccurrenceMatrix matrix, String rootWord = null)
        {
            this.matrix = matrix;
            this.rootWord = rootWord;
        }
        
        public IShapedWord[] Build()
        {
            var wordsAndCount = rootWord == null ? CollectWordsFromPool() : CollectNeighborWords(rootWord);
            var list = wordsAndCount.Select(p => new WordWithShape(p.Key, p.Value)).ToArray();
            SetWordShape(list);
            return list.Cast<IShapedWord>().OrderBy(w => w.Word).ToArray();
        }

        private Dictionary<String, int> CollectWordsFromPool()
        {
            var wordsAndCounts = matrix.GetAllWordsAndCount().OrderByDescending(p => p.Value).
                TrimIfOverlyLong(MAX_WORD_COUNT * 2).ToList();
            var trivialWords = SpecialWords.NonInformativeWords();
            return SelectWordsAndCount(wordsAndCounts, trivialWords);
        }

        private Dictionary<string, int> SelectWordsAndCount(List<KeyValuePair<string, int>> wordsAndCounts, 
            string[] trivialWords)
        {
            wordsAndCounts = wordsAndCounts.Where(p => !trivialWords.Contains(p.Key)).Select(
                pair => new KeyValuePair<String, int>(TryGetExpandedWord(pair.Key), pair.Value)).
                    OrderByDescending(p => p.Value).ToList();

            for (int i = wordsAndCounts.Count() - 1; i >= 0; i--)
            {
                var pair = wordsAndCounts.ElementAt(i);
                var beforePairs = wordsAndCounts.GetRange(0, i);
                if (trivialWords.Contains(pair.Key.GetStemmedQuery()) ||
                    beforePairs.Any(bp => bp.Key.IsStemSameTo(pair.Key)))
                        wordsAndCounts.RemoveAt(i);
            }
            return wordsAndCounts.TrimIfOverlyLong(MAX_WORD_COUNT).ToDictionary(p => p.Key, p => p.Value);
        }

        private Dictionary<String, int> CollectNeighborWords(String word)
        {
            var trivialWords = SpecialWords.NonInformativeWords();
            var wordsAndCounts = matrix.GetCoOccurredWordsAndCount(word).
                OrderByDescending(p => p.Value).TrimIfOverlyLong
                    (MAX_WORD_COUNT * 2).ToList();
            return SelectWordsAndCount(wordsAndCounts, trivialWords);
        }


        private string TryGetExpandedWord(string word)
        {
            string result;
            var dic = SpecialWords.HyperCommonAcronyms();
            return dic.TryGetValue(word, out result) ? result : word;
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


        private void SetWordShape(WordWithShape[] list)
        {
            list = list.OrderBy(w => w.Count).ToArray();
            var starts = DivideToRanges(list.Count(), FONT_POOL.Count());
            var fontMap = new Dictionary<Predicate<int>, int>();
            var colorMap = new Dictionary<Predicate<int>, Brush>();

            for (int i = 0; i < starts.Count(); i++)
            {
                var start = starts.ElementAt(i);
                var end = i == starts.Count() - 1 ? list.Count() - 1 : 
                    starts.ElementAt(i + 1) - 1;
                fontMap.Add(j => j >= start && j <= end, FONT_POOL.ElementAt(i));
                colorMap.Add(j => j >= start && j <= end, colorPool.ElementAt(i));
            }
            for (int i = 0; i < list.Count(); i++)
            {
                var fontKey = fontMap.Keys.First(k => k.Invoke(i));
                var font = fontMap[fontKey];
                list.ElementAt(i).FontSize = font;

                var colorKey = colorMap.Keys.First(k => k.Invoke(i));
                var color = colorMap[colorKey];
                list.ElementAt(i).Color = color;
            }
        }
    }
}
