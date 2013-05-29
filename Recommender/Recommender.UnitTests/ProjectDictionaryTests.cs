using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.Recommender.UnitTests
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
            this._dictionaryBasedSplitter = DictionaryBasedSplitter.GetInstance();
            CreateDirectory(tempFolder);
            _dictionaryBasedSplitter.Initialize(tempFolder);
        }

        [TearDown]
        public void DeleteCreatedFile()
        {
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
                Assert.IsTrue(time < 10);
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
    }
}
