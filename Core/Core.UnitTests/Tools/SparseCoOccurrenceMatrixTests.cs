using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class SparseCoOccurrenceMatrixTests : RandomStringBasedTests
    {


        [Test]
        public void AddMultipleWords()
        {
            var matrix = new SparseCoOccurrenceMatrix();
            var words = this.GenerateRandomWordList(30);
            matrix.HandleCoOcurrentWordsSync(words);
            for (int i = 0; i < words.Count - 1; i ++ )
            {
                var word1 = words.ElementAt(i);
                var word2 = words.ElementAt(i + 1);
                Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) > 0);
            }

            for (int i = 0; i < words.Count - 5; i ++)
            {
                var word1 = words.ElementAt(i);
                var word2 = words.ElementAt(i + 4);
                Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) == 0);
            }
        }

        

        [Test]
        public void AddMultipleWordsMultipleTimes()
        {
            var matrix = new SparseCoOccurrenceMatrix();
            var words = this.GenerateRandomWordList(30);
            matrix.HandleCoOcurrentWordsSync(words);
            var words2 = this.GenerateRandomWordList(30);
            matrix.HandleCoOcurrentWordsSync(words2);

            for (int i = 0; i < words.Count - 1; i++)
            {
                var word1 = words.ElementAt(i);
                var word2 = words.ElementAt(i + 1);
                Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) > 0);
            }

            for (int i = 0; i < words2.Count - 1; i++)
            {
                var word1 = words2.ElementAt(i);
                var word2 = words2.ElementAt(i + 1);
                Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) > 0);
            }

            for (int i = 0; i < words.Count; i++)
            {
                var word1 = words.ElementAt(i);
                var word2 = words2.ElementAt(i);
                Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) == 0);
            }
        }

        [Test]
        public void GetAllEntriesFast()
        {
            var matrix = new SparseCoOccurrenceMatrix();
            var words = GenerateRandomWordList(30);
            matrix.HandleCoOcurrentWordsSync(words);
            var entries = matrix.GetEntries(n => true);
        }
    }
}
