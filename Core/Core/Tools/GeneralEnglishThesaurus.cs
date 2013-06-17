using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public class GeneralEnglishThesaurus : IThesaurus
    {
        private static IThesaurus instance;
        private const String dictionaryFile = @"Dictionaries\GeneralDictionary.csv";

        public static IThesaurus GetInstance()
        {
            return instance ?? (instance = new GeneralEnglishThesaurus());
        }
        
        private readonly List<KeyValuePair<String,IEnumerable<String>>> synonymLists = 
            new List<KeyValuePair<string, IEnumerable<string>>>(); 
        private readonly object locker = new object();

        public void Initialize()
        {
            lock (locker)
            {
                var lines = File.ReadAllLines(dictionaryFile).Select(a => a.Split(';'));
                List<string> csv = (from line in lines
                    select (from piece in line select piece).
                        First()).ToList();
                foreach (string line in csv)
                {
                    var pair = CreateSynonymEntry(line);
                    synonymLists.Add(pair);
                }
            }
        }

        private KeyValuePair<String, IEnumerable<String>> CreateSynonymEntry(String line)
        {
            var words = line.Split(new char[] {','});
            var key = words.First();
            var value = words.Skip(1).ToList();
            return new KeyValuePair<string, IEnumerable<string>>(key, value);
        }

        public IEnumerable<string> GetSynonyms(string word)
        {
            lock (locker)
            {
                var synonyms = ThesaurusHelper.GetValuesOfKey(synonymLists, word).FirstOrDefault();
                return synonyms ?? Enumerable.Empty<String>();
            }
        }
    }
}
