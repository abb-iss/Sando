using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.ExtensionContracts.SplitterContracts;
using UnitTestHelpers;

namespace Sando.Core.UnitTests.Tools
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
        public void ExtractSearchTermsTestLeaveCompleteTerm()
        {
            var parts = WordSplitter.ExtractSearchTerms("_solutionEvents");
            Assert.AreEqual(parts.Count, 1);
            Assert.IsTrue(parts.Contains("\"_solutionEvents\""));
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
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"word \"words inside quotes\" another_word\"", String.Join("*", parts));

            parts = WordSplitter.ExtractSearchTerms("return \"..\\\\..\\\\Parser\"");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"return \"..\\\\..\\\\Parser\"\"", String.Join("*", parts));
        }

        [Test]
        public void InterpretSearchQuery()
        {
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("Assert.AreEqual(4, parts.Count);"));
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedInsideQuotes()"));
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedInsideQuotes("));
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("WordSplitter.InvalidCharactersFound(\"\"wordSplitter.ExtractWords(\"IInUnderscore\")\"\")"));
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("simpleSearchCriteria.ToQueryString();"));
            Assert.IsFalse(WordSplitter.IsUnquotedLiteral("open file"));
            Assert.IsFalse(WordSplitter.IsUnquotedLiteral("\"sadfasdfasdfasdfasdfasdf\""));
            Assert.IsFalse(WordSplitter.IsUnquotedLiteral(" \"sadfasdfasdfasdfasdfasdf\" "));
            Assert.IsFalse(WordSplitter.IsUnquotedLiteral(" open file -test "));
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("\"sadfasdfasdfasdfasdfasdf\" test"));
            Assert.IsTrue(WordSplitter.IsUnquotedLiteral("return \"..\\..\\Parser\";"));

            var parsed = WordSplitter.ExtractSearchTerms("return \"..\\..\\Parser\";").ToArray()[0];
            Assert.IsTrue(parsed.Equals("\"return \"..\\..\\Parser\";\""));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedInsideQuotes()
        {
            Assert.IsFalse(WordSplitter.InvalidCharactersFound("\"wordSplitter.ExtractWords(\"IInUnderscore\")\""));
            Assert.IsFalse(WordSplitter.InvalidCharactersFound("\"Assert.IsNotNull(wordSplitter, \"Default word splitter should be used!\");\""));
            Assert.IsNotNull(WordSplitter.ExtractSearchTerms("\"Assert.IsNotNull(wordSplitter, \"Default word splitter should be used!\");\""));            
            List<string> parts = WordSplitter.ExtractSearchTerms("\"wordSplitter.ExtractWords(\"IInUnderscore\")\"");
            Assert.AreEqual(1, parts.Count);            
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenQuotesUsedWithInvalidQuote()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside quotes\" another word");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual(String.Join("*", parts), "\"word \"words inside quotes\" another word\"");
        }


        [Test]
        public void ExtractSearchTerms_SlashesInsideQuotesInsideQuote()
        {
            //implicit quotes
            List<string> parts = WordSplitter.ExtractSearchTerms("IsUnquotedLiteral(\"..\\Path\"))");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"IsUnquotedLiteral(\"..\\Path\"))\"", String.Join(" ", parts));

            //explicit quotes
            parts = WordSplitter.ExtractSearchTerms("\"IsUnquotedLiteral(\"..\\Path\"))\"");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"IsUnquotedLiteral(\"..\\Path\"))\"", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenColonUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file::open::now");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"file::open::now\"", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenEqualityOperatorUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file=new");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"file=new\"", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenBracketsUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file(new File())");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"file(new File())\"", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUpperCasesUsed1()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("fileOpenNow");
            Assert.AreEqual(4, parts.Count);
            Assert.AreEqual("fileopennow file open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUpperCasesUsed2()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("FileOpenNow");
            Assert.AreEqual(4, parts.Count);
            Assert.AreEqual("fileopennow file open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUpperCasesUsed3()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("FileTXTOpenNow");
            Assert.AreEqual(5, parts.Count);
            Assert.AreEqual("filetxtopennow file txt open now", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNumbersUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file324");
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual("file324 file 324", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNumbersUsed2()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("Mp3Player");
            Assert.AreEqual(4, parts.Count);
            Assert.AreEqual("mp3player mp 3 player", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenUnderscoreUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("file_open_now");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"file_open_now\"", String.Join(" ", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNoQuotesUsed()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word words inside quotes another_word");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual(String.Join("*", parts), "\"word words inside quotes another_word\"");
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNoOperatortUsedWithQuotes()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word \"words inside\"");
            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("\"word \"words inside\"\"", String.Join("*", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsValidNumberOfSearchTermsWhenNoOperatortUsedWithoutQuotes()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms("word -about");
            Assert.AreEqual(2, parts.Count);
            Assert.AreEqual("word*-about", String.Join("*", parts));
        }

        [Test]
        public void ExtractSearchTerms_ReturnsEmptyListWhenSearchTermIsEmptyString()
        {
            List<string> parts = WordSplitter.ExtractSearchTerms(String.Empty);
            Assert.AreEqual(0, parts.Count);
        }

 

        [Test]
        public void InvalidCharactersFound_ReturnsTrueWhenInvalidCharactersUsed()
        {
            bool invalidCharactersFound = WordSplitter.InvalidCharactersFound("???");
            Assert.IsTrue(invalidCharactersFound);
        }

        [Test]
        public void ParsingQuery()
        {
            bool invalidCharactersFound = WordSplitter.InvalidCharactersFound("session file info filetype:h");
            Assert.IsFalse(invalidCharactersFound);
            var terms = WordSplitter.ExtractSearchTerms("session file info filetype:h");
            WordSplitter.GetFileExtensions("session file info filetype:h");
        }

        [Test]
        public void InvalidCharactersFound_ReturnsFalseWhenInvalidCharactersUsedInQuotes()
        {
            bool invalidCharactersFound = WordSplitter.InvalidCharactersFound("\"???\"");
            Assert.IsFalse(invalidCharactersFound);
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
        public void ExtractSearchTerms_FirstPartIsAlwaysOriginal()
        {
            const string term1 = "splitWord";
            const string term2 = "splitAgain";
            List<string> parts = WordSplitter.ExtractSearchTerms(term1);
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual(parts[0], term1.ToLower());

            parts = WordSplitter.ExtractSearchTerms(term2);
            Assert.AreEqual(3, parts.Count);
            Assert.AreEqual(parts[0], term2.ToLower());

            parts = WordSplitter.ExtractSearchTerms(term1 + " " + term2);
            Assert.AreEqual(5, parts.Count);
            Assert.AreEqual(parts[0], term1.ToLower());
            Assert.AreEqual(parts[1], term2.ToLower());
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
