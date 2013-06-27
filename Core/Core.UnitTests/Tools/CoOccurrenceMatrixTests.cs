using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class CoOccurrenceMatrixTests : RandomStringBasedTests
    {
        private readonly InternalWordCoOccurrenceMatrix matrix = 
            new InternalWordCoOccurrenceMatrix();
        private const String directory = @"C:\Windows\Temp\Dictionary\";

        [SetUp]
        public void SetUp()
        {
            DeleteCreatedFile();
            CreateDirectory(directory);
            matrix.Initialize(directory);
        }

        [TearDown]
        public void DisposingMatrix()
        {
            matrix.Dispose();
            DeleteCreatedFile();
        }

        [Test]
        public void AddWordsSeveralTimes()
        {
            const int listLength = 20;
            const int coocurrenceCount = 3;
            var words = GenerateRandomWordList(listLength);
            for (int i = 0; i < coocurrenceCount; i++)
            {
                matrix.HandleCoOcurrentWords(words);   
            }
            for (int i = 0; i < listLength; i ++)
            {
                for (int j = 0; j < listLength; j ++)
                {
                    var word1 = words.ElementAt(i);
                    var word2 = words.ElementAt(j);
                    var count = matrix.GetCoOccurrenceCount(word1, word2);
                    Assert.IsTrue(count == coocurrenceCount);
                }
            }
        }

        [Test]
        public void ConfirmAssumptionAboutStringComparisons()
        {
            Assert.IsTrue("abc".CompareTo("") > 0);
            Assert.IsTrue("a".CompareTo("") > 0);
            Assert.IsTrue("zzzzzzzz".CompareTo("") > 0);
        }

        [Test]
        public void QueryWordsDoesNotDependOnOrder()
        {
            matrix.HandleCoOcurrentWords(new string[]{"word1", "word2"});
            Assert.IsTrue(matrix.GetCoOccurrenceCount("word1", "word2") == 1);
            Assert.IsTrue(matrix.GetCoOccurrenceCount("word2", "word1") == 1);
        }

        [Test]
        public void PerformanceTest()
        {
            var stopwatch = new Stopwatch();
            var words = GenerateRandomWordList(100);
            stopwatch.Start();
            matrix.HandleCoOcurrentWords(words);
            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            Assert.IsTrue(time < 1000);
        }
      
    }
}
