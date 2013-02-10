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
    	[Test]
		public void DocumentIndexer_ConstructorDoesNotThrowWhenValidData()
		{
			try
			{
				_documentIndexer = new DocumentIndexer();
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentIndexer_AddDocumentDoesNotThrowWhenValidData()
		{
			try
			{
                _documentIndexer = new DocumentIndexer();
				ClassElement classElement = SampleProgramElementFactory.GetSampleClassElement();
				SandoDocument sandoDocument = DocumentFactory.Create(classElement);
				Assert.NotNull(sandoDocument);
				Assert.NotNull(sandoDocument.GetDocument());
				_documentIndexer.AddDocument(sandoDocument);
				_documentIndexer.CommitChanges();
			}
			catch(Exception ex)
			{
				Assert.Fail(ex.Message + ". " + ex.StackTrace);
			}
		}

		[Test]
		public void DocumentIndexer_AddDocumentThrowsWhenProgramElementIsNull()
		{
			_documentIndexer = new DocumentIndexer();
			try
			{
				_documentIndexer.AddDocument(null);
			}
			catch
			{
				//contract exception catched here
			}
			Assert.True(_contractFailed, "Contract should fail!");
		}

		[Test]
		public void DocumentIndexer_CommitChangesTriggersNotifyAboutIndexUpdateOnIndexUpdateListeners()
		{
			try
			{
				_documentIndexer = new DocumentIndexer();
				TestIndexUpdateListener testIndexUpdateListener = new TestIndexUpdateListener();
				_documentIndexer.AddIndexUpdateListener(testIndexUpdateListener);
				Assert.True(testIndexUpdateListener.NotifyCalled == false, "Notify flag set without NotifyAboutIndexUpdate call!");
				_documentIndexer.CommitChanges();
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
            try
            {                                
                TestUtils.ClearDirectory(_luceneTempIndexesDirectory);
                _documentIndexer = new DocumentIndexer();
                MethodElement sampleMethodElement = SampleProgramElementFactory.GetSampleMethodElement();
                _documentIndexer.AddDocument(DocumentFactory.Create(sampleMethodElement));
                _documentIndexer.CommitChanges();
                int numDocs = _documentIndexer.IndexSearcher.reader_ForNUnit.NumDocs();
                Assert.IsTrue(numDocs == 1);
                _documentIndexer.DeleteDocuments(sampleMethodElement.FullFilePath);
                _documentIndexer.CommitChanges();
                int docs = _documentIndexer.IndexSearcher.reader_ForNUnit.NumDocs();
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
            _contractFailed = false;
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetHandled();
                e.SetUnwind();
                _contractFailed = true;
            };
        }

		[TestFixtureSetUp]
		public void SetUp()
		{
		    _luceneTempIndexesDirectory = Path.Combine(Path.GetTempPath(), "basic");
		    if (!Directory.Exists(_luceneTempIndexesDirectory))
		        Directory.CreateDirectory(_luceneTempIndexesDirectory);
            TestUtils.InitializeDefaultExtensionPoints();
		    var solutionKey = ServiceLocator.Resolve<SolutionKey>();
		    var newSolutionKey = new SolutionKey(solutionKey.SolutionId, solutionKey.SolutionPath, _luceneTempIndexesDirectory);
		    ServiceLocator.RegisterInstance(newSolutionKey);
		    ServiceLocator.RegisterType<Analyzer, SimpleAnalyzer>();
		}

		[TearDown]
		public void CloseDocumentIndexer()
		{
			if(_documentIndexer != null)
                _documentIndexer.Dispose(true);
		}

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (Directory.Exists(_luceneTempIndexesDirectory))
                Directory.Delete(_luceneTempIndexesDirectory, true);
        }
        
        private string _luceneTempIndexesDirectory;
        private bool _contractFailed;
		private DocumentIndexer _documentIndexer;
	}
}
