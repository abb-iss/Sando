using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sando.Core;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using Sando.UI.View;
using UnitTestHelpers;
using Sando.UI.InterleavingExperiment;
using System.Linq;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class InterleavingSearchTest
	{
        [Test]
        public void TestInterleavedResults()
        {
            string keywords = "test interleaved results";
        	var resultListener = new InterleavingSearchTest_ResultListener();
        	var searchManager = new SearchManager(resultListener);
			searchManager.Search(keywords, indexPath, key);
			FeatureLocationTechnique fltA = InterleavingExperimentManager.Instance.fltA;
            FeatureLocationTechnique fltB = InterleavingExperimentManager.Instance.fltB;
            if (fltA != null && fltB != null)
            {
                int expectedNumOfResults = 10;
                CheckInterleaving(fltA.Results, fltB.Results, resultListener.Results, expectedNumOfResults);
            }
        }

        [Test]
        public void TestInterleavedResults2()
        {
            string keywords = "sando document";
            var resultListener = new InterleavingSearchTest_ResultListener();
            var searchManager = new SearchManager(resultListener);
            searchManager.Search(keywords, indexPath, key);
            FeatureLocationTechnique fltA = InterleavingExperimentManager.Instance.fltA;
            FeatureLocationTechnique fltB = InterleavingExperimentManager.Instance.fltB;
            if (fltA != null && fltB != null)
            {
                int expectedNumOfResults = 10;
                CheckInterleaving(fltA.Results, fltB.Results, resultListener.Results, expectedNumOfResults);
            }
        }

        [Test]
        public void TestClickRecording()
        {
            string keywords = "search";
            var resultListener = new InterleavingSearchTest_ResultListener();
            var searchManager = new SearchManager(resultListener);
            searchManager.Search(keywords, indexPath, key);
            FeatureLocationTechnique fltA = InterleavingExperimentManager.Instance.fltA;
            FeatureLocationTechnique fltB = InterleavingExperimentManager.Instance.fltB;
            if (fltA != null && fltB != null)
            {
                InterleavingExperimentManager.Instance.NotifyClicked(resultListener.Results[0]);
                InterleavingExperimentManager.Instance.NotifyClicked(resultListener.Results[2]);
                Assert.IsTrue(InterleavingExperimentManager.Instance.ClickIdx.Contains(0));
                Assert.IsTrue(InterleavingExperimentManager.Instance.ClickIdx.Contains(2));
                int scoreA, scoreB;
                BalancedInterleaving.DetermineWinner(fltA.Results, fltB.Results, resultListener.Results,
                                                     InterleavingExperimentManager.Instance.ClickIdx, 
                                                     out scoreA, out scoreB);
                Assert.IsTrue((scoreA == 2 && scoreB == 0) || (scoreA == 0 && scoreB == 2));
            }

        }

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
            InterleavingExperimentManager.Instance.InitializeExperimentParticipants(indexPath);
		}

		[SetUp]
		public void Setup()
		{
			indexPath = Path.Combine(Path.GetTempPath(), "InterleavingSearchTest");
			Directory.CreateDirectory(indexPath);
			key = new SolutionKey(Guid.NewGuid(), "..\\..", indexPath);
			var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
			monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer, false);

			string startingPath = "..\\..";
			string[] dirs = Directory.GetDirectories(startingPath);
			ProcessDirectoryForTesting(dirs);

			monitor.UpdateAfterAdditions();
		}

        private static void CheckInterleaving(List<CodeSearchResult> resultsA, List<CodeSearchResult> resultsB, List<CodeSearchResult> resultsI, int expectedNumOfResults)
        {
            Assert.IsNotNull(resultsA);
            Assert.IsNotNull(resultsB);
            Assert.IsTrue(resultsA.Count > expectedNumOfResults);
            Assert.IsTrue(resultsB.Count > expectedNumOfResults);
            Assert.IsTrue(!resultsA.SequenceEqual(resultsB));
            Assert.IsTrue(!resultsA.SequenceEqual(resultsI));
            Assert.IsTrue(!resultsB.SequenceEqual(resultsI));
            Assert.IsTrue(resultsI.Count > expectedNumOfResults);
            Assert.IsTrue(resultsI[0] == resultsA[0] || resultsI[0] == resultsB[0]);
        }

		private void ProcessDirectoryForTesting(string[] dirs)
		{
			foreach (var dir in dirs)
			{
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

		private string indexPath;
		private static SolutionMonitor monitor;
		private static SolutionKey key;
	}
}
