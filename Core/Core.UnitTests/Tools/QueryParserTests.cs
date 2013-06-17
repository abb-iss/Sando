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
        [TestCaseSource("ValidLiteralQueryTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryIsLiteralString_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
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


        private static readonly object[] InvalidQueryTestCases =
            {
                "",
                " ",
                "\t",
                "\n",
                "\t  \n",
                "\"  \""
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
                new object[]{"-\"name \\\"\" \" -sandoQueryParser\" -\"new Sando\"",    "Literal search terms:[-\"name \\\"\",\" -sandoQueryParser\",-\"new Sando\"]"},
                new object[]{"\"open\" file\"",                                         "Literal search terms:[\"open\"]"}
            };

        private static readonly object[] ValidFileExtensionFiltersTestCases =
            {
                new object[]{"file:cs",                 "File extensions:[cs]"},
                new object[]{"file:long_entension",     "File extensions:[long_entension]"},
                new object[]{"-file:mp3",               "File extensions:[-mp3]"},
                new object[]{"file:txt -file:dat",      "File extensions:[txt,-dat]"}
            };

        private static readonly object[] ValidProgramElementTypeFiltersTestCases =
            {
                new object[]{"type:class",                  "Program element types:[class]"},
                new object[]{"type:method",                 "Program element types:[method]"},
                new object[]{"-type:enum type:field",       "Program element types:[-enum,field]"},
                new object[]{"type:property -type:struct",  "Program element types:[property,-struct]"}
            };

        private static readonly object[] ValidAccessLevelFiltersTestCases =
            {
                new object[]{"access:public",                       "Access levels:[public]"},
                new object[]{"access:private",                      "Access levels:[private]"},
                new object[]{"-access:protected access:internal",   "Access levels:[-protected,internal]"},
                new object[]{"access:protected -access:public",     "Access levels:[protected,-public]"}
            };
    }
}