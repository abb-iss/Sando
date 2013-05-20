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

        public override List<Field> GetFieldsForLucene()
        {
            List<Field> fields = new List<Field>();
            StructElement structElement = (StructElement)programElement;
            fields.Add(new Field(SandoField.Namespace.ToString(), structElement.Namespace.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            AddField(fields, new Field(SandoField.Body.ToString(), structElement.Body.ToSandoSearchable(), Field.Store.NO, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.AccessLevel.ToString(), structElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.ExtendedClasses.ToString(), structElement.ExtendedStructs.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.Modifiers.ToString(), structElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
            return fields;
        }

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, int definitionColumnNumber, string snippet, Document document)
        {
            string namespaceName = document.GetField(SandoField.Namespace.ToString()).StringValue().ToSandoDisplayable();
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue().ToSandoDisplayable();
            string extendedClasses = document.GetField(SandoField.ExtendedClasses.ToString()).StringValue().ToSandoDisplayable();            
            string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
            return new object[] { name, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet, accessLevel, namespaceName, body, extendedClasses, modifiers };
        }
    }
}
