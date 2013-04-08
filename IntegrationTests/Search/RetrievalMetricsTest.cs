using NUnit.Framework;
using Sando.DependencyInjection;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Metrics;
using Sando.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.IntegrationTests.Search
{
    [TestFixture]
    public class RetrievalMetricsTest : AutomaticallyIndexingTestClass
    {
        //Todo: these terms are already stemmed. To get this working for real, we need to apply a stemmer to the query (somehow)
        private static List<string> queries = new List<string>() { "sando", "search", "index", "potato chip", "solution close", "add tooltip to sando search bar" };

        [Test]
        public void TestSpecificityMetrics()
        {
            var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
            PreRetrievalMetrics preMetrics = new PreRetrievalMetrics(documentIndexer.Reader);

            foreach (var query in queries)
            {
                Console.WriteLine("for query='" + query + "', AvgIDF=" + preMetrics.AvgIdf(query));
            }
            foreach (var query in queries)
            {
                Console.WriteLine("for query='" + query + "', DevIDF=" + preMetrics.DevIdf(query));
            }
            
        }

        public override string GetFilesDirectory()
        {
            return "..\\..";
        }

        public override string GetIndexDirName()
        {
            return "RetrievalMetricsTest";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(4);
        }
    }
}
