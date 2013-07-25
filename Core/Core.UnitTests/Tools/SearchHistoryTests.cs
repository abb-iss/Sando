using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class SearchHistoryTests : RandomStringBasedTests
    {
        private SearchHistory history;

        public SearchHistoryTests()
        {
            history = new SearchHistory();
        }

        [SetUp]
        public void Initialize()
        {
        }

        [TearDown]
        public void disposeHistory()
        {
        }

        [Test]
        public void TestMultiQueries()
        {
            for (int i = 0; i < 900; i++)
            {
                var query = GenerateRandomString(20);
                history.IssuedSearchString(query);
                Assert.IsTrue(history.GetSearchHistoryItems(item => item.SearchString.
                    Equals(query)).Any());
            }
        }
    }

    [TestFixture]
    public class InFileSearchHistoryTests : RandomStringBasedTests
    {
        private const String directory = @"C:\Windows\Temp\";
        private List<String> queries; 
        private SearchHistory history;

        public InFileSearchHistoryTests()
        {
            queries = new List<string>();
            history = new SearchHistory();
        }

        [SetUp]
        public void Initialize()
        {
            var path = Path.Combine(directory, SearchHistory.FILE_NAME);
            File.Delete(path);
            queries.Clear();
            history.Initialize(directory);
            for (var i = 0; i < 900; i++)
            {
                var query = GenerateRandomString(20);
                history.IssuedSearchString(query);
                queries.Add(query);
            }
            history.Dispose();
            history.Initialize(directory);
        }

        [Test]
        public void EnsureQueriesSavedCorrectly()
        {
            Assert.IsTrue(queries.All(q => history.GetSearchHistoryItems(item => 
                item.SearchString.Equals(q)).Any()));
        }

        [Test]
        public void EnsureTimeSavedCorrectly()
        {
            var now = DateTime.Now.Ticks;
            foreach (var query in queries)
            {
                string q = query;
                var entry = history.GetSearchHistoryItems(item => item.SearchString.Equals(q)).First();
                Assert.IsTrue(entry.TimeStamp < now);

                // The gap should be less than 1 seconds.
                Assert.IsTrue(now - entry.TimeStamp < 1*1000*10000);
            }
        }
    }

}
