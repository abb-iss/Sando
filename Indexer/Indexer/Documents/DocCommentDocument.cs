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
			document.Add(new Field("Body", docCommentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
		}
	}
}
