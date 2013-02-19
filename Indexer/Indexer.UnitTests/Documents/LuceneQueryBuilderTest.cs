using NUnit.Framework;
using Sando.Indexer.Searching.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTestHelpers;

namespace Sando.Indexer.UnitTests.Documents
{
    [TestFixture]
    public class LuceneQueryBuilderTest
    {

        [Test]
        public void LuceneQueryBuilderTest_LocationBuilding()
        {
            TestUtils.InitializeDefaultExtensionPoints();
            SimpleSearchCriteria criteria = new SimpleSearchCriteria();
            criteria.Locations.Add("C:\\open\\file");
            criteria.SearchByLocation = true;
            var x = criteria.ToQueryString();
            Assert.IsTrue(x.Contains("C:\\\\open\\\\file"));
        }

    }
}
