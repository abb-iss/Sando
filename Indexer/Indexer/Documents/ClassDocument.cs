using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class ClassDocument : SandoDocument
	{
		public ClassDocument(ClassElement classElement)
			: base(classElement)
		{
		}

		protected override void AddDocumentFields()
		{
			ClassElement classElement = (ClassElement) programElement;
			document.Add(new Field(SandoField.Namespace.ToString(), classElement.Namespace, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.AccessLevel.ToString(), classElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.ExtendedClasses.ToString(), classElement.ExtendedClasses, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ImplementedInterfaces.ToString(), classElement.ImplementedInterfaces, Field.Store.YES, Field.Index.ANALYZED));
		}
	}
}
