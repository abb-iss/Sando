using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    class LocalDictionaryTests : RandomStringBasedTests
    {
        private const string tempFolder = @"C:\Windows\Temp\Dictionary\";
        private DictionaryBasedSplitter _dictionaryBasedSplitter;
    
        [SetUp]
        public void SetUp()
        {
            this._dictionaryBasedSplitter = new DictionaryBasedSplitter();
            CreateDirectory(tempFolder);
            _dictionaryBasedSplitter.Initialize(tempFolder);
        }

        [TearDown]
        public void Disposing()
        {
            _dictionaryBasedSplitter.Dispose();
            DeleteCreatedFile();
        }


        [Test]
        public void GetStemmedWord()
        {
            for (int i = 0; i < 1000; i ++)
            {
                string word = "adding";
                var stemmedWord = DictionaryHelper.GetStemmedQuery(word);
                Assert.IsTrue(stemmedWord.Equals("ad"));
                word = "add";
                stemmedWord = DictionaryHelper.GetStemmedQuery(word);
                Assert.IsTrue(stemmedWord.Equals("add"));
            }
        }


        [Test]
        public void MakeSureCanNotAddShortWords()
        {
            for (int i = 0; i < 100; i++)
            {
                var word = GenerateRandomString(2);
                _dictionaryBasedSplitter.AddWords(new[] { word });
                Assert.IsTrue(!_dictionaryBasedSplitter.DoesWordExist(word, 
                    DictionaryOption.NoStemming));
            }
        }

        [Test]
        public void AddSeveralWords()
        {
            const string alpha = "abcdefghijklmnopqrstuvwxwz";
            const int length = 3;
            var words = new List<String>();

            for (int i = 0; i <= 26 - length; i++)
            {
                words.Add(alpha.Substring(i, length));
            }
            this._dictionaryBasedSplitter.AddWords(words);
            foreach (string word in words)
            {
                Assert.IsTrue(this._dictionaryBasedSplitter.DoesWordExist(word, 
                    DictionaryOption.NoStemming));
            }
        }

        [Test]
        public void AddManyWords()
        {
            var words = new List<String>();
            for (int i = 0; i < 100; i++)
            {
                words.Add(GenerateRandomString(30));
            }
            this._dictionaryBasedSplitter.AddWords(words);
            foreach (string word in words)
            {
                Assert.IsTrue(this._dictionaryBasedSplitter.DoesWordExist(word, 
                    DictionaryOption.NoStemming));
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
                _dictionaryBasedSplitter.Dispose();
            }

            foreach (string project in projectNames)
            {
                _dictionaryBasedSplitter.Initialize(tempFolder + project + @"\");
                var words = wordDictionary[project];
                foreach (string word in words)
                {
                    Assert.IsTrue(_dictionaryBasedSplitter.DoesWordExist(word, 
                        DictionaryOption.NoStemming));
                }
            }
        }

        [Test]
        public void SplitSimpleWord()
        {
            _dictionaryBasedSplitter.AddWords(new string[] { "int", "i" });
            var subWords = _dictionaryBasedSplitter.ExtractWords("inti");
            Assert.IsTrue(subWords[0].Equals("int"));
            Assert.IsTrue(subWords[1].Equals("i"));
        }


        [Test]
        public void SplitRandomWord()
        {
            for (int length = 0; length < 61; length++)
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
            for (int i = 0; i < length; i++)
            {
                var sb = new StringBuilder();
                sb.Append(prefixes.ElementAt(i));
                sb.Append(middels.ElementAt(i));
                sb.Append(suffixes.ElementAt(i));
                var subWords = _dictionaryBasedSplitter.ExtractWords(sb.ToString());
                Assert.IsTrue(subWords.Count() >= 3);
                Assert.IsTrue(subWords.First().Equals(prefixes.ElementAt(i)));
                Assert.IsTrue(subWords.Last().Equals(suffixes.ElementAt(i)));
            }
        }

        [Test]
        public void SplitSimpleQuote()
        {
            const string quote = "\"inti\"";
            _dictionaryBasedSplitter.AddWords(new string[] { "int", "i" });
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

        [Test]
        public void AddSpecialWords()
        {
            _dictionaryBasedSplitter.AddWords(new string[]{"abb"});
            _dictionaryBasedSplitter.DoesWordExist("abb", DictionaryOption.NoStemming);
        }

    }
}
