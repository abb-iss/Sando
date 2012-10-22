using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.SearchEngine;
using Sando.UI.InterleavingExperiment;
using Sando.UI.Monitoring;
using UnitTestHelpers;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class SelfSearchTest
	{
		/*
		[Test]
		public void TestInterleavingStudy()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			var interleaving = InterleavingManagerSingleton.GetInstance();
			for(int i = 0; i < 6; i++)
			{
				string keywords = "sando document";
				interleaving.RewriteQuery(keywords);
				List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
				Assert.IsTrue(codeSearchResults.Count > 2);
				interleaving.ReorderSearchResults(codeSearchResults.AsQueryable());
				interleaving.NotifyClicked(codeSearchResults[1]);
			}
		}
		*/

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
			InterleavingManagerSingleton.CreateInstance(indexPath);

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

		private string indexPath;
		private static SolutionMonitor monitor;
		private static SolutionKey key;
		private static List<string> sandoDirsToAvoid;
	}
}
