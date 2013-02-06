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
using Sando.Recommender;

namespace Sando.UI.UnitTests
{
    [TestFixture]   // [TestClass]
    public class SolutionMonitorTest
    {
        private static SolutionMonitor monitor;
        private static SolutionKey key;
        private const string _luceneTempIndexesDirectory = "C:/Windows/Temp";




        [TestFixtureTearDown]  // [TestCleanup] (TearDown for Unit Test)
		public void TearDown()
        {
            monitor.StopMonitoring(true);
			Directory.Delete(_luceneTempIndexesDirectory + "/basic/", true);
        }


        [TestFixtureSetUp] // [TestInitialize] (Setup for Unit Test)
        public void Setup()
        {
            TestUtils.InitializeDefaultExtensionPoints();
		
			Directory.CreateDirectory(_luceneTempIndexesDirectory + "/basic/");
            TestUtils.ClearDirectory(_luceneTempIndexesDirectory + "/basic/");
            key = new SolutionKey(Guid.NewGuid(), ".\\TestFiles\\FourCSFiles", _luceneTempIndexesDirectory + "/basic/");
            var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
            monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer, false);
            SwumManager.Instance.Initialize(key.GetIndexPath(), true);
            SwumManager.Instance.Generator = new ABB.SrcML.SrcMLGenerator("LIBS\\SrcML"); ;
            string[] files = Directory.GetFiles(".\\TestFiles\\FourCSFiles");
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                monitor.ProcessFileForTesting(fullPath);
            }            
            monitor.UpdateAfterAdditions();
        }

        [Test]  // [TestMethod]
        public void SolutionMonitor_BasicSetupTest()
        {
     
        }

        [Test]  // [TestMethod]
		public void SolutionMonitor_SearchTwoWords()
		{
		    var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
		    string ensureLoaded = "extension file";
		    List<CodeSearchResult> codeSearchResults = codeSearcher.Search(ensureLoaded);
		    foreach (var codeSearchResult in codeSearchResults)
		    {
		        var method = codeSearchResult.Element as MethodElement;
		        if (method != null)
		        {
		            if (method.Name.Equals("SetFileExtension"))
		                return;
		        }
		    }
		    Assert.Fail("Failed to find relevant search result for search: " + ensureLoaded);
		}

        //[Test]  // [TestMethod]
		//public void SolutionMonitor_SearchResultsContainsConstructor()
		//{
		//    var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
		//    string ensureLoaded = "file name template";
		//    List<CodeSearchResult> codeSearchResults = codeSearcher.Search(ensureLoaded);
		//    foreach(var codeSearchResult in codeSearchResults)
		//    {
		//        var method = codeSearchResult.Element as MethodElement;
		//        if(method != null)
		//        {
		//            if(method.Name.Equals("FileNameTemplate"))
		//                return;
		//        }
		//    }
		//    Assert.Fail("Failed to find relevant search result for search: " + ensureLoaded);
		//}

        [Test]  // [TestMethod]
        public void SolutionMonitor_SearchForExtension()
        {
            var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
            string ensureLoaded = "extension";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(ensureLoaded);
            foreach (var codeSearchResult in codeSearchResults)
            {
                var method = codeSearchResult.Element as MethodElement;
                if (method != null)
                {
                    if (method.Name.Equals("SetFileExtension"))
                        return;
                }
            }
            Assert.Fail("Failed to find relevant search result for search: " + ensureLoaded);
        }

    }
}
