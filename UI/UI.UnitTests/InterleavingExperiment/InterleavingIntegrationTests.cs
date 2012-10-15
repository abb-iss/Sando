using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sando.Core;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using UnitTestHelpers;

namespace Sando.UI.UnitTests.InterleavingExperiment
{
	[TestFixture]
	public class InterleavingIntegrationTests
	{
		[Test]
		public void ElementNameSearchesInTop3()
		{
			string keywords = "class document";
			var expectedLowestRank = 3;
			Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "ClassDocument");
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
			foreach(var dir in dirs)
			{
				if(sandoDirsToAvoid.Contains(Path.GetFileName(dir))) continue;

				string[] subdirs = Directory.GetDirectories(dir);
				ProcessDirectoryForTesting(subdirs);

				string[] files = Directory.GetFiles(dir);
				foreach(var file in files)
				{
					string fullPath = Path.GetFullPath(file);
					if(Path.GetExtension(fullPath) == ".cs")
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
			if(methodSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}

			var rank = codeSearchResults.IndexOf(methodSearchResult);
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
