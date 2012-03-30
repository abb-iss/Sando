using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Sando.SearchEngine.UnitTests
{
    [TestFixture]
    public class CodeSearchResultTest
    {
        [TestCase]
        public void FixSnipTest()
        {
            var stuff = "	public void yo()\n		sasdfsadf\n		asdfasdf";
            Assert.IsTrue(CodeSearchResult.FixSnip(stuff).Equals("public void yo()\n	sasdfsadf\n	asdfasdf"));
        }
    }
}
