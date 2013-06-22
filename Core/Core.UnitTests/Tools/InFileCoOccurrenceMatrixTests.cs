using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    class InFileCoOccurrenceMatrixTests
    {
        public IWordCoOccurrenceMatrix matrix;

        public InFileCoOccurrenceMatrixTests()
        {
            this.matrix = new InternalWordCoOccurrenceMatrix();
        }

        [SetUp]
        public void ReadData()
        {
            matrix.Initialize(@"TestFiles\");
        }

        private void AssertWordPairExist(string word1, string word2)
        {
            Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) > 0);
        }

        private void AssertWordPairNonExist(string word1, string word2)
        {
            Assert.IsTrue(matrix.GetCoOccurrenceCount(word1, word2) == 0);
        }

        [Test]
        public void SameLocalDictionaryWordPairAlwaysExist()
        {
            AssertWordPairExist("sando", "sando");
            AssertWordPairExist("abb", "abb");
            AssertWordPairExist("test", "test");
        }

        [Test]
        public void SameNonLocalDictionaryWordNeverExist()
        {
            AssertWordPairNonExist("animal", "animal");
            AssertWordPairNonExist("bush", "bush");
            AssertWordPairNonExist("pinkcolor", "pinkcolor");
        }

        [Test]
        public void DifferentWordPairsThatExist()
        {
            AssertWordPairExist("confidence", "false");
            AssertWordPairExist("confidence", "locat");
            AssertWordPairExist("configuration", "middle");
            AssertWordPairExist("configuration", "results");
        }

        [Test]
        public void DifferentWordPairsThatDoesNotExist()
        {
            AssertWordPairNonExist("confidence", "apple");
            AssertWordPairNonExist("confidence", "lackof");
            AssertWordPairNonExist("confidence", "configuration");
            AssertWordPairNonExist("configuration", "nomad");   
        }
    }
}
