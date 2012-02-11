using System;
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
			document.Add(new Field("Namespace", classElement.Namespace, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", classElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("ExtendedClasses", classElement.ExtendedClasses, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("ImplementedInterfaces", classElement.ImplementedInterfaces, Field.Store.YES, Field.Index.ANALYZED));
		}

		
	}
}
