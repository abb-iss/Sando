using Lucene.Net.Analysis;
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
        [Test]
        public void TestStemmingForMetricsCalculation()
        {
            var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
            var analyzer = ServiceLocator.Resolve<Analyzer>();

            PreRetrievalMetrics preMetrics = new PreRetrievalMetrics(documentIndexer.Reader, analyzer);
            Assert.AreEqual(preMetrics.StemText("searching"), "search");
            Assert.AreEqual(preMetrics.StemText("search"), "search");
            Assert.AreEqual(preMetrics.StemText("solution closing"), "solut close");
            Assert.AreEqual(preMetrics.StemText("indexer"), "index");
        }

        [Test]
        public void TestSpecificityMetrics()
        {
            var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
            var analyzer = ServiceLocator.Resolve<Analyzer>();

            PreRetrievalMetrics preMetrics = new PreRetrievalMetrics(documentIndexer.Reader, analyzer);
            Assert.IsTrue(preMetrics.AvgIdf("splitting") > preMetrics.AvgIdf("searching"));
            Assert.IsTrue(preMetrics.AvgIdf("solution closing") > preMetrics.AvgIdf("indexer"));
        }

        [Test]
        public void TestSimilarityMetrics()
        {
            var documentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
            var analyzer = ServiceLocator.Resolve<Analyzer>();

            PreRetrievalMetrics preMetrics = new PreRetrievalMetrics(documentIndexer.Reader, analyzer);
            Assert.IsTrue(preMetrics.AvgSqc("indexer") > preMetrics.AvgSqc("potato chip"));
            Assert.IsTrue(preMetrics.AvgSqc("indexer") > preMetrics.AvgSqc("soda pop"));
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
