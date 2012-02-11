using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class CommentDocument : SandoDocument
	{
		public CommentDocument(CommentElement classElement)
			: base(classElement)
		{
		}

		protected override void AddDocumentFields()
		{
			CommentElement commentElement = (CommentElement) programElement;
			document.Add(new Field("Body", commentElement.Body, Field.Store.YES, Field.Index.ANALYZED));
		}
	}
}
