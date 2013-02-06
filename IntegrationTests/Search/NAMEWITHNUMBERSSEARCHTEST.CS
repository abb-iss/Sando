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

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class NamesWithNumbersSearchTest
	{
		[Test]
		public void SearchReturnsElementsWithNumbersInNames()
		{
			var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
			string keywords = "problem";
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
			Assert.AreEqual(codeSearchResults.Count, 2, "Invalid results number");
			var classSearchResult = codeSearchResults.Find(el => el.Element.ProgramElementType == ProgramElementType.Class && el.Element.Name == "Problem0010Resolver");
			if(classSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var classElement = classSearchResult.Element as ClassElement;
			Assert.AreEqual(classElement.AccessLevel, AccessLevel.Public, "Class access level differs!");
			Assert.AreEqual(classElement.ExtendedClasses, "IResolver", "Class extended classes differs!");
			Assert.AreEqual(classElement.DefinitionLineNumber, 8, "Class definition line number differs!");
			Assert.True(classElement.FullFilePath.EndsWith("\\TestFiles\\NamesWithNumbersTestFiles\\Problem0010Resolver.cs"), "Class full file path is invalid!");
			Assert.AreEqual(classElement.Name, "Problem0010Resolver", "Class name differs!");
			Assert.AreEqual(classElement.ProgramElementType, ProgramElementType.Class, "Program element type differs!");
			Assert.AreEqual(classElement.ImplementedInterfaces, String.Empty, "Class implemented interfaces differs!");
			Assert.False(String.IsNullOrWhiteSpace(classElement.RawSource), "Class snippet is invalid!");

			var constructorSearchResult = codeSearchResults.Find(el => el.Element.ProgramElementType == ProgramElementType.Method && el.Element.Name == "Problem0010Resolver");
			if(constructorSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var constructorElement = constructorSearchResult.Element as MethodElement;
			//Assert.AreEqual(constructorElement.AccessLevel, AccessLevel.Public, "Constructor access level differs!");
			Assert.IsTrue(constructorElement.IsConstructor, "IResolver", "Constructor extended classes differs!");
			Assert.AreEqual(constructorElement.DefinitionLineNumber, 10, "Constructor definition line number differs!");
			Assert.True(constructorElement.FullFilePath.EndsWith("\\TestFiles\\NamesWithNumbersTestFiles\\Problem0010Resolver.cs"), "Constructor full file path is invalid!");
			Assert.AreEqual(constructorElement.Name, "Problem0010Resolver", "Constructor name differs!");
			Assert.AreEqual(constructorElement.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			Assert.AreEqual(constructorElement.ReturnType, String.Empty, "Constructor should have an empty return type!");
			Assert.AreEqual(constructorElement.Modifiers, String.Empty, "Constructor modifiers differs!");
			Assert.AreEqual(constructorElement.Arguments, "long max", "Constructor modifiers differs!");
			Assert.False(String.IsNullOrWhiteSpace(constructorElement.RawSource), "Constructor snippet is invalid!");
		}



        [TestFixtureSetUp]
		public void Setup()
		{
            TestUtils.InitializeDefaultExtensionPoints();
			indexPath = Path.Combine(Path.GetTempPath(), "NamesWithNumbersSearchTest");
			Directory.CreateDirectory(indexPath);
			key = new SolutionKey(Guid.NewGuid(), "..\\..\\IntegrationTests\\TestFiles\\NamesWithNumbersTestFiles", indexPath);
			var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
			monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer, false);
			string[] files = Directory.GetFiles("..\\..\\IntegrationTests\\TestFiles\\NamesWithNumbersTestFiles");
            SwumManager.Instance.Initialize(key.GetIndexPath(), true);
            SwumManager.Instance.Generator = new ABB.SrcML.SrcMLGenerator("LIBS\\SrcML"); ;
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
