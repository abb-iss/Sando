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
		[Test]
		public void ElementNameSearchesInTop3()
		{
            string keywords = "header element resolver";
		    var expectedLowestRank = 3;
			Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "CppHeaderElementResolver");
			EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}



	    [Test]
        public void TestSandoSearch()
        {
            string keywords = "test sando search";
            var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "SelfSearchTest");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void TestNoteSandoSearch()
        {
            string keywords = "-test sando search";
            var expectedLowestRank = 10;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "SelfSearchTest");
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
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "RespondToSolutionOpened");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void TestSeveralSearchesOnSandoCodeBase()
        {
            string keywords = "parse method";
            var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "ParseMethod");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "parse class";
            expectedLowestRank = 2;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "ParseClass");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "parse util";
            expectedLowestRank = 3;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "SrcMLParsingUtils");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "custom properties";
            expectedLowestRank = 2;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "GetCustomProperties");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "analyze type";
            expectedLowestRank = 7;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Enum && (el.Element.Name == "AnalyzerType");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "ParserException";
            expectedLowestRank = 1;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "ParserException");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "word extract";
            expectedLowestRank = 1;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "ExtractWords");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "translation get";
            expectedLowestRank = 3;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "GetTranslation");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "register extension points";
            expectedLowestRank = 12;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "RegisterExtensionPoints");
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
            return TimeSpan.FromSeconds(10);
        }
        

	}
}
