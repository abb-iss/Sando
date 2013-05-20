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
			Assert.AreEqual(QueryMetrics.ExamineQuery("\"a\" BBB_b").ToString(), "Quoted,AcronymUnderscore");
			Assert.AreEqual(QueryMetrics.ExamineQuery("-Abb aAAAc").ToString(), "MinusCamelcase,CamelcaseAcronym");
        }

        [Test]
        public void QueryMetrics_DiceCoefficientTest()
        {           
            Assert.IsTrue(QueryMetrics.DiceCoefficient("a b","b c") - 0.50 < 0.001);
            Assert.IsTrue(QueryMetrics.DiceCoefficient("a b c d e f g h", "a b c d e f g") - 0.933 < 0.001);
            Assert.IsTrue(QueryMetrics.DiceCoefficient("\"a\" BBB_b", "BB BBB BBB_b") - 0.40 < 0.001);
            Assert.IsTrue(QueryMetrics.DiceCoefficient(String.Empty, String.Empty) == 0.0);
        }
    }
}
