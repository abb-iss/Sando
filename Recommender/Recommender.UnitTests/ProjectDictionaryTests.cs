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
        private static Random random = new Random((int) DateTime.Now.Ticks);
        private ProjectDictionary dictionary;

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
            this.dictionary = ProjectDictionary.GetInstance();
            dictionary.UpdateProjectName("test");
        }


        [Test]
        public void Method1()
        {
            const string alpha = "abcdefghijklmnopqrstuvwxwz";
            var words = new List<String>();

            for (int i = 0; i < 26; i++)
            {
                words.Add(alpha.Substring(26 - i - 1, 1));
            }

            this.dictionary.AddWords(words);
            foreach (string word in words)
            {
                Assert.IsTrue(this.dictionary.DoesWordExist(word));
            }
        }

        [Test]
        public void Method2()
        {
            var words = new List<String>();
            for (int i = 0; i < 1000; i ++)
            {
                words.Add(GenerateRandomString(30));
            }
            this.dictionary.AddWords(words);
            foreach (string word in words)
            {
                this.dictionary.DoesWordExist(word);
            }
        }

        [Test]
        public void Method3()
        {
            var words = new List<String>();
            for (int i = 0; i < 1000; i++)
            {
                words.Add(GenerateRandomString(15));
            }
            this.dictionary.AddWords(words);
            for (int i = 0; i < 100; i++)
            {
                var combinedWords = CombiningWords(words, 2);
                var watch = Stopwatch.StartNew();
                var subWords = this.dictionary.ExtractWords(combinedWords);
                var time = watch.ElapsedMilliseconds;
                Assert.IsTrue(time<10);
                Assert.IsTrue(subWords.Count() == 2);
            }
        }
    }
}
