using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Collections.Generic;

namespace Sando.Indexer.Documents
{
	public class EnumDocument : SandoDocument
	{
		public EnumDocument(EnumElement enumElement)
			: base(enumElement)
		{	
		}

		public EnumDocument(Document document)
			: base(document)
		{
		}

        public override List<Field> GetFieldsForLucene()
        {
            var fields = new List<Field>();
			EnumElement enumElement = (EnumElement) programElement;
            fields.Add(new Field(SandoField.Namespace.ToString(), enumElement.Namespace.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.AccessLevel.ToString(), enumElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.Body.ToString(), enumElement.Body.ToSandoSearchable(), Field.Store.NO, Field.Index.ANALYZED));
            return fields;
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, int definitionColumnNumber, string snippet, Document document)
		{
			string namespaceName = document.GetField(SandoField.Namespace.ToString()).StringValue().ToSandoDisplayable();
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue().ToSandoDisplayable();
			if(name == String.Empty) name = ProgramElement.UndefinedName;
            return new object[] { name, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet, accessLevel, namespaceName, body };			
		}
	}
}
