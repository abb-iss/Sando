using NUnit.Framework;
using Sando.Indexer.Searching.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
    public class QueryMetricsTest
    {
        [Test]
        public void QueryMetrics_QueryTypeTest()
        {
            Assert.AreEqual(QueryMetrics.ExamineQuery("a b").ToString(), "Plain,Plain");
            Assert.AreEqual(QueryMetrics.ExamineQuery("\"a b c\"").ToString(), "Quoted,Quoted,Quoted");
            Assert.AreEqual(QueryMetrics.ExamineQuery("a_aAa b").ToString(), "CamelcaseUnderscore,Plain");
            Assert.AreEqual(QueryMetrics.ExamineQuery("\" a\" bBb").ToString(), "Quoted,Camelcase");
            Assert.AreEqual(QueryMetrics.ExamineQuery("a\" \"b").ToString(), "Plain,Plain");
            Assert.AreEqual(QueryMetrics.ExamineQuery("a\" B_b_b_B \"c").ToString(), "Plain,QuotedUnderscore,Plain");
            Assert.AreEqual(QueryMetrics.ExamineQuery("\"a\" BBB_b").ToString(), "Quoted,UnderscoreAcronym");
        }
    }
}
