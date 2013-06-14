using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Core.Tools
{
    public enum DictionaryOption
    {
        IncludingStemming,
        NoStemming
    }

    /// <summary>
    /// This class keeps records of used words in the code under searching. Also, it can greedily 
    /// split a given string by matching words in the dictionary. 
    /// </summary>
    public class DictionaryBasedSplitter : IWordSplitter, IDisposable
    {
        private readonly FileDictionary dictionary;
        
        public DictionaryBasedSplitter()
        {
            this.dictionary = new FileDictionary();
        }

        public void Initialize(String directory)
        {
            dictionary.Initialize(directory);
        }

        public void AddWords(IEnumerable<String> words)
        {
            words = words.ToList();
            dictionary.AddWords(words, DictionaryOption.IncludingStemming);
        }
       

        private sealed class FileDictionary : IDisposable
        {
            private const int TERM_MINIMUM_LENGTH = 2;
            const string dictionaryName = "dictionary.txt";
            private string directory;
            private readonly List<string> allWords = new List<string>();
            private WordCorrector corrector = new WordCorrector();
            
            private delegate void NewWordsAdded(IEnumerable<String> words);
            private event NewWordsAdded addWordsEvent;


            public FileDictionary()
            {
                this.corrector = new WordCorrector();
                addWordsEvent += corrector.AddWords;
            }

            public void Initialize(String directory)
            {
                lock (allWords)
                {
                    allWords.Clear();
                    this.directory = directory;
                    ReadWordsFromFile();
                }
            }

            private void WriteWordsToFile()
            {
                using (var writer = new StreamWriter(GetDicFilePath(), false, Encoding.ASCII))
                {        
                    foreach (string word in allWords)
                    {
                        writer.WriteLine(word.Trim());
                    }
                }
            }

            private void ReadWordsFromFile()
            {
                if (File.Exists(GetDicFilePath()))
                {
                    var allLines = File.ReadAllLines(GetDicFilePath());
                    allWords.Clear();
                    allWords.AddRange(allLines);
                    addWordsEvent(allLines);
                }
            }

            private String GetDicFilePath()
            {
                var path = Path.Combine(directory, dictionaryName);
                return path;
            }

            public void AddWords(IEnumerable<String> words, DictionaryOption option)
            {
                var wordsToAdd = words.ToList();

                if (option == DictionaryOption.IncludingStemming)
                {
                    var stemmedWords = wordsToAdd.Select(DictionaryHelper.GetStemmedQuery).ToList();
                    wordsToAdd.AddRange(stemmedWords);
                }

                wordsToAdd = wordsToAdd.Distinct().ToList();

                lock (allWords)
                {
                    foreach (string word in wordsToAdd)
                    {
                        var trimedWord = word.Trim().ToLower();
                        if (!String.IsNullOrEmpty(trimedWord) && trimedWord.Length > 
                            TERM_MINIMUM_LENGTH)
                        {
                            bool found = false;
                            int smallerWordsCount = GetSmallerWordsCount(trimedWord, out found);
                            if (!found)
                                allWords.Insert(smallerWordsCount, trimedWord);
                        }
                    }
                    addWordsEvent(wordsToAdd);
                }
            }

            public Boolean DoesWordExist(String word, DictionaryOption option)
            {
                var trimmedWord = word.Trim().ToLower();
                bool found = false;
                lock (allWords)
                {
                    GetSmallerWordsCount(trimmedWord, out found);
                    if (!found && option == DictionaryOption.IncludingStemming)
                    {
                        var stemmedWord = DictionaryHelper.GetStemmedQuery(trimmedWord);
                        if (!stemmedWord.Equals(word))
                        {
                            GetSmallerWordsCount(stemmedWord, out found);
                        }
                    }
                    return found;
                }
            }

            private int GetSmallerWordsCount(string word, out bool found)
            {
                int min = 0;
                int max = allWords.Count - 1;
                found = false;
               
                // If no words, then return directly.
                if (max == -1)
                {
                    return 0;
                }

                while (!found && min <= max)
                {
                    int current = (min + max)/2;
                    var currentWord = allWords.ElementAt(current);
                    if (word.CompareTo(currentWord) < 0)
                        max = current - 1;
                    else if (word.CompareTo(currentWord) > 0)
                        min = current + 1;
                    else
                        found = true;
                }
                return min;
            }

            public IEnumerable<String> FindSimilarWords(String word)
            {
                var similarWords = corrector.FindSimilarWords(word).ToList();
                if(similarWords.Any())
                    return similarWords.Select(p => p.Key);
                return Enumerable.Empty<string>();
            }

            public void Dispose()
            {
                lock (allWords)
                {
                    if (directory != null && allWords.Any())
                    {
                        WriteWordsToFile();
                        directory = null;
                    }
                }
            }
        }

        public void UpdateProgramElement(ReadOnlyCollection<ProgramElement> elements)
        {
            foreach (ProgramElement element in elements)
            {
                AddWords(DictionaryHelper.ExtractElementWords(element));
            }
        }


        public Boolean DoesWordExist(String word, DictionaryOption option)
        {
            word = word.Trim();   
            return word.Equals(String.Empty) || dictionary.DoesWordExist(word, option);
        }

        private bool IsQuoted(String text)
        {
            text = text.Trim();
            return text.StartsWith("\"") && text.EndsWith("\"");
        }

        public string[] ExtractWords (string text)
        {
            if (IsQuoted(text) || DoesWordExist(text, DictionaryOption.IncludingStemming))
            {
                return new string[]{text};    
            }

            var allSplits = new List<String>();
            var starts = DictionaryHelper.GetQuoteStarts(text).ToList();
            starts.Add(text.Length);
            var ends = DictionaryHelper.GetQuoteEnds(text).ToList();
            ends.Insert(0, -1);

            for (int i = 0; i < starts.Count; i++)
            {
                // Split the non-quotes part.
                int nonQuoteLength = starts.ElementAt(i) - ends.ElementAt(i) - 1;
                if (nonQuoteLength > 0)
                {
                    var nonQuote = text.Substring(ends.ElementAt(i) + 1, nonQuoteLength);
                    allSplits.AddRange(SplitNonQuote(nonQuote));
                }

                // Keep the quotes part.
                if (i != starts.Count - 1)
                {
                    var quote = text.Substring(starts.ElementAt(i), ends.ElementAt(i + 1) -
                        starts.ElementAt(i) + 1);
                    allSplits.Add(quote);
                }
            }
            return allSplits.ToArray();

        }

        private IEnumerable<String> SplitNonQuote(string text)
        {
            var allSplits = new List<String>();
            var allWords = text.Split(null).Select(w => w.ToLower().Trim()).
                Where(s => !String.IsNullOrEmpty(s));
            var strategy = new GreedySplitStrategy();

            foreach (string word in allWords)
            {
                allSplits.AddRange(strategy.SplitWord(word, s => DoesWordExist(s, 
                    DictionaryOption.NoStemming)));
            }
            return allSplits;
        }


        public IEnumerable<string> FindSimilarWords(String word)
        {
            return dictionary.FindSimilarWords(word);
        }


        public void Dispose()
        {
            dictionary.Dispose();
        }


        private interface IWordSplitStrategy
        {
            IEnumerable<string> SplitWord(String word, Predicate<String> doesWordExist);
        }

        private class GreedySplitStrategy : IWordSplitStrategy
        {
            public IEnumerable<string> SplitWord(string word, Predicate<String> doesWordExist)
            {
                var allSubWords = new List<String>();
                int prefixLength, suffixLength;
                string prefix = null;
                string suffix = null;

                // Get the longest prefix.
                for (prefixLength = word.Length; prefixLength > 0; prefixLength--)
                {
                    prefix = word.Substring(0, prefixLength);
                    if (doesWordExist.Invoke(prefix))
                        break;
                }

                // Get the longest suffix.
                for (suffixLength = word.Length - prefixLength; suffixLength > 0; suffixLength--)
                {
                    suffix = word.Substring(word.Length - suffixLength);
                    if(doesWordExist.Invoke(suffix))
                        break;
                }

                String middel = word.Substring(prefixLength, word.Length - prefixLength - suffixLength);
                if (middel.Equals(word))
                    return new []{word};

                if(prefixLength > 0)
                    allSubWords.Add(prefix);
             
                if(middel.Length > 0)
                    allSubWords.AddRange(SplitWord(middel, doesWordExist));

                if(suffixLength > 0)
                    allSubWords.Add(suffix);
             
                return allSubWords;
            }
        }

        private class PerfectSplitStrategy : IWordSplitStrategy
        {
            public IEnumerable<String> SplitWord(String word, Predicate<String> doesWordExist)
            {
                var split = PerfectSplitWordHelper(word, doesWordExist);
                if (!split.Any())
                    split.Add(word);
                return split;
            }


            /// <summary>
            /// Given a word with all letters in the lower case, this method try to split the word to 
            /// meaningful subwords.
            /// </summary>
            /// <param name="word"></param>
            /// <param name="doesWordExist"></param>
            /// <returns></returns>
            private List<string> PerfectSplitWordHelper(string word, Predicate<string> doesWordExist)
            {
                var allSubWords = new List<String>();

                if (doesWordExist.Invoke(word))
                {
                    allSubWords.Add(word);
                    return allSubWords;
                }

                if (word.Length == 1)
                {
                    if (doesWordExist.Invoke(word))
                        allSubWords.Add(word);
                    return allSubWords;
                }

                for (int length = word.Length - 1; length > 0; length--)
                {
                    var subWord1 = word.Substring(0, length);
                    var subWord2 = word.Substring(length);
                    List<String> split1, split2;

                    // Always split the shorter sub word first.
                    if (subWord1.Length > subWord2.Length)
                    {
                        split2 = PerfectSplitWordHelper(subWord2, doesWordExist);
                        if (!split2.Any())
                            continue;
                        split1 = PerfectSplitWordHelper(subWord1, doesWordExist);
                        if (!split1.Any())
                            continue;
                    }
                    else
                    {
                        split1 = PerfectSplitWordHelper(subWord1, doesWordExist);
                        if (!split1.Any())
                            continue;
                        split2 = PerfectSplitWordHelper(subWord2, doesWordExist);
                        if (!split2.Any())
                            continue;
                    }

                    allSubWords.AddRange(split1);
                    allSubWords.AddRange(split2);
                    return allSubWords;
                }
                return allSubWords;
            }
        }
    }
}
