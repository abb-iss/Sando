using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public partial class DictionaryBasedSplitter
    {
        private sealed class FileDictionary : IDisposable
        {
            private const int TERM_MINIMUM_LENGTH = 2;
            private const string dictionaryName = "dictionary.txt";
            private const string stemDictionary = "stemDictionary.txt";
            private string directory;
            
            private readonly object locker = new object();
            private readonly List<string> originalWords = new List<string>();
            private readonly List<string> stemmedWords = new List<string>();
            private readonly WordCorrector corrector = new WordCorrector();

            public event NewWordsAdded addWordsEvent;

            public FileDictionary()
            {
                this.corrector = new WordCorrector();
                addWordsEvent += corrector.AddWords;
            }

            public void Initialize(String directory)
            {
                lock (locker)
                {
                    this.directory = directory;
                    originalWords.Clear();
                    stemmedWords.Clear();
                    ReadWordsFromFile(GetDicFilePath(), originalWords);
                    ReadWordsFromFile(GetStemDicPath(), stemmedWords);
                }
            }

            public void Dispose()
            {
                lock (locker)
                {
                    if (directory != null && originalWords.Any())
                    {
                        WriteWordsToFile(GetDicFilePath(), originalWords);
                        WriteWordsToFile(GetStemDicPath(), stemmedWords);
                        directory = null;
                    }
                }
            }

            private void WriteWordsToFile(String path, IEnumerable<String> wordsToWrite)
            {
                using (var writer = new StreamWriter(path, false, Encoding.ASCII))
                {
                    foreach (string word in wordsToWrite)
                    {
                        writer.WriteLine(word.Trim());
                    }
                }
            }

            private void ReadWordsFromFile(String path, List<String> wordList)
            {
                if (File.Exists(path))
                {
                    var allLines = File.ReadAllLines(GetDicFilePath());
                    wordList.Clear();
                    wordList.AddRange(allLines);
                    addWordsEvent(wordList);
                }
            }

            private String GetDicFilePath()
            {
                var path = Path.Combine(directory, dictionaryName);
                return path;
            }

            private String GetStemDicPath()
            {
                return Path.Combine(directory, stemDictionary);
            }

            private IEnumerable<String> AddWordsToList(List<String> wordsPool, IEnumerable<String> wordsToAdd)
            {
                var addedWords = new List<String>();
                foreach (string word in wordsToAdd)
                {
                    var found = false;
                    var smallerWordsCount = GetSmallerWordsCount(wordsPool, word, out found);
                    if (!found)
                    {
                        wordsPool.Insert(smallerWordsCount, word);
                        addedWords.Add(word);
                    }
                }
                return addedWords;
            }


            public void AddWords(IEnumerable<String> words, DictionaryOption option)
            {
                var addedWords = new List<String>();
                var originalWordsToAdd = SelectingWordsAddToDictionary(words).ToList();
                var stemmedWordsToAdd = new List<String>();
                if (option == DictionaryOption.IncludingStemming)
                {
                    var stems = originalWordsToAdd.Select(DictionaryHelper.GetStemmedQuery);
                    stemmedWordsToAdd.AddRange(stems);
                }

                lock (locker)
                {
                    addedWords.AddRange(AddWordsToList(originalWords, originalWordsToAdd));
                    addedWords.AddRange(AddWordsToList(stemmedWords, stemmedWordsToAdd));
                }

                addWordsEvent(addedWords);
            }

            private IEnumerable<String> SelectingWordsAddToDictionary(IEnumerable<String> words)
            {
                return words.Select(w => w.Trim().ToLower()).Distinct().Where(w => !String.IsNullOrEmpty(w)
                    && w.Length > TERM_MINIMUM_LENGTH);
            }


            public Boolean DoesWordExist(String word, DictionaryOption option)
            {
                var trimmedWord = word.Trim().ToLower();
                bool found = false;
                lock (locker)
                {
                    GetSmallerWordsCount(originalWords, trimmedWord, out found);
                    if (!found && option == DictionaryOption.IncludingStemming)
                    {
                        var stemmedWord = trimmedWord.GetStemmedQuery();
                        if (!stemmedWord.Equals(word))
                        {
                            GetSmallerWordsCount(stemmedWords, stemmedWord, out found);
                        }
                    }
                    return found;
                }
            }

            private int GetSmallerWordsCount(List<String> list, string word, out bool found)
            {
                int min = 0;
                int max = list.Count - 1;
                found = false;

                // If no words, then return directly.
                if (max == -1)
                {
                    return 0;
                }

                while (!found && min <= max)
                {
                    int current = (min + max) / 2;
                    var currentWord = list.ElementAt(current);
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
                return corrector.FindSimilarWords(word).ToList();
            }
        }
    }
}
