using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;

namespace Sando.Indexer.UnitTests.Documents
{
    public class MyCustomProgramElementForTesting: ProgramElement 
    {
        public MyCustomProgramElementForTesting(string name, int definitionLineNumber, string fullFilePath, string snippet) : base(name, definitionLineNumber, fullFilePath, snippet)
        {
        }
        
        [CustomIndexField]
        public String A { get; set; }
        [CustomIndexField]
        public String B { get; set; }
        [CustomIndexField]
        public String C { get; set; }

        public static Document GetLuceneDocument()
        {
            var document = new Document();
            document.Add(new Field("A", "A's value", Field.Store.YES, Field.Index.NO));
            document.Add(new Field("B", "B's value", Field.Store.YES, Field.Index.NO));
            document.Add(new Field("C", "C's value", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Id.ToString(), "xycasdf3k34", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Name.ToString(), "customThingName", Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(SandoField.ProgramElementType.ToString(), ProgramElementType.Custom.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.FullFilePath.ToString(), @"C:\stuff\place.txt", Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.DefinitionLineNumber.ToString(), "123", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Snippet.ToString(), "The text of the custom thing.", Field.Store.YES, Field.Index.NO));
            document.Add(new Field(ProgramElement.CustomTypeTag, typeof(MyCustomProgramElementForTesting).AssemblyQualifiedName, Field.Store.YES, Field.Index.NO));
            return document;
        }

        public static MyCustomProgramElementForTesting GetProgramElement()
        {
            var element =  new MyCustomProgramElementForTesting("customThingName", 123, @"C:\stuff\place.txt", "The text of the custom thing.");
            element.A = "A's value";
            element.B = "B's value";
            element.C = "C's value";
            return element;
        }



        public override ProgramElementType ProgramElementType
        {
            get { return ProgramElementType.Custom; }
        }
    }
}
