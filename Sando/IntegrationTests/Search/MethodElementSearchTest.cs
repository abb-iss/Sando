using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class MethodElementSearchTest
	{
		[Test]
		public void MethodElementReturnedFromSearchContainsAllFields()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			string ensureLoaded = "fetch output stream";
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(ensureLoaded);
			Assert.AreEqual(codeSearchResults.Count, 2, "Invalid results number");
			var method = codeSearchResults[0].Element as MethodElement;
			if(method == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + ensureLoaded);
			}
			Assert.AreEqual(method.AccessLevel, AccessLevel.Public, "Method access level differs!");
			//Assert.AreEqual(method.Arguments, "StreamHandler streamHandler string fileName Image image", "Method arguments differs!");
			Assert.NotNull(method.Body, "Method body is null!");
			Assert.True(method.ClassId != null && method.ClassId != Guid.Empty, "Class id is invalid!");
			Assert.AreEqual(method.ClassName, "ImageCapture", "Method class name differs!");
			Assert.AreEqual(method.DefinitionLineNumber, 83, "Method definition line number differs!");
			Assert.True(method.FullFilePath.EndsWith("\\TestFiles\\ClassNameTestFiles\\ImageCapture.cs"), "Method full file path is invalid!");
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
			key = new SolutionKey(new Guid(), "..\\..\\TestFiles\\ClassNameTestFiles", indexPath);
			var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
			monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer);
			string[] files = Directory.GetFiles("..\\..\\TestFiles\\ClassNameTestFiles");
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
