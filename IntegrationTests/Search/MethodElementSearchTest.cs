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
	public class MethodElementSearchTest
	{
		[Test]
		public void MethodElementReturnedFromSearchContainsAllFields()
		{
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
			string keywords = "fetch output stream";
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
			Assert.AreEqual(codeSearchResults.Count, 5, "Invalid results number");
			var methodSearchResult = codeSearchResults.Find(el => el.Element.ProgramElementType == ProgramElementType.Method && el.Element.Name == "FetchOutputStream");
			if(methodSearchResult == null)
			{ 
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var method = methodSearchResult.Element as MethodElement;
			Assert.AreEqual(method.AccessLevel, AccessLevel.Public, "Method access level differs!");
			Assert.AreEqual(method.Arguments, "A B string fileName Image image", "Method arguments differs!");
			Assert.NotNull(method.Body, "Method body is null!");
			Assert.True(method.ClassId != null && method.ClassId != Guid.Empty, "Class id is invalid!");
            Assert.AreEqual(method.ClassName, "ImageCapture", "Method class name differs!");
			Assert.AreEqual(method.DefinitionLineNumber, 83, "Method definition line number differs!");
			Assert.True(method.FullFilePath.EndsWith("\\TestFiles\\MethodElementTestFiles\\ImageCapture.cs"), "Method full file path is invalid!");
			Assert.AreEqual(method.Name, "FetchOutputStream", "Method name differs!");
			Assert.AreEqual(method.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			Assert.AreEqual(method.ReturnType, "void", "Method return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(method.RawSource), "Method snippet is invalid!");
		}

		[Test]
		public void MethodSearchRespectsAccessLevelCriteria()
		{
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
			string keywords = "to string";
			SearchCriteria searchCriteria = new SimpleSearchCriteria()
			{ 
				AccessLevels = new SortedSet<AccessLevel>() { AccessLevel.Public },
				SearchByAccessLevel = true,
				SearchTerms = new SortedSet<string>(keywords.Split(' '))
			};
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(searchCriteria);
			Assert.AreEqual(7, codeSearchResults.Count, "Invalid results number");
			var methodSearchResult = codeSearchResults.Find(el => el.Element.ProgramElementType == ProgramElementType.Method && el.Element.Name == "ToQueryString");
			if(methodSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var method = methodSearchResult.Element as MethodElement;
			Assert.AreEqual(method.AccessLevel, AccessLevel.Public, "Method access level differs!");
			Assert.AreEqual(method.Arguments, String.Empty, "Method arguments differs!");
			Assert.NotNull(method.Body, "Method body is null!");
			Assert.True(method.ClassId != null && method.ClassId != Guid.Empty, "Class id is invalid!");
            Assert.AreEqual(method.ClassName, "SimpleSearchCriteria", "Method class name differs!");
			Assert.AreEqual(method.DefinitionLineNumber, 31, "Method definition line number differs!");
			Assert.True(method.FullFilePath.EndsWith("\\TestFiles\\MethodElementTestFiles\\Searcher.cs"), "Method full file path is invalid!");
			Assert.AreEqual(method.Name, "ToQueryString", "Method name differs!");
			Assert.AreEqual(method.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			Assert.AreEqual(method.ReturnType, "void", "Method return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(method.RawSource), "Method snippet is invalid!");
		}

        [Test]
        public void MethodSearchRespectsFileExtensionsCriteria()
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            var keywords = "main";
            var searchCriteria = new SimpleSearchCriteria()
                {
                    SearchTerms = new SortedSet<string>(keywords.Split(' ')),
                    SearchByFileExtension = true,
                    FileExtensions = new SortedSet<string> {".cpp"}
                };
            var codeSearchResults = codeSearcher.Search(searchCriteria);
            Assert.AreEqual(1, codeSearchResults.Count, "Invalid results number");
            var methodSearchResult = codeSearchResults.Find(el => el.Element.ProgramElementType == ProgramElementType.Method && el.Element.Name == "main");
            if (methodSearchResult == null)
            {
                Assert.Fail("Failed to find relevant search result for search: " + keywords);
            }
            //var method = methodSearchResult.Element as MethodElement;
            //Assert.AreEqual(method.AccessLevel, AccessLevel.Public, "Method access level differs!");
            //Assert.AreEqual(method.Arguments, String.Empty, "Method arguments differs!");
            //Assert.NotNull(method.Body, "Method body is null!");
            //Assert.True(method.ClassId != null && method.ClassId != Guid.Empty, "Class id is invalid!");
            //Assert.AreEqual(method.ClassName, "SimpleSearchCriteria", "Method class name differs!");
            //Assert.AreEqual(method.DefinitionLineNumber, 31, "Method definition line number differs!");
            //Assert.True(method.FullFilePath.EndsWith("\\TestFiles\\MethodElementTestFiles\\Searcher.cs"), "Method full file path is invalid!");
            //Assert.AreEqual(method.Name, "ToQueryString", "Method name differs!");
            //Assert.AreEqual(method.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
            //Assert.AreEqual(method.ReturnType, "void", "Method return type differs!");
            //Assert.False(String.IsNullOrWhiteSpace(method.RawSource), "Method snippet is invalid!");
        }


		[TestFixtureSetUp]
		public void Setup()
		{
            TestUtils.InitializeDefaultExtensionPoints();
			indexPath = Path.Combine(Path.GetTempPath(), "MethodElementSearchTest");
			Directory.CreateDirectory(indexPath);
			key = new SolutionKey(Guid.NewGuid(), "..\\..\\IntegrationTests\\TestFiles\\MethodElementTestFiles", indexPath);
            ServiceLocator.RegisterInstance(key); ServiceLocator.RegisterInstance<Analyzer>(new SnowballAnalyzer("English"));

            var indexer = new DocumentIndexer(1000, 0); //0 means synchronous commits
            ServiceLocator.RegisterInstance(indexer);

			monitor = new SolutionMonitor(new SolutionWrapper(), indexer, false);
            SwumManager.Instance.Initialize(key.IndexPath, true);
            SwumManager.Instance.Generator = new ABB.SrcML.SrcMLGenerator("LIBS\\SrcML"); ;

			string[] files = Directory.GetFiles("..\\..\\IntegrationTests\\TestFiles\\MethodElementTestFiles");
			foreach(var file in files)
			{
				string fullPath = Path.GetFullPath(file);
				monitor.ProcessFileForTesting(fullPath);
			}
			monitor.UpdateAfterAdditions();
		}

		[TestFixtureTearDown]
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
