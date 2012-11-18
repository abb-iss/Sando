using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sando.Core;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using UnitTestHelpers;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class SelfSearchTest
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
        public void TestSolutionMonitor()
        {
            string keywords = "solution monitor";
            var expectedLowestRank = 3;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "SolutionMonitor");
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
            expectedLowestRank = 4;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Enum && (el.Element.Name == "AnalyzerType");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "ParserException";
            expectedLowestRank = 1;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "ParserException");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "Source Namespace";
            expectedLowestRank = 1;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Field && (el.Element.Name == "SourceNamespace");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "word extract";
            expectedLowestRank = 1;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "ExtractWords");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "translation get";
            expectedLowestRank = 2;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "GetTranslation");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            keywords = "register extension points";
            expectedLowestRank = 7;
            predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "RegisterExtensionPoints");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);            
        }



		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
		}

		[SetUp]
		public void Setup()
		{
			indexPath = Path.Combine(Path.GetTempPath(), "SelfSearchTest");
			Directory.CreateDirectory(indexPath);
			key = new SolutionKey(Guid.NewGuid(), "..\\..", indexPath);
			var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
			monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer, false);

			//not an exaustive list, so it will be a bit of a messy parse
			sandoDirsToAvoid = new List<String>() { "LIBS", ".hg", "bin", "obj" };

			string startingPath = "..\\..";
			string[] dirs = Directory.GetDirectories(startingPath);
			ProcessDirectoryForTesting(dirs);

			monitor.UpdateAfterAdditions();
		}

		private void ProcessDirectoryForTesting(string[] dirs)
		{
			foreach (var dir in dirs)
			{
				if (sandoDirsToAvoid.Contains(Path.GetFileName(dir))) continue;

				string[] subdirs = Directory.GetDirectories(dir);
				ProcessDirectoryForTesting(subdirs);

				string[] files = Directory.GetFiles(dir);
				foreach (var file in files)
				{ 
					string fullPath = Path.GetFullPath(file);
					if (Path.GetExtension(fullPath) == ".cs")
					{
						monitor.ProcessFileForTesting(fullPath);
					}
				}
			}
		}

		[TearDown]
		public void TearDown()
		{
			monitor.StopMonitoring(true);
			Directory.Delete(indexPath, true);
		}

        private static void EnsureRankingPrettyGood(string keywords, Predicate<CodeSearchResult> predicate, int expectedLowestRank)
        {
            var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
            var methodSearchResult = codeSearchResults.Find(predicate);
            if (methodSearchResult == null)
            {
                Assert.Fail("Failed to find relevant search result for search: " + keywords);
            }

            var rank = codeSearchResults.IndexOf(methodSearchResult) + 1;
            Assert.IsTrue(rank <= expectedLowestRank,
                          "Searching for " + keywords + " doesn't return a result in the top " + expectedLowestRank + "; rank=" +
                          rank);
        }

		private string indexPath;
		private static SolutionMonitor monitor;
		private static SolutionKey key;
		private static List<string> sandoDirsToAvoid;
	}
}
