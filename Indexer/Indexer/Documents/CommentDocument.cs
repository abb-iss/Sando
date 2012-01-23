using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class CommentDocument : SandoDocument
	{
		public static CommentDocument Create(CommentElement commentElement)
		{
			if(commentElement == null)
				throw new ArgumentException("CommentElement cannot be null");
			if(commentElement.Body == null)
				throw new ArgumentException("Comment body cannot be null");
			if(commentElement.Id == null)
				throw new ArgumentException("Comment id cannot be null");
			if(commentElement.MethodId == null)
				throw new ArgumentException("Method id cannot be null");

			return new CommentDocument(commentElement);
		}

		protected override void CreateDocument()
		{
			document = new Document();
			CommentElement commentElement = (CommentElement) programElement;
			document.Add(new Field("Id", commentElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Body", commentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("MethodId", commentElement.MethodId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
		}

		private CommentDocument(CommentElement commentElement)
			: base(commentElement)
		{	
		}
	}
}
