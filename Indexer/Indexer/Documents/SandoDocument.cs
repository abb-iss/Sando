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
			CreateDocument();
			return document;
		}

		protected abstract void CreateDocument();

		protected ProgramElement programElement;
		protected Document document;
	}
}
