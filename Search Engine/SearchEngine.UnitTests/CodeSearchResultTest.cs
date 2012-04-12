using NUnit.Framework;

namespace Sando.SearchEngine.UnitTests
{
    [TestFixture]
    public class CodeSearchResultTest
    {
        [TestCase]
        public void FixSnipTabTest()
        {
            var stuff = "	public void yo()\n		sasdfsadf\n		asdfasdf\n";
            string fixSnip = CodeSearchResult.FixSnip(stuff);
            Assert.IsTrue(fixSnip.Equals("public void yo()\n\tsasdfsadf\n\tasdfasdf\n"));
        }

        [TestCase]
        public void FixSnipSpacesTest()
        {
            var stuff = "      public void yo()\n            sasdfsadf\n            asdfasdf\n";
            string fixSnip = CodeSearchResult.FixSnip(stuff);
            Assert.IsTrue(fixSnip.Equals("public void yo()\n      sasdfsadf\n      asdfasdf\n"));
        }
    }
}
