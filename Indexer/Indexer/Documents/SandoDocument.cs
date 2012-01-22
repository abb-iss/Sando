using Lucene.Net.Documents;
using Sando.Core;
using System.Reflection;

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
			return document;
		}

		protected abstract void CreateDocument();

		protected ProgramElement programElement;
		protected Document document;
	}
}
