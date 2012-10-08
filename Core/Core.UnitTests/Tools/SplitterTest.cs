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
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual(String.Join("*", parts), "words inside quotes*word*another");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedWithInvalidQuote()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside quotes\" another\"word");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual(String.Join("*", parts), "words inside quotes*word*another");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenColonUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file::open::now");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("file open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenEqualityOperatorUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file=new");
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual("file new", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenBracketsUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file(new File())");
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual("file new", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUpperCasesUsed1()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("fileOpenNow");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("file open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUpperCasesUsed2()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("FileOpenNow");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("file open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUpperCasesUsed3()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("FileTXTOpenNow");
            Assert.AreEqual(4, parts.Count);
            Assert.AreEqual("file txt open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNumbersUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file324");
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual("file 324", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNumbersUsed2()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("Mp3Player");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("mp 3 player", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUnderscoreUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file_open_now");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("file open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNoQuotesUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word words inside quotes another_word");
            Assert.AreEqual(5, parts.Count);
            Assert.AreEqual(String.Join("*", parts), "word*words*inside*quotes*another");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsEmptyListWhenSearchTermIsEmptyString()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms(String.Empty);
            Assert.AreEqual(0, parts.Count);
        }

        [Test]
        public void ExtractSearchTerms_ReturnsEmptyListWhenSearchTermContainsInvalidCharacters()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("\\/:~");
            Assert.AreEqual(0, parts.Count);
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

            wordSplitter = ExtensionPointsRepository.GetInstance().GetWordSplitterImplementation();
        }

        private bool contractFailed;
        private IWordSplitter wordSplitter;
    }
}
