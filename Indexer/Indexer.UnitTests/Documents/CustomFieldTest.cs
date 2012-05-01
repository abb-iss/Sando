using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;

namespace Sando.Indexer.UnitTests.Documents
{
    [TestFixture]
    public class CustomFieldTest
    {
        [Test]
        public void LuceneDocToCustomProgramElement()
        {
            //test ReadProgramElementFromDocument  
            var document = new MethodDocument(MyCustomMethodElementForTesting.GetLuceneDocument());
            var customProgramElement = document.ReadProgramElementFromDocument();
            var myCustomProgramElementForTesting = customProgramElement as MyCustomMethodElementForTesting;
            Assert.IsTrue(myCustomProgramElementForTesting != null);
            Assert.IsTrue(myCustomProgramElementForTesting.Boom.Equals("Ba dow"));
        }



        [Test]
        public void CustomDocumentToLuceneDocument()
        {
            //test AddDocumentFields
            var customSandoDocument = new MethodDocument(MyCustomMethodElementForTesting.GetMethodElement());
            var luceneDocumentWithCustomFields = customSandoDocument.GetDocument();
            Assert.IsTrue(luceneDocumentWithCustomFields != null);
            Assert.IsTrue(luceneDocumentWithCustomFields.GetField("Boom") != null);
            Assert.IsTrue(luceneDocumentWithCustomFields.GetField("Boom").StringValue().Equals("Ba dow"));
        }

    }

    public class MyCustomMethodElementForTesting : MethodElement
    {
        public const string CustomTypeTag = "CustomType";

        public String CustomType { get { return GetType().AssemblyQualifiedName; } }


        public MyCustomMethodElementForTesting(string name, int definitionLineNumber, string fullFilePath, string snippet, AccessLevel accessLevel, string arguments, string returnType, string body, Guid classId, string className, string modifiers, bool isConstructor) 
            : base(name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, classId, className, modifiers, isConstructor) 
        {
        }

        [CustomIndexFieldAttribute("Boom")]
        public string Boom { get; set; }

        public static MethodElement GetMethodElement()
        {
            var methodElement = new MyCustomMethodElementForTesting("multiply", 12, "C:/Projects/SampleClass.cs",
                                                "private int multiply(int number, int factor)\n{\nreturn number * factor;\n};",
                                                AccessLevel.Public, "int number, int factor", "int",
                                                "return number * factor;", new Guid(),
                                                            "SampleCLass", "", false);
            methodElement.Boom = "Ba dow";
            return methodElement;
        }

        public static Document GetLuceneDocument()
        {
            var document = new Document();
            document.Add(new Field(SandoField.Id.ToString(), "xycasdf3k34", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Name.ToString(), "customThingName", Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field("Boom", "Ba dow", Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(SandoField.ProgramElementType.ToString(), ProgramElementType.Custom.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.FullFilePath.ToString(), @"C:\stuff\place.txt", Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.DefinitionLineNumber.ToString(), "123", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Snippet.ToString(), "The text of the custom thing.", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.AccessLevel.ToString(), "Public", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Arguments.ToString(), "int number, int factor", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Body.ToString(), "return number * factor;", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.ClassId.ToString(), "0f8fad5b-d9cb-469f-a165-70867728950e", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.ClassName.ToString(), "SampleCLass", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.ReturnType.ToString(), "int", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Modifiers.ToString(), "", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.IsConstructor.ToString(), "false", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(ProgramElement.CustomTypeTag, typeof(MyCustomMethodElementForTesting).AssemblyQualifiedName, Field.Store.YES, Field.Index.NO));
            return document;
        }

    }
}
