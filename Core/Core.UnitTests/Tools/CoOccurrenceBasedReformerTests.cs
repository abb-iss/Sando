using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.QueryRefomers;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    class CoOccurrenceBasedReformerTests : RandomStringBasedTests
    {
        private readonly DictionaryBasedSplitter dictionary;
        

        public CoOccurrenceBasedReformerTests()
        {
            dictionary = new DictionaryBasedSplitter();
        }

        [SetUp]
        public void Intialize()
        {
            dictionary.Initialize(@"TestFiles\");
        }

        private IEnumerable<string> GetReformedWord(String target, IEnumerable<String> context)
        {
            var reformer = new CoOccurrenceBasedReformer(dictionary);
            reformer.SetContextWords(context);
            var list = reformer.GetReformedTarget(target).ToList();
            return list.Select(w => w.NewTerm);
        }

        private IEnumerable<String> GetWordsCoOccurWithWords(IEnumerable<String> words)
        {
            return words.Select(w => dictionary.GetCoOccurredWordsAndCount(w).Select(p => p.Key))
                     .Aggregate((list1, list2) => list1.Intersect(list2)).Distinct();
        }

        private void AssertReformProperly(String target, IEnumerable<String> context)
        {
            var list = GetWordsCoOccurWithWords(context);
            var subList = GetReformedWord(target, context);
            Assert.IsTrue(list.Any());
            Assert.IsTrue(subList.Any());
            Assert.IsTrue(subList.All(w => list.Contains(w)));
        }

        private void AssertNotReformed(String target, IEnumerable<String> context)
        {
            var list = GetReformedWord(target, context);
            Assert.IsTrue(!list.Any());
        }
            
        [Test]
        public void ReformWordWithoutContext()
        {
            Assert.IsTrue(!GetReformedWord("", new []{""}).Any());
            Assert.IsTrue(!GetReformedWord("sando", new string[]{}).Any());
            Assert.IsTrue(!GetReformedWord("abb", new string[]{""}).Any());
        }

        [Test]
        public void ReformWordWithOneNeighborredWord()
        {
            AssertReformProperly("somewiredwodnotindictionary", new string[] { "activator", "add" });
            AssertReformProperly("somewiredwodnotindictionary", new string[] { "allow", "add" });
            AssertNotReformed("sando", new string[]{"add", "activator"});
            AssertNotReformed("sando", new string[]{"add", "allow"});
        }


    }
}
