using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sando.Core;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.SearchEngine;
using Sando.UI.Monitoring;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class AllElementsSearchTest
	{
		[Test]
		public void SearchRespectsAccessLevelCriteria()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			string keywords = "usage type";
			SearchCriteria searchCriteria = new SimpleSearchCriteria()
			{
				AccessLevels = new SortedSet<AccessLevel>() { AccessLevel.Private },
				SearchByAccessLevel = true,
				SearchTerms = new SortedSet<string>(keywords.Split(' '))
			};
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(searchCriteria);
			Assert.AreEqual(codeSearchResults.Count, 2, "Invalid results number");
			var methodSearchResult = codeSearchResults.Find(el =>
																el.Element.ProgramElementType == ProgramElementType.Method &&
																(el.Element.Name == "UsageTypeCriteriaToString"));
			if(methodSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var method = methodSearchResult.Element as MethodElement;
			Assert.AreEqual(method.AccessLevel, AccessLevel.Private, "Method access level differs!");
			Assert.AreEqual(method.Arguments, "StringBuilder stringBuilder bool searchByUsageType", "Method arguments differs!");
			Assert.NotNull(method.Body, "Method body is null!");
			//Assert.True(method.ClassId != null && method.ClassId != Guid.Empty, "Class id is invalid!");
			//Assert.AreEqual(method.ClassName, "SimpleSearchCriteria", "Method class name differs!");
			Assert.AreEqual(method.DefinitionLineNumber, 96, "Method definition line number differs!");
			Assert.True(method.FullFilePath.EndsWith("\\TestFiles\\MethodElementTestFiles\\Searcher.cs"), "Method full file path is invalid!");
			Assert.AreEqual(method.Name, "UsageTypeCriteriaToString", "Method name differs!");
			Assert.AreEqual(method.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			Assert.AreEqual(method.ReturnType, "void", "Method return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(method.Snippet), "Method snippet is invalid!");

			methodSearchResult = codeSearchResults.Find(el =>
															el.Element.ProgramElementType == ProgramElementType.Method &&
															(el.Element.Name == "SingleUsageTypeCriteriaToString"));
			if(methodSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			method = methodSearchResult.Element as MethodElement;
			Assert.AreEqual(method.AccessLevel, AccessLevel.Private, "Method access level differs!");
			Assert.AreEqual(method.Arguments, "StringBuilder stringBuilder UsageType usageType", "Method arguments differs!");
			Assert.NotNull(method.Body, "Method body is null!");
			//Assert.True(method.ClassId != null && method.ClassId != Guid.Empty, "Class id is invalid!");
			//Assert.AreEqual(method.ClassName, "SimpleSearchCriteria", "Method class name differs!");
			Assert.AreEqual(method.DefinitionLineNumber, 141, "Method definition line number differs!");
			Assert.True(method.FullFilePath.EndsWith("\\TestFiles\\MethodElementTestFiles\\Searcher.cs"), "Method full file path is invalid!");
			Assert.AreEqual(method.Name, "SingleUsageTypeCriteriaToString", "Method name differs!");
			Assert.AreEqual(method.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			Assert.AreEqual(method.ReturnType, "void", "Method return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(method.Snippet), "Method snippet is invalid!");
		}

		[Test]
		public void SearchWorksNormallyForTermsWhichEndsWithSpace()
		{
			try
			{
				var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
				string keywords = "  usage ";
				List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void SearchReturnsElementsUsingCrossFieldMatching()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			string keywords = "fetch output argument";
			SearchCriteria searchCriteria = new SimpleSearchCriteria()
			{
				SearchTerms = new SortedSet<string>(keywords.Split(' '))
			};
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(searchCriteria);

			var methodSearchResult = codeSearchResults.Find(el =>
																el.Element.ProgramElementType == ProgramElementType.Method &&
																(el.Element.Name == "FetchOutputStream"));
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
