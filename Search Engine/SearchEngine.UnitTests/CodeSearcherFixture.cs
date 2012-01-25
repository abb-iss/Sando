
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
    [TestFixture]
    public class CodeSearcherFixture
    {
        [Test]
        public void TestCreateCodeSearcher()
        {
            SimpleAnalyzer analyzer = new SimpleAnalyzer();           
            Assert.DoesNotThrow(() => new CodeSearcher( System.IO.Path.GetTempPath() +"luceneindexer", analyzer));
        }

        [Test]     
        public void PerformBasicSearch()
        {
            Analyzer analyzer = new SimpleAnalyzer();            
            string indexerPath = System.IO.Path.GetTempPath() +"luceneindexer";
            if (System.IO.Directory.Exists(indexerPath))
            {
                System.IO.Directory.Delete(indexerPath,true);
            }

            DocumentIndexer target = new DocumentIndexer(indexerPath, analyzer);
            ClassElement classElement = new ClassElement()
            {
                AccessLevel = Core.AccessLevel.Public,
                DefinitionLineNumber = 11,
                ExtendedClasses = "SimpleClassBase",
                FileName = "SimpleClass.cs",
                FullFilePath = "C:/Projects/SimpleClass.cs",
                Id = Guid.NewGuid(),
                ImplementedInterfaces = "IDisposable",
                Name = "SimpleName",
                Namespace = "Sanod.Indexer.UnitTests"
            };
            SandoDocument sandoDocument = ClassDocument.Create(classElement);
            target.AddDocument(sandoDocument);
            MethodElement methodElement = new MethodElement()
            {
                AccessLevel = Core.AccessLevel.Protected,
                Name = "SimpleName",
                Id = Guid.NewGuid(),
                ReturnType="Void",
                ClassId=Guid.NewGuid()
            };
            sandoDocument = MethodDocument.Create(methodElement);
            target.AddDocument(sandoDocument);
            target.CommitChanges();
            CodeSearcher cs = new CodeSearcher(indexerPath, new SimpleAnalyzer());            
            List<CodeSearchResult> result = cs.Search("SimpleName");
            Assert.AreEqual(2, result.Count);
            target.Dispose();                        
        }
    }
}
