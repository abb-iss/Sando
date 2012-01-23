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
			if(String.IsNullOrWhiteSpace(docCommentElement.FileName)) 
				throw new ArgumentException("File name cannot be null");
			if(String.IsNullOrWhiteSpace(docCommentElement.FullFilePath)) 
				throw new ArgumentException("Full file path name cannot be null");
			if(docCommentElement.Id == null) 
				throw new ArgumentException("Doc comment id cannot be null");

			return new DocCommentDocument(docCommentElement);
		}

		protected override void CreateDocument()
		{
			document = new Document();
			DocCommentElement docCommentElement = (DocCommentElement)programElement;
			document.Add(new Field("Id", docCommentElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Body", docCommentElement.Body, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("DefinitionLineNumber", docCommentElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("FileName", docCommentElement.FileName, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("FullFilePath", docCommentElement.FullFilePath, Field.Store.YES, Field.Index.NO));
		}

		private DocCommentDocument(DocCommentElement docCommentElement)
			: base(docCommentElement)
		{	
		}
	}
}
