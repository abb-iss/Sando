using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Collections.Generic;

namespace Sando.Indexer.Documents
{
	public class TextLineDocument : SandoDocument
	{
		public TextLineDocument(TextLineElement classElement)
			: base(classElement)
		{
		}

		public TextLineDocument(Document document)
			: base(document)
		{
		}

        public override List<Field> GetFieldsForLucene()
		{
            List<Field> fields = new List<Field>();
			var textLineElement = (TextLineElement) programElement;
            fields.Add(new Field(SandoField.Body.ToString(), textLineElement.Body, Field.Store.NO, Field.Index.ANALYZED));
            return fields;
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, int definitionColumnNumber, string snippet, Document document)
		{
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue();
            return new object[] { name, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet, body };
		}
	}
}
