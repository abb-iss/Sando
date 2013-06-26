using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class ToolHelpersTests
    {
        private class Data
        {
            internal Data(string text, int iNumber, float fNumber)
            {
                this.text = text;
                this.iNumber = iNumber;
                this.fNumber = fNumber;
            }
            public string text { set; get; }
            public int iNumber { get; set; }
            public float fNumber { get; set; }
        }

        [Test]
        public void TestDistinctBy()
        {
            var d1 = new Data("a", 2, 2.3f);
            var d2 = new Data("a", 3, 3.4f);
            var d3 = new Data("a", 4, 1.1f);
            var list = new[] {d1, d2, d3};
            list = list.DistinctBy(d => d.text).ToArray();
            Assert.IsTrue(list.Count() == 1);

            d1 = new Data("a", 1, 3.2f);
            d2 = new Data("b", 1, 2.2f);
            d3 = new Data("c", 1, 12.2f);
            list = new[] {d1, d2, d3};
            list = list.DistinctBy(d => d.iNumber).ToArray();
            Assert.IsTrue(list.Length == 1);

            d1 = new Data("a", 1, 3.2f);
            d2 = new Data("b", 2, 2.2f);
            d3 = new Data("c", 1, 12.2f);
            list = new[] { d1, d2, d3 };
            list = list.DistinctBy(d => d.iNumber).ToArray();
            Assert.IsTrue(list.Length == 2);
        }
    }
}
