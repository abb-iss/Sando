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
    class AcronymExpanderTests
    {
        private IWordCoOccurrenceMatrix matrix;
        private AcronymExpander expander;

        public AcronymExpanderTests()
        {
            this.matrix = new InternalWordCoOccurrenceMatrix();
            this.expander = new AcronymExpander(matrix);
        }

        [SetUp]
        public void ReadData()
        {
            matrix.Initialize(@"TestFiles\");
        }

        [Test]
        public void ExpandThreeLetters()
        {
            var queries = expander.GetExpandedQueries("abc");
            Assert.IsNotNull(queries);
            queries = expander.GetExpandedQueries("bcd");
            Assert.IsNotNull(queries);
            queries = expander.GetExpandedQueries("def");
            Assert.IsNotNull(queries);
        }

        [Test]
        public void ExpandTwoLetters()
        {
            var queries = expander.GetExpandedQueries("ab");
            Assert.IsTrue(queries.Any());
        }
    }
}
