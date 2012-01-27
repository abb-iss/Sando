using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class DocCommentDocument : SandoDocument
	{
		public static DocCommentDocument Create(DocCommentElement docCommentElement)
		{
			if(docCommentElement == null)
				throw new ArgumentException("DocCommentElement cannot be null");
			if(docCommentElement.Body == null)
				throw new ArgumentException("Comment body cannot be null");

			return new DocCommentDocument(docCommentElement);
		}

		protected override void AddDocumentFields()
		{
			DocCommentElement docCommentElement = (DocCommentElement)programElement;
			document.Add(new Field("Body", docCommentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
		}

		private DocCommentDocument(DocCommentElement docCommentElement)
			: base(docCommentElement)
		{	
		}
	}
}
