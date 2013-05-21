using NUnit.Framework;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.SearchEngine.UnitTests
{
    [TestFixture]
    public class CodeSearchResultTest
    {
        [TestCase]
        public void FixSnipTabTest()
        {
            var stuff = "	public void yo()"+
                        "\n\t\tsasdfsadf"+
                        "\n\t\tasdfasdf\n";
            string fixSnip = CodeSearchResult.SourceToSnippet(stuff, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("public void yo()"+
                                         "\n    sasdfsadf"+
                                         "\n    asdfasdf\n"));
        }

        [TestCase]
        public void FixSnipSpacesTest()
        {
            var stuff = "      public void yo()"+
                        "\n            sasdfsadf"+
                        "\n            asdfasdf\n";
			string fixSnip = CodeSearchResult.SourceToSnippet(stuff, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("public void yo()\n      sasdfsadf\n      asdfasdf\n"));
        }


        [TestCase]
        public void FixSnipSpacesTwoTest()
        {
            var stuff = "\t\tpublic void yo()" +
                        "\n\t\t{" +
                        "\n\t\t\tasdfasdf\n";
            string fixSnip = CodeSearchResult.SourceToSnippet(stuff, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("public void yo()\n{\n    asdfasdf\n"));
        }

        [TestCase]
        public void FixNoSpaceOnFirst()
        {
            var stuff = "public void yo()" +
                        "\n\t\t{" +
                        "\n\t\t\tasdfasdf\n";
            string fixSnip = CodeSearchResult.SourceToSnippet(stuff, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("public void yo()\n{\n    asdfasdf\n"));
        }

        //List<Monster> monsterlist;

        [TestCase]
        public void FixFieldsSpace()
        {
            var stuff = "List<Monster> monsterlist;";
            string fixSnip = CodeSearchResult.SourceToSnippet(stuff, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("List<Monster> monsterlist;\n"));
        }

private string input = 
"        public virtual List<CodeSearchResult> Search(string searchString, bool rerunWithWildcardIfNoResults = false)\n"+
"		{\n"+
"			Contract.Requires(String.IsNullOrWhiteSpace(searchString), \"CodeSearcher:Search - searchString cannot be null or an empty string!\");\n"+
"            var searchCriteria = CriteriaBuilder.GetBuilder().AddSearchString(searchString).GetCriteria();";


        [TestCase]
        public void FixSpacesDifferentCase()
        {            
            string fixSnip = CodeSearchResult.SourceToSnippet(input, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("public virtual List<CodeSearchResult> Search(string searchString, bool rerunWithWildc...\n"+
                "{\n"+
"    Contract.Requires(String.IsNullOrWhiteSpace(searchString), \"CodeSearcher:Search -...\n"+
"    var searchCriteria = CriteriaBuilder.GetBuilder().AddSearchString(searchString).G...\n"
                ));
        }


        private string tab =
@"        protected virtual void Layout(bool continueLayout)
        {
            if (Graph == null || Graph.VertexCount == 0 || !LayoutAlgorithmFactory.IsValidAlgorithm(LayoutAlgorithmType) || !CanLayout)
                return; //no graph to layout, or wrong layout algorithm
";

        [TestCase]
        public void FixAnotherWeirdCase()
        {
            string fixSnip = CodeSearchResult.SourceToSnippet(tab, CodeSearchResult.DefaultSnippetSize);
            Assert.IsTrue(fixSnip.Equals("protected virtual void Layout(bool continueLayout)\r\n" +
                "{\r\n" +
"    if (Graph == null || Graph.VertexCount == 0 || !LayoutAlgorithmFactory.IsValidAlg...\n" +
"        return; //no graph to layout, or wrong layout algorithm\r\n"
                ));
        }


    }
}
