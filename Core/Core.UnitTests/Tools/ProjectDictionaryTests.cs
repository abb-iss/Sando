using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests
{
    [TestFixture]
    class ProjectDictionaryTests
    {
        private const string tempFolder = @"C:\Windows\Temp\Dictionary\";
        private static Random random = new Random((int) DateTime.Now.Ticks);
        private DictionaryBasedSplitter _dictionaryBasedSplitter;
        private List<string> _createdDirectory = new List<string>();

        private string GenerateRandomString(int size)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.
                    Floor(26 * random.NextDouble() + 97)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private List<string> GenerateRandomWordList(int length)
        {
            var words = new List<String>();
            for (int i = 0; i < length; i++)
            {
                words.Add(GenerateRandomString(15));
            }
            return words;
        }

        private void CreateDirectory(String path)
        {
            Directory.CreateDirectory(path);
            if(!_createdDirectory.Contains(path))
                _createdDirectory.Add(path);
        }

        private string CombiningWords(IEnumerable<String> words, int wordCount)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < wordCount; i++)
            {
                int index = random.Next() % words.Count();
                sb.Append(words.ElementAt(index));
            }
            return sb.ToString();
        }

        [SetUp]
        public void SetUp()
        {
            this._dictionaryBasedSplitter = new DictionaryBasedSplitter();
            CreateDirectory(tempFolder);
            _dictionaryBasedSplitter.Initialize(tempFolder);
        }

        [TearDown]
        public void DeleteCreatedFile()
        {
            _dictionaryBasedSplitter.Dispose();
            foreach (string directory in _createdDirectory)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            _createdDirectory.Clear();
        }



        [Test]
        public void AddSeveralWords()
        {
            const string alpha = "abcdefghijklmnopqrstuvwxwz";
            var words = new List<String>();

            for (int i = 0; i < 26; i++)
            {
                words.Add(alpha.Substring(26 - i - 1, 1));
            }
            this._dictionaryBasedSplitter.AddWords(words);
            foreach (string word in words)
            {
                Assert.IsTrue(this._dictionaryBasedSplitter.DoesWordExist(word));
            }
        }

        [Test]
        public void AddManyWords()
        {
            var words = new List<String>();
            for (int i = 0; i < 1000; i ++)
            {
                words.Add(GenerateRandomString(30));
            }
            this._dictionaryBasedSplitter.AddWords(words);
            foreach (string word in words)
            {
                Assert.IsTrue(this._dictionaryBasedSplitter.DoesWordExist(word));
            }
        }

        [Test]
        public void SplitPerformanceTest()
        {
            var words = GenerateRandomWordList(1000);
            this._dictionaryBasedSplitter.AddWords(words);
            for (int i = 0; i < 100; i++)
            {
                var combinedWords = CombiningWords(words, 2);
                var watch = Stopwatch.StartNew();
                var subWords = this._dictionaryBasedSplitter.ExtractWords(combinedWords);
                var time = watch.ElapsedMilliseconds;
                Assert.IsTrue(time < 20);
                Assert.IsTrue(subWords.Count() == 2);
            }
        }

        [Test]
        public void UpdateProjectNamesAndReloadOldOnes()
        {
            var projectNames = GenerateRandomWordList(10);
            var wordDictionary = new Dictionary<String, IEnumerable<String>>();
            
            foreach (string project in projectNames)
            {
                CreateDirectory(tempFolder + project + @"\");
                _dictionaryBasedSplitter.Initialize(tempFolder + project + @"\");
                var words = GenerateRandomWordList(100);
                _dictionaryBasedSplitter.AddWords(words);
                wordDictionary.Add(project, words);
            }

            foreach (string project in projectNames)
            {
                _dictionaryBasedSplitter.Initialize(tempFolder + project + @"\");
                var words = wordDictionary[project];
                foreach (string word in words)
                {
                    Assert.IsTrue(_dictionaryBasedSplitter.DoesWordExist(word));
                }
            }
        }

        [Test]
        public void SplitSimpleWord()
        {
            _dictionaryBasedSplitter.AddWords(new string[]{"int", "i"});
            var subWords = _dictionaryBasedSplitter.ExtractWords("inti");
            Assert.IsTrue(subWords[0].Equals("int"));
            Assert.IsTrue(subWords[1].Equals("i"));
        }

        
        [Test]
        public void SplitRandomWord()
        {
            for (int length = 0; length < 61; length ++)
            {
                for (int i = 0; i < 100; i++)
                {
                    var word = GenerateRandomString(length);
                    _dictionaryBasedSplitter.ExtractWords(word);
                }
            }
        }

        [Test]
        public void SplitHalfSplittableWord()
        {
            const int length = 30;
            var prefixes = GenerateRandomWordList(length);
            var middels = GenerateRandomWordList(length);
            var suffixes = GenerateRandomWordList(length);
            _dictionaryBasedSplitter.AddWords(prefixes);
            _dictionaryBasedSplitter.AddWords(suffixes);
            for (int i = 0; i < length; i ++)
            {
                var sb = new StringBuilder();
                sb.Append(prefixes.ElementAt(i));
                sb.Append(middels.ElementAt(i));
                sb.Append(suffixes.ElementAt(i));
                var subWords = _dictionaryBasedSplitter.ExtractWords(sb.ToString());
                Assert.IsTrue(subWords.Count() == 3);
                Assert.IsTrue(subWords.ElementAt(0).Equals(prefixes.ElementAt(i)));
                Assert.IsTrue(subWords.ElementAt(1).Equals(middels.ElementAt(i)));
                Assert.IsTrue(subWords.ElementAt(2).Equals(suffixes.ElementAt(i)));
            }
        }

        private AutoResetEvent waitHandle ;
        private IEnumerable<string> selectedWords;

        [Test]
        public void SimilarWordsQueryTest()
        {
            waitHandle = new AutoResetEvent(false);
            const string word = "similar";
            var words = CreateSimilarWords(word);
            _dictionaryBasedSplitter.AddWords(words);
            _dictionaryBasedSplitter.QueryDictionary(DictionaryQueryFactory.
                GetSimilarWordsDictionaryQuery (word, Callback));
            waitHandle.WaitOne();
        }


        [Test]
        public void SplitSimpleQuote()
        {
            const string quote = "\"inti\"";
            _dictionaryBasedSplitter.AddWords(new string[]{"int", "i"});
            var words = _dictionaryBasedSplitter.ExtractWords(quote);
            Assert.IsTrue(words.Count() == 1);
            Assert.IsTrue(words.ElementAt(0).Equals(quote));
        }

        [Test]
        public void SplitQuoteWithNonQuote()
        {
            const string quote = "\"inti\"";
            const string nonQuote = "inti";
            const string mix1 = quote + " " + nonQuote;
            const string mix2 = nonQuote + " " + quote;
            const string mix3 = nonQuote + " " + quote + " " + nonQuote;
            const string mix4 = quote + " " + nonQuote + " " + quote;
            _dictionaryBasedSplitter.AddWords(new string[] { "int", "i" });
            
            var words = _dictionaryBasedSplitter.ExtractWords(mix1);
            Assert.IsTrue(words.Count() == 3);
            Assert.IsTrue(words.ElementAt(0).Equals("\"inti\""));
            Assert.IsTrue(words.ElementAt(1).Equals("int"));
            Assert.IsTrue(words.ElementAt(2).Equals("i"));
            
            words = _dictionaryBasedSplitter.ExtractWords(mix2);
            Assert.IsTrue(words.Count() == 3);
            Assert.IsTrue(words.ElementAt(0).Equals("int"));
            Assert.IsTrue(words.ElementAt(1).Equals("i"));
            Assert.IsTrue(words.ElementAt(2).Equals("\"inti\""));

            words = _dictionaryBasedSplitter.ExtractWords(mix3);
            Assert.IsTrue(words.Count() == 5);
            Assert.IsTrue(words.ElementAt(0).Equals("int"));
            Assert.IsTrue(words.ElementAt(1).Equals("i"));
            Assert.IsTrue(words.ElementAt(2).Equals("\"inti\""));
            Assert.IsTrue(words.ElementAt(3).Equals("int"));
            Assert.IsTrue(words.ElementAt(4).Equals("i"));

            words = _dictionaryBasedSplitter.ExtractWords(mix4);
            Assert.IsTrue(words.Count() == 1);
        }


        [Test]
        public void SplitQuoteInsideQuote()
        {
            string keywords = "\"Assert.IsNotNull(wordSplitter, \"Default word splitter should x used!!\");\"";
            var words = _dictionaryBasedSplitter.ExtractWords(keywords);
            Assert.IsTrue(words.Count() == 1);
        }

        [Test]
        public void SplitEmptyQuote()
        {
            const string quote = "\"\"";
            var words = _dictionaryBasedSplitter.ExtractWords(quote);
            Assert.IsTrue(words.Count() == 1);
            Assert.IsTrue(words.ElementAt(0).Equals(quote));
        }

        private static IEnumerable<string> CreateSimilarWords(String word)
        {
            var words = new List<String>();
            for (char c = 'a'; c <= 'z'; c++ )
                words.Add(word + c);
            return words;
        }

        private void Callback(IEnumerable<string> selectedWords)
        {
            this.selectedWords = selectedWords;
            waitHandle.Set();
        }
    }
}
