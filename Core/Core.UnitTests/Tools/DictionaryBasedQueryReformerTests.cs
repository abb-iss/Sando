using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class DictionaryBasedQueryReformerTests
    {
        private const String directory = @"TestFiles\";
        private readonly DictionaryBasedSplitter dictionary;
        private readonly DictionaryBasedQueryReformer reformer;

        public DictionaryBasedQueryReformerTests()
        {
            this.dictionary = new DictionaryBasedSplitter();
            this.reformer = new DictionaryBasedQueryReformer(dictionary);
            dictionary.Initialize(directory);
            reformer.Initialize();
        }

        private void AssertNotReformed(IEnumerable<string> words)
        {
            var newTermLists = reformer.ReformTermsSynchronously(words);
            Assert.IsTrue(newTermLists.Count() == 1);
            var changedTerms = newTermLists.First();
            Assert.IsTrue(changedTerms.Count() == words.Count());
            for (int i = 0; i < changedTerms.Count(); i++)
            {
                Assert.IsTrue(changedTerms.ElementAt(i).Equals(words.
                    ElementAt(i)));
            }
        }

        private void AssertReformed(String[] words, int[] changedIndexes)
        {
            var newTermLists = reformer.ReformTermsSynchronously(words).ToList();
            Assert.IsTrue(newTermLists.Any());
            foreach (List<string> newTermList in newTermLists)
            {
                var list = newTermList.ToList();
                for (int i = 0; i < list.Count(); i++)
                {
                    if (changedIndexes.Contains(i))
                    {
                        Assert.IsTrue(!list.ElementAt(i).Equals(words.ElementAt(i)));
                    }
                    else
                    {
                        Assert.IsTrue(list.ElementAt(i).Equals(words.ElementAt(i)));
                    }
                }
            }
        }

        [Test]
        public void ReformWiredTerms()
        {
           reformer.ReformTermsSynchronously(new string[]{"", ""});
        }

        [Test]
        public void ReformOneTermInDictionary()
        {
            AssertNotReformed(new string[] { "sando" });
            AssertNotReformed(new string[] { "add" });
            AssertNotReformed(new string[] { "adjust" });
            AssertNotReformed(new string[] { "after" });
            AssertNotReformed(new string[] { "alignment" });
            AssertNotReformed(new string[] { "alignment","add","after"});
        }

        [Test]
        public void ReformOneTermNotInDictionary()
        {
            AssertReformed(new string[] {"addi"}, new int[]{0});
            AssertReformed(new string[] {"addi", "add"}, new int[]{0});
            AssertReformed(new string[] {"add", "additional", "addi"}, new int[]{2});
            AssertReformed(new string[] {"san"}, new int[]{0});
            AssertReformed(new string[] {"adjusting"}, new int[]{0});
            AssertReformed(new string[] {"aft"}, new int[]{0});
            AssertReformed(new string[] {"alignme"}, new int[]{0});
            AssertReformed(new string[] {"addi", "aft", "alignme"}, new int[]{0, 1, 2});
        }       
    }
}
