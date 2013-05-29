using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Recommender
{
    public class DictionaryBasedSplitter : IWordSplitter
    {
        private static DictionaryBasedSplitter instance;   
      
        public static DictionaryBasedSplitter GetInstance()
        {
            return instance ?? (instance = new DictionaryBasedSplitter());
        }

        private readonly FileDictionary dictionary = new FileDictionary();
        private readonly Regex _patternChars = new Regex(@"([A-Z][a-z]+)", RegexOptions.Compiled);
        private readonly Regex _patternCharsLowerCase = new Regex(@"([^a-zA-Z][a-z]+)", RegexOptions.Compiled);
        

        private DictionaryBasedSplitter(){}



        public void Initialize(String directory)
        {
            dictionary.Initialize(directory);
        }

        public void AddWords(IEnumerable<String> words)
        {
            if (dictionary != null)
            {
                lock (dictionary)
                {
                    dictionary.AddWords(words);
                }
            }
        }

        private sealed class FileDictionary
        {
            const string dictionaryName = "dictionary.txt";
            private string directory;
            private readonly List<String> allWords = new List<string>();
            
            public void Initialize(String directory)
            {
                WriteWordsToFile();
                this.directory = directory;
                ReadWordsFromFile();
            }

            private void WriteWordsToFile()
            {
                if (directory != null)
                {
                    using (var writer = new StreamWriter(GetDicFilePath(), false, Encoding.ASCII))
                    {
                        foreach (string word in allWords)
                        {
                            writer.WriteLine(word.Trim());
                        }
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
                }
            }

       
            ~FileDictionary()
            {
                WriteWordsToFile();
            }

            private String GetDicFilePath()
            {
                var path = Path.Combine(directory, dictionaryName);
                return path;
            }


            private static IEnumerable<String> GetCSharpKeyWords()
            {
                const string pull = @"abstract event new struct as explicit null switch
                base extern object this bool false operator	throw
                break finally out true byte	fixed override try case	float params typeof
                catch for private uint char	foreach	protected ulong checked	goto public	unchecked
                class if readonly unsafe const implicit	ref	ushort continue	in return using
                decimal	int	sbyte virtual default interface	sealed volatile delegate internal short	void
                do is sizeof while double lock stackalloc else long	static enum	namespace string";
                return pull.Split(null);
            }

            public void AddWords(IEnumerable<String> words)
            {
                foreach (string word in words)
                {
                    var trimedWord = word.Trim().ToLower();
                    if (!String.IsNullOrEmpty(trimedWord))
                    {
                        bool found;
                        int smallerWordsCount = GetSmallerWordCount(trimedWord, out found);
                        if (!found)
                            allWords.Insert(smallerWordsCount, trimedWord);
                    }
                }
            }

            public Boolean DoesWordExist(String word)
            {
                bool found;
                GetSmallerWordCount(word.ToLower(), out found);
                return found;
            }

            private int GetSmallerWordCount(string word, out bool found)
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

          
        }

        public void UpdateProgramElement(ReadOnlyCollection<ProgramElement> elements)
        {
            foreach (ProgramElement element in elements)
            {
                if (element as MethodElement != null)
                {
                    AddWords(ExtractMethodWords(element as MethodElement));
                    continue;
                }

                if (element as XmlXElement != null)
                {
                    AddWords(ExtractXmlWords(element as XmlXElement));
                    continue;
                }
            }
        }


        public Boolean DoesWordExist(String word)
        {
            word = word.Trim();
            if (dictionary != null)
            {
                lock (dictionary)
                {
                    return word.Equals(String.Empty) || dictionary.DoesWordExist(word);
                }
            }
            return true;
        }


        private IEnumerable<String> ExtractXmlWords(XmlXElement element)
        {
            var names = new List<String>();
            return names.AsReadOnly();
        }

        private IEnumerable<String> ExtractMethodWords(MethodElement element)
        {
            var words = new List<String>();
            words.AddRange(GetMatchedWords(_patternChars, element.RawSource));
            words.AddRange(GetMatchedWords(_patternCharsLowerCase, element.RawSource).
                Select(s => s.Substring(1)));
            return words;
        }

        private IEnumerable<string> GetMatchedWords(Regex pattern, String code)
        {
            var matches = pattern.Matches(code);
            return matches.Cast<Match>().Select(m => m.Groups[0].Value);
        }
 
       

        public string[] ExtractWords(string text)
        {
            var allSplits = new List<String>();
            var allWords = text.Split(null).Select(w => w.ToLower());
            var strategy = new GreadySplitStrategy();

            foreach (string word in allWords)
            {
                allSplits.AddRange(strategy.SplitWord(word, s => dictionary.DoesWordExist(s)));
            }
            return allSplits.ToArray();
        }

        private interface IWordSplitStrategy
        {
            IEnumerable<string> SplitWord(String word, Predicate<String> doesWordExist);
        }

        private class GreadySplitStrategy : IWordSplitStrategy
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

                if(prefixLength > 0)
                    allSubWords.Add(prefix);

                String middel = word.Substring(prefixLength, word.Length - prefixLength - suffixLength);
                
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
