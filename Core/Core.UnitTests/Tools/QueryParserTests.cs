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
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsLocationFilters_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidLiteralQueryTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsLiteralSearchTerms_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidFileExtensionFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsFileExtensionFilters_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidProgramElementTypeFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsProgramElementTypeFilters_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidAccessLevelFiltersTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsAccessLevelFilters_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("ValidNormalQueryTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsNormalSearchTerms_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
        {
            var sandoQueryParser = new SandoQueryParser();
            var sandoQueryDescription = sandoQueryParser.Parse(query);

            Assert.IsTrue(sandoQueryDescription.IsValid);
            Assert.AreEqual(expectedQueryDescription, sandoQueryDescription.ToString());
        }

        [Test]
        [TestCaseSource("DifferentQueryTypesTestCases")]
        public void GIVEN_QueryParser_WHEN_ParseIsCalled_AND_QueryContainsDifferentQueryTypes_THAN_ValidQueryDescriptionIsReturned(string query, string expectedQueryDescription)
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
                new object[]{"-location:D*",                                            "Locations:[-D*]"},
                new object[]{"location:\"C:\\Program Files*\"",                         "Locations:[\"C:\\Program Files*\"]"},
                new object[]{"location:C:\\Adobe\\Photoshop(8) location:D",             "Locations:[C:\\Adobe\\Photoshop(8),D]"},
                new object[]{"location:\"C:\\*\\Adobe\" -location:\"D:\\Photos\\*\"",   "Locations:[\"C:\\*\\Adobe\",-\"D:\\Photos\\*\"]"},
                new object[]{"location:\"C\" location:\"D:\\Photos\"",                  "Locations:[\"C\",\"D:\\Photos\"]"}
            };
         
        private static readonly object[] ValidLiteralQueryTestCases =
            {
                new object[]{"\"asd^&*#@$%!()_+-=:';[]{}?/><,.żźćąśęńłó\"",             "Literal search terms:[\"asd^&*#@$%!()_+-=:';[]{}?/><,.żźćąśęńłó\"]"},
                new object[]{"\"sample literal search \"",                               "Literal search terms:[\"sample literal search\"]"},
                new object[]{"\"sample literal search with escaped quote \\\"\"",       "Literal search terms:[\"sample literal search with escaped quote\\\"\"]"},
                new object[]{"\"\\\"\"",                                                "Literal search terms:[\"\\\"\"]"},
                new object[]{"\"var sandoQueryParser = new SandoQueryParser();\"",      "Literal search terms:[\"var sandoQueryParser = new SandoQueryParser();\"]"},
                new object[]{"\"var sandoQueryParser\" \"new SandoQueryParser();\"",    "Literal search terms:[\"new SandoQueryParser();\",\"var sandoQueryParser\"]"},
                new object[]{"\"name \\\"\" \" sandoQueryParser\" \"new Sando\"",       "Literal search terms:[\"name\\\"\",\"new Sando\",\"sandoQueryParser\"]"},
                new object[]{"-\"name\" \" \t\n \"",                                    "Literal search terms:[-\"name\"]"},
                new object[]{"-\"name \\\"\" \" -sandoQueryParser\" -\"new Sando\"",    "Literal search terms:[-\"name\\\"\",-\"new Sando\",\"-sandoQueryParser\"]"}
            };

        private static readonly object[] ValidFileExtensionFiltersTestCases =
            {
                new object[]{"file:cs",                 "File extensions:[cs]"},
                new object[]{"file:long_Entension",     "File extensions:[long_entension]"},
                new object[]{"-file:.mp3",               "File extensions:[-mp3]"},
                new object[]{"file:txt -file:.DAT",      "File extensions:[-dat,txt]"}
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
                new object[]{"-access:Protected access:internal",   "Access levels:[internal,-protected]"},
                new object[]{"access:protected -access:puBLIc",     "Access levels:[protected,-public]"}
            };

        private static readonly object[] ValidNormalQueryTestCases =
            {
                new object[]{"identifier",                          "Search terms:[identifier]"},
                new object[]{"file*manager",                        "Search terms:[file*manager]"},
                new object[]{"do something -special",               "Search terms:[do,something,-special]"},
                new object[]{"every*Single*Word",                   "Search terms:[every*Single*Word]"}
            };

        private static readonly object[] DifferentQueryTypesTestCases =
            {
                new object[]{"open \" var isEmpty = repository.CheckIfEmpty()\"",
                    "Literal search terms:[\"var isEmpty = repository.CheckIfEmpty()\"], Search terms:[open]"},
                new object[]{"location file:dat -location:sando* type:method -type:enum",
                    "Locations:[-sando*], File extensions:[dat], Program element types:[-enum,method], Search terms:[location]"},
                new object[]{"access:public -access:sando",
                    "Access levels:[public], Search terms:[-access,sando]"},
                new object[]{"get stores type:meTHod access:Public -Repository",
                    "Program element types:[method], Access levels:[public], Search terms:[get,-Repository,stores]"},
                new object[]{"filter:values \" location:C \"",
                    "Literal search terms:[\"location:C\"], Search terms:[filter,values]"},
                new object[]{"mylocation:C mylocation:\"D:\\Photos\" myaccess:public mytype:method myfile:cs",
                    "Literal search terms:[\"D:\\Photos\"], Search terms:[C,cs,method,myaccess,myfile,mylocation,mytype,public]"},
                new object[]{"\"location:C\" \"access:public\" \"type:method\" \"file:cs\"",
                    "Literal search terms:[\"access:public\",\"file:cs\",\"location:C\",\"type:method\"]"},
                new object[]{"\" \" \"al-location:F\" file:cs\" something",
                    "Literal search terms:[\"al-location:F\"], File extensions:[cs], Search terms:[something]"},
                new object[]{"wild****card testing",
                    "Search terms:[testing,wild*card]"},
                new object[]{"\" var wildcard=\\\"\" some thing",
                    "Literal search terms:[\"var wildcard=\\\"\"], Search terms:[some,thing]"},
                new object[]{"location:file:cs access:publication type:methodical",
                    "Locations:[file:cs], Search terms:[access,methodical,publication,type]"}
            };
    }
}