using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Collections.Generic;

namespace Sando.Indexer.Documents
{
	public class MethodPrototypeDocument : SandoDocument
	{
		public MethodPrototypeDocument(MethodPrototypeElement methodPrototypeElement)
			: base(methodPrototypeElement)
		{	
		}

		public MethodPrototypeDocument(Document document)
			: base(document)
		{
		}

        public override List<Field> GetFieldsForLucene()
        {
            List<Field> fields = new List<Field>();
			MethodPrototypeElement methodPrototypeElement = (MethodPrototypeElement)programElement;
            fields.Add(new Field(SandoField.AccessLevel.ToString(), methodPrototypeElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.Arguments.ToString(), methodPrototypeElement.Arguments.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            AddBodyField(fields, new Field(SandoField.Body.ToString(), methodPrototypeElement.RawSource.ToSandoSearchable(), Field.Store.NO, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.ReturnType.ToString(), methodPrototypeElement.ReturnType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.IsConstructor.ToString(), methodPrototypeElement.IsConstructor.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            return fields;
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, int definitionColumnNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string arguments = document.GetField(SandoField.Arguments.ToString()).StringValue().ToSandoDisplayable();
			string returnType = document.GetField(SandoField.ReturnType.ToString()).StringValue().ToSandoDisplayable();
			bool isConstructor = bool.Parse(document.GetField(SandoField.IsConstructor.ToString()).StringValue().ToSandoDisplayable());
            return new object[] { name, definitionLineNumber, definitionColumnNumber, returnType, accessLevel, arguments, fullFilePath, snippet, isConstructor };			
		}
	}
}
