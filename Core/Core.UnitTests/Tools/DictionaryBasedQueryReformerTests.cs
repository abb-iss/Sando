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
    public class DictionaryBasedQueryReformerTests
    {
        private const String directory = @"TestFiles\";
        private readonly DictionaryBasedSplitter dictionary;
        private readonly QueryReformerManager reformer;

        public DictionaryBasedQueryReformerTests()
        {
            this.dictionary = new DictionaryBasedSplitter();
            this.reformer = new QueryReformerManager(dictionary);
            dictionary.Initialize(directory);
        }

        private void AssertNotReformed(IEnumerable<string> words)
        {
            var newQueries = reformer.ReformTermsSynchronously(words);
            if (newQueries.Any())
            {
                Assert.IsTrue(newQueries.Count() == 1);
                var query = newQueries.First();
                Assert.IsTrue(query.ReformedWords.All(q => q.Category ==
                    TermChangeCategory.NOT_CHANGED));
            }
        }

        private void AssertReformed(String[] words, int[] changedIndexes, String[] expectedReforms)
        {
            if(changedIndexes.Count() != expectedReforms.Count())
                Assert.IsTrue(false);
            var reformForEachWord = new List<String>[words.Count()];
            var newQueries = reformer.ReformTermsSynchronously(words).ToList();
            Assert.IsTrue(newQueries.Any());
            var termLists = newQueries.Select(q => q.ReformedWords.Select(p => p.NewTerm));
            foreach (IEnumerable<string> newTermList in termLists)
            {
                var list = newTermList.ToList();
                for (int i = 0; i < list.Count(); i++)
                {
                    if (changedIndexes.Contains(i))
                    {
                        Assert.IsTrue(!list.ElementAt(i).Equals(words.ElementAt(i)));
                        if (reformForEachWord[i] == null)
                        {
                            reformForEachWord[i] = new List<string>();
                        }
                        var reform = reformForEachWord[i];
                        reform.Add(list.ElementAt(i));
                    }
                    else
                    {
                        Assert.IsTrue(list.ElementAt(i).Equals(words.ElementAt(i)));
                    }
                }
            }

            for (int i = 0; i < changedIndexes.Count(); i++)
            {
                var index = changedIndexes.ElementAt(i);
                var expected = expectedReforms.ElementAt(i);
                var reformWords = reformForEachWord[index];
                Assert.IsNotNull(reformWords);
                Assert.IsTrue(reformWords.Any());
                Assert.IsTrue(reformWords.Contains(expected));
            }
        }

        private void AssertOriginalTerm(IEnumerable<IReformedQuery> queries, String term)
        {
            Assert.IsTrue(queries.All(q => q.ReformedWords.All(p => p.OriginalTerm.Equals(term))));
        }

        [SetUp]
        public void setUp()
        {
            reformer.Initialize();
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
            AssertReformed(new string[] {"addi"}, new int[] {0}, new string[]{"add"});
            AssertReformed(new string[] {"addi", "add"}, new int[] {0}, new string[]{"add"});
            AssertReformed(new string[] {"add", "additional", "addi"}, new int[] {2}, 
                new string[]{"adding"});
            AssertReformed(new string[] {"san"}, new int[] {0}, new string[]{"sando"});
            AssertReformed(new string[] {"adjusting"}, new int[] {}, new string[]{});
            AssertReformed(new string[] {"aft"}, new int[] {0}, new string[]{"after"});
            AssertReformed(new string[] {"alignme"}, new int[] {0}, new string[]{"alignment"});
            AssertReformed(new string[] {"Sano", "Sand", "Sando"}, new int[] {0, 1}, new 
                string[]{"sando", "sando"});
        }

        [Test]
        public void NotReformingWordWhoseStemmingIsInDictionary()
        {
            AssertNotReformed(new string[]{"adapted"});
            AssertNotReformed(new string[]{"searchers"});
            AssertNotReformed(new string[]{"absentive"});
            AssertNotReformed(new string[]{"accessive"});
            AssertNotReformed(new string[]{"actually"});
            AssertNotReformed(new string[]{"automatically"});
            AssertNotReformed(new string[]{"application"});
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
            var q1 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                MISSPELLING);
            var q2 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                SE_SYNONYM);
            var q3 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
               GENERAL_SYNONYM);
            Assert.IsTrue(q1.Any());
            Assert.IsTrue(!q2.Any());
            Assert.IsTrue(!q3.Any());
        }


        [Test]
        public void TermChangeTypeTestForWordExistingInLocalDictionary()
        {
            const string word = "add";
            var newQueries = reformer.ReformTermsSynchronously(new string[] {word});
            AssertOriginalTerm(newQueries, word);
            var q1 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                MISSPELLING);
            var q2 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                SE_SYNONYM);
            var q3 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                GENERAL_SYNONYM);
 
            Assert.IsTrue(!q1.Any());
            Assert.IsTrue(!q2.Any());
            Assert.IsTrue(!q3.Any());
        }

        [Test]
        public void TermChangeTypeTestForWordNotExistingInLocalDictionary()
        {
            const string word = "instantiate";
            var newQueries = reformer.ReformTermsSynchronously(new string[] {word});
            AssertOriginalTerm(newQueries, word);
            var q1 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                MISSPELLING);
            var q2 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                SE_SYNONYM);
            var q3 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                GENERAL_SYNONYM);
            Assert.IsTrue(q1.Any());
            Assert.IsTrue(q2.Any());
            Assert.IsTrue(!q3.Any());
            var word1 = q1.First().ReformedWords.First().NewTerm;
            var word2 = q2.First().ReformedWords.First().NewTerm;
            Assert.IsTrue(!word1.Equals(word2));
        }

        [Test]
        public void TermChangeTypeForTermsInGeneralEnglishDictionary()
        {
            const string word = "principal";
            var newQueries = reformer.ReformTermsSynchronously(new string[] { word });
            AssertOriginalTerm(newQueries, word);
            var q1 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                MISSPELLING);
            var q2 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                SE_SYNONYM);
            var q3 = newQueries.Where(q => q.ReformedWords.First().Category == TermChangeCategory.
                GENERAL_SYNONYM);
            Assert.IsTrue(q1.Any());
            Assert.IsTrue(!q2.Any());
            Assert.IsTrue(q3.Any());
        }

        [Test]
        public void TestingTermsThatNeverOccurTogetherAreFiltered()
        {
            var reformed = reformer.ReformTermsSynchronously(new string[] {"sando", "me"});
            reformed = reformer.ReformTermsSynchronously(new string[] {"sand", "s", "and"});
            
        }
    }
}
