using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using Sando.Indexer.Searching.Criteria;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class MethodElementSearchTest
	{
		[Test]
		public void MethodElementReturnedFromSearchContainsAllFields()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			string keywords = "fetch output stream";
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
			Assert.AreEqual(codeSearchResults.Count, 3, "Invalid results number");
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
			Assert.False(String.IsNullOrWhiteSpace(method.Snippet), "Method snippet is invalid!");
		}

		[Test]
		public void MethodSearchRespectsAccessLevelCriteria()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			string keywords = "to string";
			SearchCriteria searchCriteria = new SimpleSearchCriteria()
			{
				AccessLevels = new SortedSet<AccessLevel>() { AccessLevel.Public },
				SearchByAccessLevel = true,
				SearchTerms = new SortedSet<string>(keywords.Split(' '))
			};
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(searchCriteria);
			Assert.AreEqual(codeSearchResults.Count, 1, "Invalid results number");
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
			Assert.False(String.IsNullOrWhiteSpace(method.Snippet), "Method snippet is invalid!");
		}

		[SetUp]
		public void Setup()
		{
			indexPath = Path.Combine(Path.GetTempPath(), "MethodElementSearchTest");
			Directory.CreateDirectory(indexPath);
			key = new SolutionKey(Guid.NewGuid(), "..\\..\\IntegrationTests\\TestFiles\\MethodElementTestFiles", indexPath);
			var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
			monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer);
			string[] files = Directory.GetFiles("..\\..\\IntegrationTests\\TestFiles\\MethodElementTestFiles");
			foreach(var file in files)
			{
				string fullPath = Path.GetFullPath(file);
				monitor.ProcessFileForTesting(fullPath);
			}
			monitor.UpdateAfterAdditions();
		    Thread.Sleep(1000);
		}

		[TearDown]
		public void TearDown()
		{
			monitor.StopMonitoring();
			Directory.Delete(indexPath, true);
		}

		private string indexPath;
		private static SolutionMonitor monitor;
		private static SolutionKey key;
	}
}
