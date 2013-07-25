using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class GeneralEnglishThesaurusTests
    {

        private readonly IThesaurus thesaurus = GeneralEnglishThesaurus.GetInstance();

        [SetUp]
        public void setUp()
        {
            thesaurus.Initialize(null);
        }

        [Test]
        public void QueryPopolarWords()
        {
            Assert.IsTrue(thesaurus.GetSynonyms("red").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("black").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("peach").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("house").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("dictionary").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("need").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("sand").Any());
            Assert.IsTrue(thesaurus.GetSynonyms("monkey").Any());
        }

        [Test]
        public void QueryWiredWords()
        {
            Assert.IsFalse(thesaurus.GetSynonyms("obama").Any());
            Assert.IsFalse(thesaurus.GetSynonyms("georgewbush").Any());
            Assert.IsFalse(thesaurus.GetSynonyms("philly").Any());
            Assert.IsFalse(thesaurus.GetSynonyms("fdaf").Any());
            Assert.IsFalse(thesaurus.GetSynonyms("verylongwords").Any());
            Assert.IsFalse(thesaurus.GetSynonyms("???").Any());
            Assert.IsFalse(thesaurus.GetSynonyms("7455489735").Any());
            Assert.IsFalse(thesaurus.GetSynonyms(" ").Any());
        }

        [Test]
        public void GetSynonymsCorrectly()
        {
            Assert.IsTrue(thesaurus.GetSynonyms("see").Select(s => s.Synonym).Contains("watch"));
            Assert.IsTrue(thesaurus.GetSynonyms("get").Select(s => s.Synonym).Contains("have"));
            Assert.IsTrue(thesaurus.GetSynonyms("however").Select(s => s.Synonym).Contains("nevertheless"));
            Assert.IsTrue(thesaurus.GetSynonyms("however").Select(s => s.Synonym).Contains("withal"));
            Assert.IsTrue(thesaurus.GetSynonyms("however").Select(s => s.Synonym).Contains("still"));
            Assert.IsTrue(thesaurus.GetSynonyms("however").Count() == 8);
        }
    }
}
