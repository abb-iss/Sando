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
            var words = thesaurus.GetSynonyms("add").ToList();
            Assert.IsNotNull(words);
            Assert.IsTrue(words.Any());
        }

        [Test]
        public void QueryNonExistingWord()
        {
            AssertNonExistingWord("fafad");
            AssertNonExistingWord("");
            AssertNonExistingWord(null);
        }

        private void AssertNonExistingWord(String word)
        {
            var words = thesaurus.GetSynonyms(word).ToList();
            Assert.IsTrue(!words.Any());
        }
    }
}
