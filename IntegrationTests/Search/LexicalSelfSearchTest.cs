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
	public class LexicalSelfSearchTest
	{
		[Test]
		public void ExactLexMatchSearch1()
		{
            string keywords = "LexicalSelfSearchTest";
		    var expectedLowestRank = 1;
			Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "LexicalSelfSearchTest");
			EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}

        [Test]
        public void ExactLexMatchSearch2()
        {
            string keywords = "lexicalselfsearchtest";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "LexicalSelfSearchTest");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void ExactLexMatchSearch3()
        {
            string keywords = "LexicalSelf";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "LexicalSelfSearchTest");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

		/*
        private void EyeIsBullsThis() { 
		  //bulls eye bulls eye bulls eye is here
		}
        private void ThisIsBullsEye() { }
        private void ThisIsBulls() { }
      
        [Test]
        public void PreferenceForWordOrderTest()
        {
            string keywords = "ThisIsBullsEye";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "ThisIsBullsEye");
            List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }
		*/

        private void PumpkinSpiceLatte() { }
        private void Pumpkin() { }
        private void LattePumpkinSpice() { }

        [Test]
        public void PreferenceForShortestMatch()
        {
            string keywords = "Pumpkin";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "Pumpkin");
            List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
            Assert.IsTrue(results.Count >= 3);
            Assert.IsTrue(results[0].Score > results[1].Score);
            Assert.IsTrue(results[0].Score > results[2].Score);
        }

/*
		private class CaramelMacchiato 
		{ 
			//caramel is sweet
		}

		[Test]
		public void PreferenceForMethodsAndClasses()
		{
			string keywords = "caramel";
			var expectedLowestRank = 1;
			Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "CaramelMacchiato");
			List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
			CodeSearchResult classResult = results.Find(predicate);
			Assert.IsTrue((classResult.Element as ClassElement).Body.Contains("sweet"));
		}

        [Test]
        public void IncompleteTermTest()
        {
            string keywords = "caram";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Class && (el.Element.Name == "CaramelMacchiato");
            List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }
*/

		[Test]
		public void QuotedExactQueryTest()
		{
            string keywords = "\"private static void InitDte2()\"";
            var expectedLowestRank = 3;
            Predicate<CodeSearchResult> predicate = el => el.Element.ProgramElementType == ProgramElementType.Method && (el.Element.Name == "InitDte2");
            List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}

        [TestFixtureSetUp]
        public void Setup()
        {
            TestUtils.InitializeDefaultExtensionPoints();
            indexPath = Path.Combine(Path.GetTempPath(), "SelfSearchTest");
            Directory.CreateDirectory(indexPath);
            key = new SolutionKey(Guid.NewGuid(), "..\\..", indexPath, indexPath);
            ServiceLocator.RegisterInstance(key);

            ServiceLocator.RegisterInstance<Analyzer>(new SnowballAnalyzer("English"));

            var indexer = new DocumentIndexer();
            ServiceLocator.RegisterInstance(indexer);

            monitor = new SolutionMonitor(new SolutionWrapper(), indexer, false);

            SwumManager.Instance.Initialize(key.IndexPath, true);
            SwumManager.Instance.Generator = new ABB.SrcML.SrcMLGenerator("LIBS\\SrcML"); ;

            //not an exaustive list, so it will be a bit of a messy parse
            sandoDirsToAvoid = new List<String>() { "LIBS", ".hg", "bin", "obj" };

            string startingPath = "..\\..";
            string[] dirs = Directory.GetDirectories(startingPath);
            ProcessDirectoryForTesting(dirs);

            monitor.UpdateAfterAdditions();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            monitor.StopMonitoring(true);
            Directory.Delete(indexPath, true);
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

	

        private static List<CodeSearchResult> EnsureRankingPrettyGood(string keywords, Predicate<CodeSearchResult> predicate, int expectedLowestRank)
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
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

            return codeSearchResults;
        }

		private string indexPath;
		private static SolutionMonitor monitor;
		private static SolutionKey key;
		private static List<string> sandoDirsToAvoid;
	}
}
