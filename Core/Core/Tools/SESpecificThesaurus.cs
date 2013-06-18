using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public interface IThesaurus
    {
        void Initialize();
        IEnumerable<String> GetSynonyms(String word);
    }

    public class ThesaurusHelper
    {
        public static IEnumerable<T> GetValuesOfKey<T>(List<KeyValuePair<String, T>> keyValuePairs,
          string word)
        {
            var target = new KeyValuePair<String, T>(word, default(T));
            var comparer = new KeyComparer<T>();
            int endIndex = keyValuePairs.BinarySearch(target, comparer);
            if (endIndex > -1 && endIndex < keyValuePairs.Count)
            {
                int startInex = endIndex;
                for (; comparer.Compare(keyValuePairs.ElementAt(startInex - 1), target) == 0; startInex--);
                return keyValuePairs.GetRange(startInex, endIndex - startInex + 1).Select(p => p.Value);
            }
            return Enumerable.Empty<T>();
        }

        private class KeyComparer<T> : IComparer<KeyValuePair<string, T>>
        {
            public int Compare(KeyValuePair<string, T> x, KeyValuePair<string, T> y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }
    }


    public class SeSpecificThesaurus : IThesaurus
    {
        private static SeSpecificThesaurus instance;
        private SeSpecificThesaurus()
        {
            lock (locker)
            {
                orderedWordPairs = new List<KeyValuePair<string, string>>();
                switchedWordPairs = new List<KeyValuePair<string, string>>();
                this.isInitialized = false;
            }
        }
        public static IThesaurus GetInstance()
        {
            return instance ?? (instance = new SeSpecificThesaurus());
        }

        const string filePath = @"Dictionaries\mined_related_unique.csv";
        private List<KeyValuePair<String, String>> orderedWordPairs;
        private List<KeyValuePair<String, String>> switchedWordPairs;
        private readonly object locker = new object();
        private bool isInitialized;

        public void Initialize()
        {
            lock (locker)
            {
                if (!isInitialized)
                {
                    var lines = File.ReadAllLines(filePath).Select(a => a.Split(';'));
                    IEnumerable<string> csv = (from line in lines
                                               select (from piece in line select piece).First()).Skip(1);
                    var synonyms = csv.Select(element => element.Split(new char[] {','})).
                                       Select(s => new KeyValuePair<string, string>(s[0].Trim(), s[1].Trim())).
                                       ToList();
                    this.orderedWordPairs = synonyms.OrderBy(p => p.Key).ToList();
                    this.switchedWordPairs = synonyms.Select(p => new KeyValuePair<String,
                                                                      String>(p.Value, p.Key))
                                                     .OrderBy(p => p.Key)
                                                     .ToList();
                    this.isInitialized = true;
                }
            }
        }

        private String Preprocess(String word)
        {
            return word.Trim().ToLower();
        }

        public IEnumerable<String> GetSynonyms(String word)
        {
            lock (locker)
            {
                if (!String.IsNullOrEmpty(word))
                {
                    word = Preprocess(word);
                    return ThesaurusHelper.GetValuesOfKey(orderedWordPairs, word).
                        Union(ThesaurusHelper.GetValuesOfKey(switchedWordPairs, word));
                }
                return Enumerable.Empty<String>();
            }
        }
    }
}
