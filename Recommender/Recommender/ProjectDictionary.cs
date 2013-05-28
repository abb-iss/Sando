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
    public class ProjectDictionary : IWordSplitter
    {
        private static ProjectDictionary instance;   
      
        public static ProjectDictionary GetInstance()
        {
            return instance ?? (instance = new ProjectDictionary());
        }

        private String projectName;
        private FileDictionary dictionary;
        private const String directory = @"C:\Users\xige\Desktop\Dictionary\";
        private readonly Regex _patternChars = new Regex(@"([A-Z][a-z]+)", RegexOptions.Compiled);
        private readonly Regex _patternCharsLowerCase = new Regex(@"([^a-zA-Z][a-z]+)", RegexOptions.Compiled);
        

        private ProjectDictionary()
        {

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

        private sealed class FileDictionary : IDisposable
        {
            private const int WORD_CELL_LENGTH = 40;
            private const int DICTIONARY_FILE_COUNT = 60;
            private readonly string dictionaryDirectory;
            private static readonly IEnumerable<String> keyWords = GetCSharpKeyWords();
            private readonly Dictionary<String, FileStream> fileStreams;

            public FileDictionary(String dictionaryDirectory)
            {
                this.dictionaryDirectory = dictionaryDirectory;
                this.fileStreams = new Dictionary<string, FileStream>();

                if (!Directory.Exists(dictionaryDirectory))
                {
                    Directory.CreateDirectory(dictionaryDirectory);
                }

                AddWords(keyWords);
            }


            private String GetDicFilePath(String word)
            {
                var path = this.dictionaryDirectory + @"\" + HashWord(word) + ".txt";
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                return path;
            }


            private String HashWord(String word)
            {
                return Math.Abs(word.GetHashCode() % DICTIONARY_FILE_COUNT).ToString();
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
                    var trimedWord = word.Trim();
                    if (trimedWord.Length > 0 && trimedWord.Length <= WORD_CELL_LENGTH)
                    {
                        bool found;
                        string dicFilePath = GetDicFilePath(trimedWord);
                        int smallerWordsCount = GetSmallerWordCount(dicFilePath, trimedWord, out found);
                        if (!found)
                        {
                            int offset = smallerWordsCount*WORD_CELL_LENGTH;
                            InsertTextInFile(dicFilePath, ComplementWord(trimedWord), offset);
                        }
                    }
                }
            }

            private String ComplementWord(String word)
            {
                var sb = new StringBuilder();
                sb.Append(word);
                sb.Append(' ', WORD_CELL_LENGTH - word.Length);
                return sb.ToString();
            }

            private void InsertTextInFile(string dicFilePath, string text, int position)
            {
                var stream = GetFileStream(dicFilePath);
                byte[] allBytes = ReadAllBytes(stream);
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(allBytes, 0, position);
                stream.Write(Encoding.ASCII.GetBytes(text), 0, text.Length);
                stream.Write(allBytes, position, allBytes.Count() - position);
            }

            public Boolean DoesWordExist(String word)
            {
                bool found;
                GetSmallerWordCount(GetDicFilePath(word), word, out found);
                return found;
            }

            private int GetSmallerWordCount(String dicFilePath, string word, 
                out bool found)
            {
                int min = 0;
                int max = GetWordsCount(dicFilePath) - 1;
                found = false;
               
                // If no words, then return directly.
                if (max == -1)
                {
                    return 0;
                }

                while (!found && min <= max)
                {
                    int current = (min + max)/2;
                    var currentWord = GetIthWord(dicFilePath, current);
                    if (word.CompareTo(currentWord) < 0)
                        max = current - 1;
                    else if (word.CompareTo(currentWord) > 0)
                        min = current + 1;
                    else
                        found = true;
                }
                return min;
            }

            private int GetWordsCount(String dicFilePath)
            {
                var stream = GetFileStream(dicFilePath);
                stream.Seek(0, SeekOrigin.Begin);
                return (int) stream.Length/WORD_CELL_LENGTH;
            }

            private string GetIthWord(String dicFilePath, int i)
            {
                var stream = GetFileStream(dicFilePath);
                int offset = i*WORD_CELL_LENGTH;
                stream.Seek(offset, SeekOrigin.Begin);
                var buffer = new byte[WORD_CELL_LENGTH];
                stream.Read(buffer, 0, WORD_CELL_LENGTH);
                return Encoding.ASCII.GetString(buffer).Trim();
            }

            private FileStream GetFileStream(String path)
            {
                if (this.fileStreams.ContainsKey(path))
                {
                    return this.fileStreams[path];
                }
                var stream = new FileStream(path, FileMode.Open);
                stream.Seek(0, SeekOrigin.Begin);
                this.fileStreams[path] = stream;
                return stream;
            }

            private byte[] ReadAllBytes(FileStream stream)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int) stream.Length);
                return buffer;
            }

            public void Dispose()
            {
                foreach (var stream in fileStreams.Values)
                {
                    stream.Close();
                }
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

        public void UpdateProjectName(String projectName)
        {
            if (this.projectName == null || !this.projectName.Equals(projectName))
            {
                this.projectName = projectName;
                this.dictionary = new FileDictionary(directory + this.projectName);
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

                String middel = word.Substring(prefixLength, word.Length - prefixLength - prefixLength);
                
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
