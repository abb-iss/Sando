using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class WordCorrectorTests
    {
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);
        private readonly WordCorrector corrector = new WordCorrector(); 

        private string GenerateRandomString(int size)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                char ch = GenerateRandomChar();
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private char GenerateRandomChar()
        {
            return Convert.ToChar(GenerateRandomInt(97, 122));
        }

        private int GenerateRandomInt(int min, int max)
        {
            return Convert.ToInt32(Math.Floor((max - min + 1) * 
                random.NextDouble() + min));
        }

        private IEnumerable<string> GenerateRandomWordList(int length)
        {
            var words = new List<String>();
            for (int i = 0; i < length; i++)
            {
                words.Add(GenerateRandomString(15));
            }
            return words;
        }

        [Test]
        public void AddMultipleWords()
        {
            corrector.AddWords(GenerateRandomWordList(200));
        }

        [Test]
        public void RandomIntegersTest()
        {
            var min = 0;
            var max = 100;
            var lowReached = false;
            var highReached = false;
            for (int i = 0; i < 1000; i++)
            {
                var value = GenerateRandomInt(min, max);
                Assert.IsTrue(value >= min);
                Assert.IsTrue(value <= max);
                if (value == min)
                    lowReached = true;
                if (value == max)
                    highReached = true;
            }
            Assert.IsTrue(lowReached);
            Assert.IsTrue(highReached);

        }

        [Test]
        public void QuerySameWords()
        {
            for (int i = 0; i < 100; i++)
            {
                string word = GenerateRandomString(20);
                corrector.AddWords(GenerateRandomWordList(200));
                corrector.AddWords(new string[] {word});
                var results = corrector.FindSimilarWords(word).ToArray();
                Assert.IsNotNull(results);
                Assert.IsTrue(results.Any());
                Assert.IsTrue(results.First().Equals(word));
            }
        }

        [Test]
        public void QuerySlightlyDifferentWords()
        {
            for (int i = 0; i < 100; i++)
            {
                string word = GenerateRandomString(20);
                corrector.AddWords(GenerateRandomWordList(200));
                corrector.AddWords(new string[] { word });
                int splitIndex = GenerateRandomInt(0, word.Length - 1);
                var changedWord = word.Substring(0, splitIndex + 1) + GenerateRandomChar() + word.
                    Substring(splitIndex + 1);
                var results = corrector.FindSimilarWords(changedWord).ToArray();
                Assert.IsNotNull(results);
                Assert.IsTrue(results.Any());
                Assert.IsTrue(results.First().Equals(word));
            }
        }
    }
}
