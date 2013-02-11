using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class CommentDocument : SandoDocument
	{
		public CommentDocument(CommentElement classElement)
			: base(classElement)
		{
		}

		public CommentDocument(Document document)
			: base(document)
		{
		}

		public override void AddFieldsToDocument(Document luceneDocument)
		{
			CommentElement commentElement = (CommentElement) programElement;
            luceneDocument.Add(new Field(SandoField.Body.ToString(), commentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
		    string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue();
			return new object[]{name, definitionLineNumber, fullFilePath, snippet, body};
		}
	}
}
