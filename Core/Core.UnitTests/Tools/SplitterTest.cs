using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.ExtensionContracts.SplitterContracts;
using UnitTestHelpers;

namespace Sando.Core.UnitTests
{
    [TestFixture]
    class SplitterTest
    {
        [Test]
        public void TestSplitCamelCase()
        {
            string[] parts = wordSplitter.ExtractWords("aLongVariableNameInCamelCase");
            Assert.AreEqual(parts.Length, 7);
        }

        [Test]
        public void TestSplitUnderscores()
        {
            string[] parts = wordSplitter.ExtractWords("a_name_separated_by_lots_of_underscores");
            Assert.AreEqual(parts.Length, 7);
        }

        [Test]
        public void TestUnsplittable()
        {
            string[] parts = wordSplitter.ExtractWords("unsplittable");
            Assert.AreEqual(parts.Length, 1);
        }

        [Test]
        public void TestAbbreviations()
        {
            string[] parts = wordSplitter.ExtractWords("whatAboutALM");
            Assert.IsTrue(parts.Length == 3);
        }

        [Test]
        public void TestAllCaps()
        {
            string[] parts = wordSplitter.ExtractWords("WHATIFALLINCAPS");
            Assert.AreEqual(parts.Length, 1);
        }

        [Test]
        public void TestAllCapsUnderscore()
        {
            string[] parts = wordSplitter.ExtractWords("WHAT_IF_ALL_IN_CAPS");
            Assert.AreEqual(parts.Length, 5);
        }

        [Test]
        public void TestBeginUnderscore()
        {
            string[] parts = wordSplitter.ExtractWords("_beginInUnderscore");
            Assert.AreEqual(parts.Length, 3);
        }

        [Test]
        public void ShortcutInName()
        {
            string[] parts = wordSplitter.ExtractWords("FBIInUnderscore");
            Assert.AreEqual(parts.Length, 3);
        }

        [Test]
        public void PseudoShortcutInName()
        {
            string[] parts = wordSplitter.ExtractWords("IInUnderscore");
            Assert.AreEqual(parts.Length, 3);
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside quotes\" another_word");
            Assert.AreEqual(parts.Count, 4);
            Assert.AreEqual(String.Join("*", parts), "words inside quotes*word*another*word");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedWithInvalidQuote()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside quotes\" another\"word");
            Assert.AreEqual(parts.Count, 4);
            Assert.AreEqual(String.Join("*", parts), "words inside quotes*word*another*word");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNoQuotesUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word words inside quotes another_word");
            Assert.AreEqual(parts.Count, 6);
            Assert.AreEqual(String.Join("*", parts), "word*words*inside*quotes*another*word");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsEmptyListWhenSearchTermIsEmptyString()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms(String.Empty);
            Assert.AreEqual(parts.Count, 0);
        }

        [Test]
        public void ExtractSearchTerms_ReturnsEmptyListWhenSearchTermContainsInvalidCharacters()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("\\/:~");
            Assert.AreEqual(parts.Count, 0);
        }

        [Test]
        public void ExtractSearchTerms_ContractFailsWhenSearchTermIsNull()
        {
            try
            {
                WordSplitter.ExtractSearchTerms(null);
            }
            catch
            {
                //contract exception catched here
            }
            Assert.True(contractFailed, "Contract should fail!");
        }


        [Test]
        public void TestPerformance()
        {
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 500; i++)
            {
                string[] parts = wordSplitter.ExtractWords("_beginInUnderscore");
                Assert.IsTrue(parts.Length == 3);
            }
            watch.Stop();
            Assert.IsTrue(watch.ElapsedMilliseconds < 500);
        }

        [SetUp]
        public void ResetContract()
        {
            contractFailed = false;
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetHandled();
                e.SetUnwind();
                contractFailed = true;
            };
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            TestUtils.InitializeDefaultExtensionPoints();

            wordSplitter = ExtensionPointsRepository.Instance.GetWordSplitterImplementation();
        }

        private bool contractFailed;
        private IWordSplitter wordSplitter;
    }
}
