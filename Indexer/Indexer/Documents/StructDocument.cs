using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
    public class StructDocument : SandoDocument
    {
        public StructDocument(StructElement element): base(element)
        {
            
        }

        public StructDocument(Document structDocument): base(structDocument)
        {
            
        }

        protected override void AddDocumentFields()
        {
            StructElement structElement = (StructElement)programElement;
            document.Add(new Field(SandoField.Namespace.ToString(), structElement.Namespace.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.AccessLevel.ToString(), structElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.ExtendedClasses.ToString(), structElement.ExtendedStructs.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));            
            document.Add(new Field(SandoField.Modifiers.ToString(), structElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
        }

        protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
        {
            string namespaceName = document.GetField(SandoField.Namespace.ToString()).StringValue().ToSandoDisplayable();
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
            string extendedClasses = document.GetField(SandoField.ExtendedClasses.ToString()).StringValue().ToSandoDisplayable();            
            string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
            return new StructElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, modifiers);
        }
    }
}
