using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching;
using Sando.Parser;

namespace Sando.Indexer.UnitTests.Documents
{
    [TestFixture]
    public class CustomElementTest
    {

        [Test]
        public void LuceneDocToCustomProgramElement()
        {
            //test ReadProgramElementFromDocument            
            var customSandoDocument = new CustomDocument(MyCustomProgramElementForTesting.GetLuceneDocument(), typeof(MyCustomProgramElementForTesting));
            var customProgramElement = customSandoDocument.ReadProgramElementFromDocument();
            var myCustomProgramElementForTesting = customProgramElement as MyCustomProgramElementForTesting;
            Assert.IsTrue(myCustomProgramElementForTesting != null);
            Assert.IsTrue(myCustomProgramElementForTesting.A.Equals("A's value"));
            Assert.IsTrue(myCustomProgramElementForTesting.B.Equals("B's value"));
            Assert.IsTrue(myCustomProgramElementForTesting.C.Equals("C's value"));
        }



        [Test]
        public void CustomDocumentToLuceneDocument()
        {
            //test AddDocumentFields
            var customSandoDocument = new CustomDocument(MyCustomProgramElementForTesting.GetProgramElement(), typeof(MyCustomProgramElementForTesting));
            var luceneDocumentWithCustomFields = customSandoDocument.GetDocument();
            Assert.IsTrue(luceneDocumentWithCustomFields!=null);
        }

        [SetUp]
        public static void InitializeExtensionPoints()
        {
            ExtensionPointsRepository extensionPointsRepository = ExtensionPointsRepository.Instance;
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".cs" }, new SrcMLCSharpParser());
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".h", ".cpp", ".cxx" }, new SrcMLCppParser());
            extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter());           
            extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());
        }

    }
}
