using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class DocCommentDocument : SandoDocument
	{
		public DocCommentDocument(DocCommentElement docCommentElement)
			: base(docCommentElement)
		{
		}

		public DocCommentDocument(Document document)
			: base(document)
		{
		}

		protected override void AddDocumentFields()
		{
			DocCommentElement docCommentElement = (DocCommentElement)programElement;
			document.Add(new Field(SandoField.Body.ToString(), docCommentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.DocumentedElementId.ToString(), docCommentElement.DocumentedElementId.ToString(), Field.Store.YES, Field.Index.NO));
		}

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue();
			Guid documentedElementId = new Guid(document.GetField(SandoField.DocumentedElementId.ToString()).StringValue());
			return new DocCommentElement(name, definitionLineNumber, fullFilePath, snippet, body, documentedElementId);
		}
	}
}
