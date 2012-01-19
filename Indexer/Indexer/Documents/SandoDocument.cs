using Lucene.Net.Documents;

namespace Sando.Indexer.Documents
{
	public abstract class SandoDocument
	{
		public Document GetDocument()
		{
			return document;
		}

		protected abstract void CreateDocument();

		protected Document document;
	}
}
