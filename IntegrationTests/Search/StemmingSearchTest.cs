using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using NUnit.Framework;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Parser;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using Sando.Recommender;
using UnitTestHelpers;

namespace Sando.IntegrationTests.Search
{
	[TestFixture]
	public class StemmingSearchTest : AutomaticallyIndexingTestClass
	{
		[Test]
		public void SearchIsUsingStemming()
		{
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
			string keywords = "name";
			List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
			Assert.AreEqual(codeSearchResults.Count, 4, "Invalid results number");
            var classSearchResult = codeSearchResults.Find(el => el.ProgramElement.ProgramElementType == ProgramElementType.Class && el.ProgramElement.Name == "FileNameTemplate");
			if(classSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var classElement = classSearchResult.ProgramElement as ClassElement;
			Assert.AreEqual(classElement.AccessLevel, AccessLevel.Public, "Class access level differs!");
			Assert.AreEqual(classElement.ExtendedClasses, String.Empty, "Class extended classes differs!");
			Assert.AreEqual(classElement.DefinitionLineNumber, 10, "Class definition line number differs!");
			Assert.True(classElement.FullFilePath.EndsWith("\\TestFiles\\StemmingTestFiles\\FileNameTemplate.cs"), "Class full file path is invalid!");
			Assert.AreEqual(classElement.Name, "FileNameTemplate", "Class name differs!");
			Assert.AreEqual(classElement.ProgramElementType, ProgramElementType.Class, "Program element type differs!");
			Assert.AreEqual(classElement.ImplementedInterfaces, String.Empty, "Class implemented interfaces differs!");
			Assert.False(String.IsNullOrWhiteSpace(classElement.RawSource), "Class snippet is invalid!");

			var methodSearchResult = codeSearchResults.Find(el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && el.ProgramElement.Name == "Parse");
			if(methodSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var methodElement = methodSearchResult.ProgramElement as MethodElement;
			Assert.AreEqual(methodElement.AccessLevel, AccessLevel.Public, "Method access level differs!");
			Assert.AreEqual(methodElement.Arguments, "string extension", "Method arguments differs!");
			Assert.NotNull(methodElement.Body, "Method body is null!");
			Assert.True(methodElement.ClassId != null && methodElement.ClassId != Guid.Empty, "Class id is invalid!");
			Assert.AreEqual(methodElement.ClassName, "FileNameTemplate", "Method class name differs!");
			Assert.AreEqual(methodElement.DefinitionLineNumber, 17, "Method definition line number differs!");
			Assert.True(methodElement.FullFilePath.EndsWith("\\TestFiles\\StemmingTestFiles\\FileNameTemplate.cs"), "Method full file path is invalid!");
			Assert.AreEqual(methodElement.Name, "Parse", "Method name differs!");
			Assert.AreEqual(methodElement.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			Assert.AreEqual(methodElement.ReturnType, "ImagePairNames", "Method return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(methodElement.RawSource), "Method snippet is invalid!");

			methodSearchResult = codeSearchResults.Find(el => el.ProgramElement.ProgramElementType == ProgramElementType.Method && el.ProgramElement.Name == "TryAddTemplatePrompt");
			if(methodSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			methodElement = methodSearchResult.ProgramElement as MethodElement;
			Assert.AreEqual(methodElement.AccessLevel, AccessLevel.Private, "Method access level differs!");
			Assert.AreEqual(methodElement.Arguments, "ImagePairNames startNames", "Method arguments differs!");
			Assert.NotNull(methodElement.Body, "Method body is null!");
			Assert.True(methodElement.ClassId != null && methodElement.ClassId != Guid.Empty, "Class id is invalid!");
            Assert.AreEqual(methodElement.ClassName, "FileNameTemplate", "Method class name differs!");
			Assert.AreEqual(methodElement.DefinitionLineNumber, 53, "Method definition line number differs!");
			Assert.True(methodElement.FullFilePath.EndsWith("\\TestFiles\\StemmingTestFiles\\FileNameTemplate.cs"), "Method full file path is invalid!");
			Assert.AreEqual(methodElement.Name, "TryAddTemplatePrompt", "Method name differs!");
			Assert.AreEqual(methodElement.ProgramElementType, ProgramElementType.Method, "Program element type differs!");
			//Assert.AreEqual(methodElement.ReturnType, "ImagePairNames", "Method return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(methodElement.RawSource), "Method snippet is invalid!");

			var fieldSearchResult = codeSearchResults.Find(el => el.ProgramElement.ProgramElementType == ProgramElementType.Field && el.ProgramElement.Name == "fileName");
			if(fieldSearchResult == null)
			{
				Assert.Fail("Failed to find relevant search result for search: " + keywords);
			}
			var fieldElement = fieldSearchResult.ProgramElement as FieldElement;
			Assert.AreEqual(fieldElement.AccessLevel, AccessLevel.Private, "Field access level differs!");
			Assert.True(fieldElement.ClassId != null && methodElement.ClassId != Guid.Empty, "Class id is invalid!");
            Assert.AreEqual(fieldElement.ClassName, "FileNameTemplate", "Field class name differs!");
			Assert.AreEqual(fieldElement.DefinitionLineNumber, 12, "Field definition line number differs!");
			Assert.True(fieldElement.FullFilePath.EndsWith("\\TestFiles\\StemmingTestFiles\\FileNameTemplate.cs"), "Field full file path is invalid!");
			Assert.AreEqual(fieldElement.Name, "fileName", "Field name differs!");
			Assert.AreEqual(fieldElement.ProgramElementType, ProgramElementType.Field, "Program element type differs!");
			Assert.AreEqual(fieldElement.FieldType, "string", "Field return type differs!");
			Assert.False(String.IsNullOrWhiteSpace(methodElement.RawSource), "Field snippet is invalid!");
		}

        public override string GetIndexDirName()
        {
            return "StemminSearchTest";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\StemmingTestFiles";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(1);
        }


	}
}
