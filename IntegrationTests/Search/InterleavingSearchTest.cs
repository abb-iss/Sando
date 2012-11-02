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
            NoWeightsFLT fltB = InterleavingExperimentManager.Instance.fltB as NoWeightsFLT;
            if (fltA != null && fltB != null)
            {
				Assert.IsNotNull(fltA.Results);
				Assert.IsNotNull(fltB.Results);
				Assert.IsTrue(fltA.Results.Count > 10);
                Assert.IsTrue(fltB.Results.Count > 10);
                Assert.AreSame(BalancedInterleaving.Interleave(fltA.Results, fltB.Results), resultListener.Results);
            }
        }

        [Test]
        public void TestClickRecording()
        {

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
