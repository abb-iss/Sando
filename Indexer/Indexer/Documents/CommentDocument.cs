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

		protected override void AddDocumentFields()
		{
			CommentElement commentElement = (CommentElement) programElement;
			document.Add(new Field(SandoField.Body.ToString(), commentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
		}

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
		    string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue();
			return new CommentElement(name, definitionLineNumber, fullFilePath, snippet, body);
		}
	}
}
