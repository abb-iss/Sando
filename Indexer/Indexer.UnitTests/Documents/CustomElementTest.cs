using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.ExtensionContracts.ProgramElementContracts;
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
            var customSandoDocument = new SandoDocument(MyCustomProgramElementForTesting.GetLuceneDocument());
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
            var customSandoDocument = new SandoDocument(MyCustomProgramElementForTesting.GetProgramElement());
            var luceneDocumentWithCustomFields = customSandoDocument.GetDocument();
            Assert.IsTrue(luceneDocumentWithCustomFields!=null);
        }

        [Test]
        public void GetCustomDocumentFromFactoryTest()
        {
            var element = DocumentFactory.Create(MyCustomProgramElementForTesting.GetProgramElement());
            Assert.IsTrue(element != null);
        }

        [Test]
        public void RoundTrip()
        {
            var element = DocumentFactory.Create(MyCustomProgramElementForTesting.GetProgramElement());
            var generatedDocumentFromElement = element.GetDocument();
            var prefabLuceneDocument = MyCustomProgramElementForTesting.GetLuceneDocument();
            foreach (var property in MyCustomProgramElementForTesting.GetProgramElement().GetCustomProperties())
            {
                if (!property.Name.Equals(ProgramElement.CustomTypeTag))
                {
                    Field field1 = generatedDocumentFromElement.GetField(property.Name);
                    Field field2 = prefabLuceneDocument.GetField(property.Name);
                    Assert.IsTrue(field1.StringValue().Equals(field2.StringValue()));
                }
            }
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
