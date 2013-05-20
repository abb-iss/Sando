using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using NUnit.Framework;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using UnitTestHelpers;
using Sando.Recommender;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class SelfSearchTest : AutomaticallyIndexingTestClass
	{

        //document add field should find CustomFieldTest.GetLuceneDocument near top
        [Test]
        public void FindBodyText()
        {
            string keywords = "document add";
            var expectedLowestRank = 5;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "GetLuceneDocument");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }


        [Test]
        public void QuotedSearchNoResults()
        {
            string keywords = "\"frigging"+"NoResultsMahn\"";
            var expectedLowestRank = 0;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "CppHeaderElementResolver");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            Assert.IsTrue(_myMessage.Contains("No results"));
        }

        [Test]
        public void QuotedNoQuotesWithQuotesInside()
        {
            string keywords = "\"return \"..\\..\\Parser\";\"";
            var expectedLowestRank = 5;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "GetFilesDirectory");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);            
        }

        [Test]
        public void QuotedSearchWithNot()
        {
            //NOT supported as of now.  Only pure literal searches are supported now,
            //meaning you cannot combine literal searches with anything else.
            string keywords = "\"Search(\" -test";
            var expectedLowestRank = 2;

            //Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "Search");
            //EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void QuotedSearchBroken()
        {            
            string keywords = "\"ServiceLocator.Resolve<DTE2>();\"";
            var expectedLowestRank = 10;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "InitDte2");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);            
        }

        [Test]
        public void QuotedWithoutQuotes()
        {
            string keywords = "ServiceLocator.Resolve<DTE2>();";
            var expectedLowestRank = 10;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "InitDte2");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void QuotedSearchWithSpacesBroken()
        {
            string keywords = "\"foreach (var term in SearchTerms)\"";
            var expectedLowestRank = 10;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "RequiresWildcards");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void ExcludeTestIfClassNameHasTest()
        {
            string keywords = "reorder search results -test";
            var expectedLowestRank = 20;
            try{
                Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "ReorderSearchResults") && (el.ProgramElement.FullFilePath.Contains("Test"));
                EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);                
            }catch(Exception e){
                //expected
                return;
            }
            Assert.IsTrue(false, "Should fail to find this method");
        }        

		[Test]
		public void ElementNameSearchesInTop3()
		{
            string keywords = "header element resolver cpp";
		    var expectedLowestRank = 3;
			Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "CppHeaderElementResolver");
			EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}

        [Test]
        public void UnderscoreSearch()
        {
            string keywords = "solutionEvents";
            var expectedLowestRank = 6;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Field && (el.ProgramElement.Name == "_solutionEvents");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "_solutionEvents";
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Field && (el.ProgramElement.Name == "_solutionEvents");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);

        }

        [Test]
        public void FileTypeWithTerm()
        {
            string keywords = "hello world filetype:cpp";
            var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "MyFunction");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);                
        }

        [Test]
        public void FileTypeH()
        {
            string keywords = "session file info filetype:h";
            var expectedLowestRank = 3;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Struct && (el.ProgramElement.Name == "sessionFileInfo");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void FileTypeSearch()
        {
            string keywords = "XmlMatchedTagsHighlighter filetype:cs";
            var expectedLowestRank = 10;
            try
            {
                Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "getXmlMatchedTagsPos");
                EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
                Assert.IsTrue(false, "Should never reach this point. If it does, then it is finding a .cpp file when searching for only cs files");
            }
            catch (Exception e)
            {
                //expected to fail
            }
        }

	    [Test]
        public void TestSandoSearch()
        {
            string keywords = "test sando search";
            var expectedLowestRank = 3;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "SelfSearchTest");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void TestNoteSandoSearch()
        {
            string keywords = "-test sando search";
            var expectedLowestRank = 10;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "SelfSearchTest");
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
            var methodSearchResult = codeSearchResults.Find(predicate);
            if (methodSearchResult != null)
            {
                Assert.Fail("Should not find anything that matches for this test: " + keywords);
            }
        }


        [Test]
        public void TestSolutionOpened()
        {
            string keywords = "RespondToSolutionOpened";
            var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "RespondToSolutionOpened");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void TestSeveralSearchesOnSandoCodeBase()
        {
            string keywords = "parse method";
            var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "ParseMethod");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "parse class";
            expectedLowestRank = 2;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "ParseClass");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "parse util";
            expectedLowestRank = 3;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "SrcMLParsingUtils");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "custom properties";
            expectedLowestRank = 2;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "GetCustomProperties");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            //keywords = "access level";
            //expectedLowestRank = 7;
            //predicate = el => el.Element.ProgramElementType == ProgramElementType.Enum && (el.Element.Name == "AccessLevel");
            //EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "ParserException";
            expectedLowestRank = 2;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "ParserException");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "word extract";
            expectedLowestRank = 1;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "ExtractWords");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "translation get";
            expectedLowestRank = 3;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "GetTranslation");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "RegisterExtensionPoints";
            expectedLowestRank = 3;
            predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "RegisterExtensionPoints");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);            
        }


        public override string GetIndexDirName()
        {
            return "SelfSearchTest";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(4);
        }
        

	}
}
