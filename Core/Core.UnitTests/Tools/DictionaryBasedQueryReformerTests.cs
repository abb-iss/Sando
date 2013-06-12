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
        private readonly QueryReformer reformer;

        public DictionaryBasedQueryReformerTests()
        {
            this.dictionary = new DictionaryBasedSplitter();
            this.reformer = new QueryReformer(dictionary);
            dictionary.Initialize(directory);
            reformer.Initialize();
        }

        private void AssertNotReformed(IEnumerable<string> words)
        {
            var newQueries = reformer.ReformTermsSynchronously(words);
            Assert.IsTrue(newQueries.Count() == 1);
            var query = newQueries.First();
            Assert.IsTrue(query.ReformedQuery.All(q => q.Category ==
                                                       TermChangeCategory.NOT_CHANGED));
        }

        private void AssertReformed(String[] words, int[] changedIndexes)
        {
            var newQueries = reformer.ReformTermsSynchronously(words).ToList();
            Assert.IsTrue(newQueries.Any());
            var termLists = newQueries.Select(q => q.ReformedQuery.Select(p => p.ReformedTerm));
            foreach (IEnumerable<string> newTermList in termLists)
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

        private void AssertOriginalTerm(IEnumerable<IReformedQuery> queries, String term)
        {
            Assert.IsTrue(queries.All(q => q.ReformedQuery.All(p => p.OriginalTerm.Equals(term))));
        }

        [Test]
        public void ReformWiredTerms()
        {
            reformer.ReformTermsSynchronously(new string[] {"", ""});
        }

        [Test]
        public void ReformOneTermInDictionary()
        {
            AssertNotReformed(new string[] {"sando"});
            AssertNotReformed(new string[] {"add"});
            AssertNotReformed(new string[] {"adjust"});
            AssertNotReformed(new string[] {"after"});
            AssertNotReformed(new string[] {"alignment"});
            AssertNotReformed(new string[] {"alignment", "add", "after"});
            AssertNotReformed(new string[] {"Directory"});
        }

        [Test]
        public void ReformOneTermNotInDictionary()
        {
            AssertReformed(new string[] {"addi"}, new int[] {0});
            AssertReformed(new string[] {"addi", "add"}, new int[] {0});
            AssertReformed(new string[] {"add", "additional", "addi"}, new int[] {2});
            AssertReformed(new string[] {"san"}, new int[] {0});
            AssertReformed(new string[] {"adjusting"}, new int[] {0});
            AssertReformed(new string[] {"aft"}, new int[] {0});
            AssertReformed(new string[] {"alignme"}, new int[] {0});
            AssertReformed(new string[] {"addi", "aft", "alignme"}, new int[] {0, 1, 2});
            AssertReformed(new string[] {"addi", "aft", "Sando"}, new int[] {0, 1});
        }

        [Test]
        public void MakeSureLeavingQuotedTermUntouched()
        {
            AssertNotReformed(new string[]{"\"fadfs\""});
            AssertNotReformed(new string[]{"\"?fdat\""});
            AssertNotReformed(new string[]{"\"534q253\""});
            AssertNotReformed(new string[]{"\"nvcd\""});
            AssertNotReformed(new string[] { "\"nvcd\"", "\"534q253\"", "\"?fdat\"" });
        }


        [Test]
        public void TermChangeTypeTestForNonWord()
        {
            const string word = "addi";
            var newQueries = reformer.ReformTermsSynchronously(new string[] { word });
            AssertOriginalTerm(newQueries, word);
            var q1 = newQueries.Where(q => q.ReformedQuery.First().Category == TermChangeCategory.
                MISSPELLING_CORRECTION);
            var q2 = newQueries.Where(q => q.ReformedQuery.First().Category == TermChangeCategory.
                SYNONYM_IN_SE_THESAURUS);
            Assert.IsTrue(q1.Any());
            Assert.IsTrue(!q2.Any());
        }


        [Test]
        public void TermChangeTypeTestForWordExistingInLocalDictionary()
        {
            const string word = "add";
            var newQueries = reformer.ReformTermsSynchronously(new string[] {word});
            AssertOriginalTerm(newQueries, word);
            var q1 = newQueries.Where(q => q.ReformedQuery.First().Category == TermChangeCategory.
                MISSPELLING_CORRECTION);
            var q2 = newQueries.Where(q => q.ReformedQuery.First().Category == TermChangeCategory.
                SYNONYM_IN_SE_THESAURUS);
            Assert.IsTrue(!q1.Any());
            Assert.IsTrue(!q2.Any());
        }

        [Test]
        public void TermChangeTypeTestForWordNotExistingInLocalDictionary()
        {
            const string word = "instantiate";
            var newQueries = reformer.ReformTermsSynchronously(new string[] {word});
            AssertOriginalTerm(newQueries, word);
            var q1 = newQueries.Where(q => q.ReformedQuery.First().Category == TermChangeCategory.
                MISSPELLING_CORRECTION);
            var q2 = newQueries.Where(q => q.ReformedQuery.First().Category == TermChangeCategory.
                SYNONYM_IN_SE_THESAURUS);
            Assert.IsTrue(q1.Any());
            Assert.IsTrue(q2.Any());
            var word1 = q1.First().ReformedQuery.First().ReformedTerm;
            var word2 = q2.First().ReformedQuery.First().ReformedTerm;
            Assert.IsTrue(!word1.Equals(word2));
        }
    }
}
