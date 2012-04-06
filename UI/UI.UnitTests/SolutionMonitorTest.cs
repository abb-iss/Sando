using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using Sando.Core;
using Sando.Indexer;
using UnitTestHelpers;

namespace Sando.UI.UnitTests
{
    [TestFixture]
    public class SolutionMonitorTest
    {
        private static SolutionMonitor monitor;
        private static SolutionKey key;
        private const string _luceneTempIndexesDirectory = "C:/Windows/Temp";




        [TearDown]
		public void TearDown()
        {
            monitor.StopMonitoring();
			Directory.Delete(_luceneTempIndexesDirectory + "/basic/", true);
        }

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(_luceneTempIndexesDirectory + "/basic/");
            TestUtils.ClearDirectory(_luceneTempIndexesDirectory + "/basic/");
            key = new SolutionKey(Guid.NewGuid(), ".\\TestFiles\\FourCSFiles", _luceneTempIndexesDirectory + "/basic/");
            var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
            monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer);            
            string[] files = Directory.GetFiles(".\\TestFiles\\FourCSFiles");
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                monitor.ProcessFileForTesting(fullPath);
            }            
            monitor.UpdateAfterAdditions();
            //TODO - Why is this necessary??
		    Thread.Sleep(1000);
        }
        [Test]
        public void SolutionMonitor_BasicSetupTest()
        {
     
        }

		[Test]
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

		//[Test]
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

        [Test]
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
