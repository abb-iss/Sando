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
	public class LexicalSelfSearchTest :AutomaticallyIndexingTestClass
	{
		[Test]
		public void ExactLexMatchSearch1(){
            string keywords = "LexicalSelfSearchTest";
		    var expectedLowestRank = 1;
			Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "LexicalSelfSearchTest");
			EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}

        [Test]
        public void ExactLexMatchSearch2()
        {
            string keywords = "lexicalselfsearchtest";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "LexicalSelfSearchTest");
            EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        [Test]
        public void ExactLexMatchSearch3()
        {
            string keywords = "LexicalSelf";
            var expectedLowestRank = 1;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && (el.ProgramElement.Name == "LexicalSelfSearchTest");
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
            var expectedLowestRank = 2;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "Pumpkin");
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
            var expectedLowestRank = 4;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "InitDte2");
            List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
		}

        //"Assert.IsNotNull(wordSplitter, "Default word splitter should be used!");"


        [Test]
        public void QuotedExactQueryWithQuotesInItMindBlownTest()
        {
            string keywords = "\"Assert.IsNotNull(wordSplitter, \\\"Default word splitter should x used!!\\\");\"";
            var expectedLowestRank = 3;
            Predicate<CodeSearchResult> predicate = el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && (el.ProgramElement.Name == "FindAndRegisterValidExtensionPoints_RemovesInvalidCustomWordSplitterConfiguration");
            List<CodeSearchResult> results = EnsureRankingPrettyGood(keywords, predicate, expectedLowestRank);
        }

        public override string GetIndexDirName()
        {
            return "LexSelfSearchTest";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(5);
        }

        //[TestFixtureSetUp]
        //public void Setup()
        //{
        //    TestUtils.InitializeDefaultExtensionPoints();
        //    indexPath = Path.Combine(Path.GetTempPath(), "SelfSearchTest");
        //    Directory.CreateDirectory(indexPath);
        //    key = new SolutionKey(Guid.NewGuid(), "..\\..", indexPath);
        //    ServiceLocator.RegisterInstance(key);

        //    ServiceLocator.RegisterInstance<Analyzer>(new SnowballAnalyzer("English"));

        //    var indexer = new DocumentIndexer(TimeSpan.FromSeconds(1));
        //    ServiceLocator.RegisterInstance(indexer);

        //    monitor = new SolutionMonitor(new SolutionWrapper(), indexer, false);

        //    SwumManager.Instance.Initialize(key.IndexPath, true);
        //    SwumManager.Instance.Generator = new ABB.SrcML.SrcMLGenerator("LIBS\\SrcML"); ;

        //    //not an exaustive list, so it will be a bit of a messy parse
        //    sandoDirsToAvoid = new List<String>() { "LIBS", ".hg", "bin", "obj" };

        //    string startingPath = "..\\..";
        //    string[] dirs = Directory.GetDirectories(startingPath);
        //    ProcessDirectoryForTesting(dirs);

        //    monitor.UpdateAfterAdditions();
        //}

        //[TestFixtureTearDown]
        //public void TearDown()
        //{
        //    monitor.StopMonitoring(true);
        //    Directory.Delete(indexPath, true);
        //}

        //private void ProcessDirectoryForTesting(string[] dirs)
        //{
        //    foreach (var dir in dirs)
        //    {
        //        if (sandoDirsToAvoid.Contains(Path.GetFileName(dir))) continue;

        //        string[] subdirs = Directory.GetDirectories(dir);
        //        ProcessDirectoryForTesting(subdirs);

        //        string[] files = Directory.GetFiles(dir);
        //        foreach (var file in files)
        //        { 
        //            string fullPath = Path.GetFullPath(file);
        //            if (Path.GetExtension(fullPath) == ".cs")
        //            {
        //                monitor.ProcessFileForTesting(fullPath);
        //            }
        //        }
        //    }
        //}

	

 		//TODO: add splitter test on sando
        // starting with some method name with all lower case.
				
	}
}
