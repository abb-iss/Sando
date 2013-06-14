using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public interface IThesaurus
    {
        IEnumerable<String> GetSynonyms(String word);
    }

    public class SESpecificThesaurus : IThesaurus
    {
        private static SESpecificThesaurus instance;
        private SESpecificThesaurus()
        {
            lock (locker)
            {
                orderedWordPairs = new List<KeyValuePair<string, string>>();
                switchedWordPairs = new List<KeyValuePair<string, string>>();
            }
        }
        public static SESpecificThesaurus GetInstance()
        {
            return instance ?? (instance = new SESpecificThesaurus());
        }

        const string filePath = @"Dictionaries\mined_related_unique.csv";
        private List<KeyValuePair<String, String>> orderedWordPairs;
        private List<KeyValuePair<String, String>> switchedWordPairs;
        private readonly object locker = new object();

        public void Initialize()
        {
            lock (locker)
            {
                var lines = File.ReadAllLines(filePath).Select(a => a.Split(';'));
                IEnumerable<string> csv = (from line in lines
                    select (from piece in line select piece).First()).Skip(1);
                var synonyms = csv.Select(element => element.Split(new char[] { ',' })).
                    Select(s => new KeyValuePair<string, string>(s[0].Trim(), s[1].Trim())).
                        ToList();
                this.orderedWordPairs = synonyms.OrderBy(p => p.Key).ToList();
                this.switchedWordPairs = synonyms.Select(p => new KeyValuePair<String,
                    String>(p.Value, p.Key)).OrderBy(p => p.Key).ToList();
            }
        }

        private class KeyComparer : IComparer<KeyValuePair<string, string>>
        {
            public int Compare(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return x.Key.CompareTo(y.Key);
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
                    return GetValuesOfKey(orderedWordPairs, word).Union(GetValuesOfKey(switchedWordPairs, word));
                }
                return Enumerable.Empty<String>();
            }
        }

        private IEnumerable<string> GetValuesOfKey(List<KeyValuePair<String, String>> keyValuePairs, 
            string word)
        {
            var target = new KeyValuePair<String, String>(word, "");
            var comparer = new KeyComparer();
            int endIndex = keyValuePairs.BinarySearch(target, comparer);
            if (endIndex > -1 && endIndex < keyValuePairs.Count)
            {
                int startInex = endIndex;
                for (; comparer.Compare(keyValuePairs.ElementAt(startInex - 1), target) == 0; startInex--) ;
                return keyValuePairs.GetRange(startInex, endIndex - startInex + 1).Select(p => p.Value);
            }
            return Enumerable.Empty<String>();
        }
    }
}
