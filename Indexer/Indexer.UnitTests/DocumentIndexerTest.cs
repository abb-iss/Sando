using System;
using System.Diagnostics.Contracts;
using System.IO;
using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.UnitTestHelpers;
using UnitTestHelpers;

namespace Sando.Indexer.UnitTests
{
    [TestFixture]
	public class DocumentIndexerTest
	{
    	private const string _luceneTempIndexesDirectory = "C:/Windows/Temp/basic";

    	[Test]
		public void DocumentIndexer_ConstructorDoesNotThrowWhenValidData()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			try
			{
				documentIndexer = new DocumentIndexer(analyzer);
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentIndexer_ConstructorThrowsWhenAnalyzerIsNull()
		{
			try
			{
				documentIndexer = new DocumentIndexer(null);
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
				documentIndexer = new DocumentIndexer(analyzer);
				ClassElement classElement = SampleProgramElementFactory.GetSampleClassElement();
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
			documentIndexer = new DocumentIndexer(analyzer);
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

		[Test]
		public void DocumentIndexer_CommitChangesTriggersNotifyAboutIndexUpdateOnIndexUpdateListeners()
		{
			Analyzer analyzer = new SimpleAnalyzer();
			try
			{
				documentIndexer = new DocumentIndexer(analyzer);
				TestIndexUpdateListener testIndexUpdateListener = new TestIndexUpdateListener();
				documentIndexer.AddIndexUpdateListener(testIndexUpdateListener);
				Assert.True(testIndexUpdateListener.NotifyCalled == false, "Notify flag set without NotifyAboutIndexUpdate call!");
				documentIndexer.CommitChanges();
				Assert.True(testIndexUpdateListener.NotifyCalled == true, "NotifyAboutIndexUpdate wasn't called!");
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

        [Test]
        public void DocumentIndexer_DeleteDocuments()
        {
            Analyzer analyzer = new SimpleAnalyzer();
            try
            {                                
                TestUtils.ClearDirectory(_luceneTempIndexesDirectory);
                documentIndexer = new DocumentIndexer(analyzer);
                MethodElement sampleMethodElement = SampleProgramElementFactory.GetSampleMethodElement();
                documentIndexer.AddDocument(DocumentFactory.Create(sampleMethodElement));
                documentIndexer.CommitChanges();
                int numDocs = documentIndexer.IndexSearcher.reader_ForNUnit.NumDocs();
                Assert.IsTrue(numDocs == 1);
                documentIndexer.DeleteDocuments(sampleMethodElement.FullFilePath);
                documentIndexer.CommitChanges();
                int docs = documentIndexer.IndexSearcher.reader_ForNUnit.NumDocs();
                Assert.IsTrue(docs == 0);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + ". " + ex.StackTrace);
            }
        }

 

        [SetUp]
        public void ResetContract()
        {
            Directory.CreateDirectory(_luceneTempIndexesDirectory);
            contractFailed = false;
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetHandled();
                e.SetUnwind();
                contractFailed = true;
            };
        }

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.InitializeDefaultExtensionPoints();
		    var solutionKey = ServiceLocator.Resolve<SolutionKey>();
		    var newSolutionKey = new SolutionKey(solutionKey.SolutionId, solutionKey.SolutionPath, _luceneTempIndexesDirectory, solutionKey.SandoAssemblyDirectoryPath);
		    ServiceLocator.RegisterInstance(newSolutionKey);
		}

		[TearDown]
		public void CloseDocumentIndexer()
		{
			if(documentIndexer != null)
                documentIndexer.Dispose(true);
		}

		private bool contractFailed;
		private DocumentIndexer documentIndexer;
	}
}
