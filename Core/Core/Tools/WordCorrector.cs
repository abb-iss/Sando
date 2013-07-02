using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public class WordCorrector
    {
        public const int GramNumber = 2;
        private readonly Dictionary<String, List<String>> indexedWords;
        private readonly object locker = new object();

        public WordCorrector()
        {
            indexedWords = new Dictionary<string, List<string>>();
        }

        public void AddWords(IEnumerable<String> words)
        {
            lock (locker)
            {
                foreach (var word in words)
                {
                    AddWord(word);
                }
            }
        }

        public IEnumerable<string> FindSimilarWords(String word)
        {
            word = word.ToLower();
            lock (locker)
            {
                var results = new Dictionary<String, int>();
                var grams = GetNGrams(word);
                foreach (var gra in grams)
                {
                    if (indexedWords.ContainsKey(gra))
                    {
                        var correctWords = indexedWords[gra];
                        foreach (var cw in correctWords)
                        {
                            if (!results.ContainsKey(cw))
                                results[cw] = 0;
                            results[cw]++;
                        }
                    }
                }
                return RankSimilarWords(results, word);
            }
        }

        private String[] RankSimilarWords(Dictionary<String, int> results, string originalWord)
        {
            var de = new Levenshtein();
            var correctionWords = results.OrderByDescending(r => r.Value).Select(r => r.Key).TrimIfOverlyLong(10);
            return correctionWords.OrderBy(w => de.LD(w, originalWord)).ToArray();
        }

        private void AddWord(String word)
        {
            var keys = GetNGrams(word);
            foreach (var key in keys)
            {
                if(!indexedWords.ContainsKey(key))
                    indexedWords.Add(key, new List<string>());
                if(indexedWords[key].Contains(word))
                    return;
                indexedWords[key].Add(word);
            }
        }

        private IEnumerable<string> GetNGrams(String word)
        {
            var list = new List<String>();
            for (int start = 0; start <= word.Length - GramNumber; start ++ )
            {
                var gram = word.Substring(start, GramNumber); 
                if(!list.Contains(gram))
                    list.Add(gram);
            }
            return list;
        }
    }
}
