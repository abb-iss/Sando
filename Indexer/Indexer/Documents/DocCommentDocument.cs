using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class DocCommentDocument : SandoDocument
	{
		public DocCommentDocument(DocCommentElement docCommentElement)
			: base(docCommentElement)
		{
		}

		protected override void AddDocumentFields()
		{
			DocCommentElement docCommentElement = (DocCommentElement)programElement;
			document.Add(new Field(SandoField.Body.ToString(), docCommentElement.Body, Field.Store.YES, Field.Index.ANALYZED));
		}
	}
}
