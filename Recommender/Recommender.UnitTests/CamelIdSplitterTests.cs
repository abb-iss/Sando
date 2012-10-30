using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.Recommender;

namespace Sando.Recommender.UnitTests {
    [TestFixture]
    public class CamelIdSplitterTests {
        [Test]
        public void SplitTest() {
            var splitter = new CamelIdSplitter();
            var actual = splitter.Split("DBGetHydro");
            var expected = new[] {"DB", "Get", "Hydro"};
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SplitTest_lowercase() {
            var splitter = new CamelIdSplitter();
            var actual = splitter.Split("lowercase");
            var expected = new[] { "lowercase" };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SplitTest_uppercase() {
            var splitter = new CamelIdSplitter();
            var actual = splitter.Split("CONSTVAL");
            var expected = new[] { "CONSTVAL" };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SplitTest_UppercaseUnderscore() {
            var splitter = new CamelIdSplitter();
            var actual = splitter.Split("CONST_VAL");
            var expected = new[] { "CONST", "VAL" };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SplitTest_BadCamelCase() {
            var splitter = new CamelIdSplitter();
            var actual = splitter.Split("XMLparser");
            var expected = new[] { "XM", "Lparser" };
            Assert.AreEqual(expected, actual);
        }
    }
}
