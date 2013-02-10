using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching;
using Sando.UnitTestHelpers;
using UnitTestHelpers;

namespace Sando.SearchEngine.UnitTests
{

    [TestFixture]
    public class CodeSearcherFixture
    {
		private DocumentIndexer _indexer;
    	private string _indexerPath;
		private SolutionKey _solutionKey;


    	[Test]
        public void TestCreateCodeSearcher()
        {
            Assert.DoesNotThrow(() => new CodeSearcher( null ));            
        }

        [Test]     
        public void PerformBasicSearch()
        {
			var indexerSearcher = new IndexerSearcher();
        	CodeSearcher cs = new CodeSearcher(indexerSearcher);            
            List<CodeSearchResult> result = cs.Search("SimpleName");
            Assert.True(result.Count > 0);                                 
        }

		[TestFixtureSetUp]
    	public void CreateIndexer()
		{
			TestUtils.InitializeDefaultExtensionPoints();

			_indexerPath = Path.GetTempPath() + "luceneindexer";
		    Directory.CreateDirectory(_indexerPath);
			_solutionKey = new SolutionKey(Guid.NewGuid(), "C:/SolutionPath", _indexerPath);
            ServiceLocator.RegisterInstance(_solutionKey);
            ServiceLocator.RegisterInstance<Analyzer>(new SimpleAnalyzer());
            _indexer = new DocumentIndexer();
            ServiceLocator.RegisterInstance(_indexer);

    		ClassElement classElement = SampleProgramElementFactory.GetSampleClassElement(
				accessLevel: AccessLevel.Public,
				definitionLineNumber: 11,
				extendedClasses: "SimpleClassBase",
				fullFilePath: "C:/Projects/SimpleClass.cs",
				implementedInterfaces: "IDisposable",
				name: "SimpleName",
				namespaceName: "Sanod.Indexer.UnitTests"
    		);
    		SandoDocument sandoDocument = DocumentFactory.Create(classElement);
    		_indexer.AddDocument(sandoDocument);
			MethodElement methodElement = SampleProgramElementFactory.GetSampleMethodElement(
				accessLevel: AccessLevel.Protected,
    		    name: "SimpleName",
				returnType: "Void",
				fullFilePath: "C:/stuff"
			);
    		sandoDocument = DocumentFactory.Create(methodElement);
    		_indexer.AddDocument(sandoDocument);
    		_indexer.CommitChanges();
    	}

		[TestFixtureTearDown]
    	public void ShutdownIndexer()
    	{
			_indexer.ClearIndex();
			_indexer.CommitChanges();
			_indexer.Dispose(true);   
    	}
    }
}
