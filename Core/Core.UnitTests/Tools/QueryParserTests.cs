using NUnit.Framework;
using Sando.Core.Tools;

namespace Sando.Core.UnitTests.Tools
{
    [TestFixture]
    public class QueryParserTests
    {
        [Test]
        [TestCaseSource("InvalidQueryTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsNullOrEmptyStringOrContainsWhiteSpaceOnly_THAN_ValidQueryDescriptionIsReturned(string query)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(null);

            Assert.IsFalse(sandoQueryDescription.IsValid);
        }

        [Test]
        [TestCaseSource("ValidLocationFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsLocationString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidLiteralQueryTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsLiteralString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        public void ParseFileH()
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse("open file:h");

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.IsTrue(sandoQueryDescription.SearchTerms.Count == 1);
        }

        
        [Test]        
        public void ParseWithNegation()
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse("reorder search results -test");

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.IsTrue(sandoQueryDescription.SearchTerms.Count == 4);
        }


        [Test]
        [TestCaseSource("ValidFileExtensionFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsFileExtensionString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidProgramElementTypeFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsProgramElementTypeString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidAccessLevelFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsAccessLevelString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidNormalQueryTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsNormalString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }


        private static readonly object[] InvalidQueryTestCases =
            {
                "",
                " ",
                "\t",
                "\n",
                "\t  \n",
                "\"  \""
            };

        private static readonly object[] ValidLocationFiltersTestCases =
            {
                new object[]{"location:\"C:\\Temp\\Sando\"",                            "Locations:[\"C:\\Temp\\Sando\"]"},
                new object[]{"location:D:\\Windows",                                    "Locations:[D:\\Windows]"},
                new object[]{"location:\"C:\\Program Files\\Adobe\\*\"",                "Locations:[\"C:\\Program Files\\Adobe\\*\"]"},
                new object[]{"location:D*",                                             "Locations:[D*]"},
                new object[]{"location:\"C:\\Program Files*\"",                         "Locations:[\"C:\\Program Files*\"]"},
                new object[]{"location:C:\\Adobe\\Photoshop(8) location:D",             "Locations:[C:\\Adobe\\Photoshop(8),D]"},
                new object[]{"location:\"C:\\*\\Adobe\" location:\"D:\\Photos\\*\"",    "Locations:[\"C:\\*\\Adobe\",\"D:\\Photos\\*\"]"}
            };
         
        private static readonly object[] ValidLiteralQueryTestCases =
            {
                new object[]{"\"asd^&*#@$%!()_+-=:';[]{}?/><,.żźćąśęńłó\"",             "Literal search terms:[\"asd^&*#@$%!()_+-=:';[]{}?/><,.żźćąśęńłó\"]"},
                new object[]{"\"sample literal search\"",                               "Literal search terms:[\"sample literal search\"]"},
                new object[]{"\"sample literal search with escaped quote \\\"\"",       "Literal search terms:[\"sample literal search with escaped quote \\\"\"]"},
                new object[]{"\"\\\"\"",                                                "Literal search terms:[\"\\\"\"]"},
                new object[]{"\"var sandoQueryParser = new SandoQueryParser();\"",      "Literal search terms:[\"var sandoQueryParser = new SandoQueryParser();\"]"},
                new object[]{"\"var sandoQueryParser\" \"new SandoQueryParser();\"",    "Literal search terms:[\"var sandoQueryParser\",\"new SandoQueryParser();\"]"},
                new object[]{"\"name \\\"\" \" sandoQueryParser\" \"new Sando\"",       "Literal search terms:[\"name \\\"\",\" sandoQueryParser\",\"new Sando\"]"},
                new object[]{"-\"name\" \" \t\n \"",                                    "Literal search terms:[-\"name\"]"},
                new object[]{"-\"name \\\"\" \" -sandoQueryParser\" -\"new Sando\"",    "Literal search terms:[-\"name \\\"\",\" -sandoQueryParser\",-\"new Sando\"]"}
            };

        private static readonly object[] ValidFileExtensionFiltersTestCases =
            {
                new object[]{"file:cs",                 "File extensions:[cs]"},
                new object[]{"file:long_Entension",     "File extensions:[long_entension]"},
                new object[]{"-file:.mp3",               "File extensions:[-mp3]"},
                new object[]{"file:txt -file:.DAT",      "File extensions:[txt,-dat]"}
            };

        private static readonly object[] ValidProgramElementTypeFiltersTestCases =
            {
                new object[]{"type:Class",                  "Program element types:[class]"},
                new object[]{"type:method",                 "Program element types:[method]"},
                new object[]{"-type:ENUM type:field",       "Program element types:[-enum,field]"},
                new object[]{"type:property -type:struct",  "Program element types:[property,-struct]"}
            };

        private static readonly object[] ValidAccessLevelFiltersTestCases =
            {
                new object[]{"access:public",                       "Access levels:[public]"},
                new object[]{"access:private",                      "Access levels:[private]"},
                new object[]{"-access:Protected access:internal",   "Access levels:[-protected,internal]"},
                new object[]{"access:protected -access:puBLIc",     "Access levels:[protected,-public]"}
            };

        private static readonly object[] ValidNormalQueryTestCases =
            {
                new object[]{"identifier",                          "Search terms:[identifier]"},
                new object[]{"open*file",                           "Search terms:[open*,file]"},
                new object[]{"do something special",                "Search terms:[do,something,special]"},
                new object[]{"every*Single*Word",                   "Search terms:[every*,Single*,Word]"}
            };
    }
}