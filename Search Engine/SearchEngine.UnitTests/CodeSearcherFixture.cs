
namespace Sando.SearchEngine.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Sando.SearchEngine;
    using Lucene.Net.Analysis;
    using Sando.Core;
    using Sando.Indexer;
    using Sando.Indexer.Documents;
    using Lucene.Net.Search;
	using Sando.Indexer.Searching;

    [TestFixture]
    public class CodeSearcherFixture
    {
		private DocumentIndexer Indexer;
    	private string IndexerPath;


    	[Test]
        public void TestCreateCodeSearcher()
        {
            SimpleAnalyzer analyzer = new SimpleAnalyzer();
    		var indexer = DocumentIndexerFactory.CreateIndexer(System.IO.Path.GetTempPath() + "luceneindexer", AnalyzerType.Standard);
			//TODO - How do we get an instance of IIndexerSearcher?
			//FYI - use this IndexerSearcherFactory.CreateSearcher
            Assert.DoesNotThrow(() => new CodeSearcher( null ));
			indexer.Dispose();
        }

        [Test]     
        public void PerformBasicSearch()
        {        	
			//TODO - get an IIndexerSearcher from the Indexer project
			//FYI - use this IndexerSearcherFactory.CreateSearcher
        	CodeSearcher cs = new CodeSearcher(null);            
            List<CodeSearchResult> result = cs.Search("SimpleName");
            Assert.AreEqual(2, result.Count);                                 
        }

		[Test]
		public void TestSearchWithCache()
		{			
			var cs = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(IndexerPath));
			List<CodeSearchResult> result = cs.Search("SimpleName");
			var sameResult = cs.Search("SimpleName");
			Assert.IsTrue(result==sameResult);
			var different = cs.Search("Simple Name 3");
			Assert.IsTrue(result != different);
			var alsoDifferent = cs.Search("Simple Name 4");
			Assert.IsTrue(alsoDifferent != different);
			var original = cs.Search("Simple Name");
			Assert.IsTrue(result != original);

		}

		[TestFixtureSetUp]
    	public void CreateIndexer()
		{
			IndexerPath = System.IO.Path.GetTempPath() + "luceneindexer";
			Indexer = DocumentIndexerFactory.CreateIndexer(IndexerPath, AnalyzerType.Standard);
    		ClassElement classElement = new ClassElement()
    		                            	{
    		                            		AccessLevel = Core.AccessLevel.Public,
    		                            		DefinitionLineNumber = 11,
    		                            		ExtendedClasses = "SimpleClassBase",
    		                            		FullFilePath = "C:/Projects/SimpleClass.cs",
    		                            		Id = Guid.NewGuid(),
    		                            		ImplementedInterfaces = "IDisposable",
    		                            		Name = "SimpleName",
    		                            		Namespace = "Sanod.Indexer.UnitTests"
    		                            	};
    		SandoDocument sandoDocument = ClassDocument.Create(classElement);
    		Indexer.AddDocument(sandoDocument);
    		MethodElement methodElement = new MethodElement()
    		                              	{
    		                              		AccessLevel = Core.AccessLevel.Protected,
    		                              		Name = "SimpleName",
    		                              		Id = Guid.NewGuid(),
    		                              		ReturnType = "Void",
    		                              		ClassId = Guid.NewGuid(),
												FullFilePath = "C:/stuff"
    		                              	};
    		sandoDocument = MethodDocument.Create(methodElement);
    		Indexer.AddDocument(sandoDocument);
    		Indexer.CommitChanges();
    	}

		[TestFixtureTearDown]
    	public void ShutdownIndexer()
    	{
			Indexer.Dispose();   
    	}
    }
}
