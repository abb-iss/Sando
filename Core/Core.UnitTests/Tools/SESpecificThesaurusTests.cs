using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    class SESpecificThesaurusTests
    {
        private readonly SESpecificThesaurus thesaurus = SESpecificThesaurus.GetInstance();

        [SetUp]
        public void SetUp()
        {
            thesaurus.Initialize();
        }

        [Test]
        public void QueryFreqentWord()
        {
            AssertWordExist("add");
            AssertWordExist("get");
            AssertWordExist("debug");
            AssertWordExist("retrieve");
            AssertWordExist("file");
            AssertWordExist("release");
        }

        [Test]
        public void QueryNonExistingWord()
        {
            AssertNonExistingWord("fafad");
            AssertNonExistingWord("");
            AssertNonExistingWord(null);
            AssertNonExistingWord("&*##@");
            AssertNonExistingWord("324312");
        }

        private void AssertNonExistingWord(String word)
        {
            var words = thesaurus.GetSynonyms(word).ToList();
            Assert.IsTrue(!words.Any());
        }

        private void AssertWordExist(String word)
        {
            var words = thesaurus.GetSynonyms(word).ToList();
            Assert.IsNotNull(words);
            Assert.IsTrue(words.Any());
        }
    }
}
