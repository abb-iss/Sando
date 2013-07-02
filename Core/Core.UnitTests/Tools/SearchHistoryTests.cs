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
        private const String directory = @"C:\Windows\Temp\";
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
            for (int i = 0; i < 1000; i++)
            {
                var query = GenerateRandomString(20);
                history.IssuedSearchString(query);
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
            history.Initiatalize(directory);
            for (var i = 0; i < 1000; i++)
            {
                var query = GenerateRandomString(20);
                history.IssuedSearchString(query);
                queries.Add(query);
            }
            history.Dispose();
            history.Initiatalize(directory);
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

                // The gap should be less than 10 seconds.
                Assert.IsTrue(now - entry.TimeStamp < 10*1000*1000);
            }
        }
    }

}
