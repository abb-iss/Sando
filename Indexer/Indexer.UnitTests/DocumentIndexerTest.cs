using System;
using System.Diagnostics.Contracts;
using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Documents;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
	public class DocumentIndexerTest
	{
    	private const string _luceneTempIndexesDirectory = "C:/Windows/Temp";

    	[Test]
		public void DocumentIndexer_ConstructorDoesNotThrowWhenValidData()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			try
			{
				documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenInvalidDirectoryPath()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			try
			{
				documentIndexer = new DocumentIndexer(null, analyzer);
			}
			catch 
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenAnalyzerIsNull()
		{
			try
			{
				documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, null);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[Test]
		public void DocumentIndexer_AddDocumentDoesNotThrowWhenValidData()
		{
			try
			{
				Analyzer analyzer = new SimpleAnalyzer();
				documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
				ClassElement classElement = new ClassElement()
				{
					AccessLevel = Core.AccessLevel.Public,
					DefinitionLineNumber = 11,
					ExtendedClasses = "SimpleClassBase",
					FullFilePath = "C:/Projects/SimpleClass.cs",
					Id = Guid.NewGuid(),
					ImplementedInterfaces = "IDisposable",
					Name = "SimpleClassName",
					Namespace = "Sanod.Indexer.UnitTests"
				};
				SandoDocument sandoDocument = DocumentFactory.Create(classElement);
				Assert.NotNull(sandoDocument);
				Assert.NotNull(sandoDocument.GetDocument());
				documentIndexer.AddDocument(sandoDocument);
				documentIndexer.CommitChanges();
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentIndexer_AddDocumentThrowsWhenProgramElementIsNull()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
			try
			{
				documentIndexer.AddDocument(null);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(contractFailed, "Contract should fail!");
		}

		[SetUp]
		public void ResetContract()
		{
			contractFailed = false;
			Contract.ContractFailed += (sender, e) =>
			{
				e.SetHandled();
				e.SetUnwind();
				contractFailed = true;
			};
		}

		[TearDown]
		public void CloseDocumentIndexer()
		{
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		private bool contractFailed;
		private DocumentIndexer documentIndexer;
	}
}
