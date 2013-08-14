using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
    public class QueryParsingAndConvertingTests
    {

        [Test]
        public void TestIfQueryParsesToEmptySearchTerm()
        {
            var description = new SandoQueryParser().Parse("g_u16ActiveFault");
            var builder = CriteriaBuilder.GetBuilder().AddFromDescription(description);
            var simple = builder.GetCriteria() as SimpleSearchCriteria;
            Assert.IsFalse(simple.SearchTerms.Where(x => String.IsNullOrWhiteSpace(x)).ToList().Count >= 1);
        }
    }
}
