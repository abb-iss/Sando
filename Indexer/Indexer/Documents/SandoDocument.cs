using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public abstract class SandoDocument
	{
		protected SandoDocument(ProgramElement programElement)
		{
			this.programElement = programElement;
		}

		public Document GetDocument()
		{
			if(document == null)
			{
				document = new Document();
				document.Add(new Field("Id", programElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
				document.Add(new Field("ProgramElementType", programElement.ProgramElementType.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
				AddDocumentFields();
			}
			return document;
		}

		protected abstract void AddDocumentFields();

		protected ProgramElement programElement;
		protected Document document;
	}
}
