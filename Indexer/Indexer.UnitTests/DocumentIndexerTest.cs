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
			try
			{
				documentIndexer = new DocumentIndexer(_luceneTempIndexesDirectory, analyzer);
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
			finally
			{
				if(documentIndexer != null)
					documentIndexer.Dispose();
			}
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
			finally
			{
				if(documentIndexer != null)
					documentIndexer.Dispose();
			}
			Assert.True(contractFailed, "Contract should fail!");
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
			finally
			{
				if(documentIndexer != null)
					documentIndexer.Dispose();
			}
			Assert.True(contractFailed, "Contract should fail!");
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
			try
			{
				target.AddDocument(sandoDocument);
				target.CommitChanges();
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
			finally
			{
				target.Dispose();
			}
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
			finally
			{
				target.Dispose();
			}
			Assert.True(contractFailed, "Contract should fail!");
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
