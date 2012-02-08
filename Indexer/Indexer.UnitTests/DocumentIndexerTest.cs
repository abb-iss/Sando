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
			DocumentIndexer documentIndexer = null;
			//Assert.DoesNotThrow(() => documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer));
			try
			{
				documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
				Assert.Pass();
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenInvalidDirectoryPath()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer documentIndexer = null;
			try
			{
				documentIndexer = new DocumentIndexer(null, analyzer);
			}
			catch 
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenAnalyzerIsNull()
		{
			DocumentIndexer documentIndexer = null;
			try
			{
				documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, null);
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
			if(documentIndexer != null)
				documentIndexer.Dispose();
		}

		[Test]
		public void DocumentIndexer_AddDocumentDoesNotThrowWhenValidData()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer target = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
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
			SandoDocument sandoDocument = ClassDocument.Create(classElement);
			Assert.NotNull(sandoDocument);
			Assert.NotNull(sandoDocument.GetDocument());
			Assert.DoesNotThrow(() => target.AddDocument(sandoDocument));
			Assert.DoesNotThrow(() => target.CommitChanges());
			target.Dispose();
		}

		[Test]
		public void DocumentIndexer_AddDocumentThrowsWhenProgramElementIsNull()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			DocumentIndexer target = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
			try
			{
				target.AddDocument(null);
			}
			catch
			{
			}
			Assert.True(contractFailed, "Contract should fail!");
			target.Dispose();
		}

		[SetUp]
		public void resetContract()
		{
			contractFailed = false;
			Contract.ContractFailed += (sender, e) =>
			{
				e.SetHandled();
				e.SetUnwind();
				contractFailed = true;
			};
		}

		private bool contractFailed;
	}
}
