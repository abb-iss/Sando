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
			document.Add(new Field("Name", classElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Namespace", classElement.Namespace, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", classElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("ExtendedClasses", classElement.ExtendedClasses, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ImplementedInterfaces", classElement.ImplementedInterfaces, Field.Store.NO, Field.Index.ANALYZED));
		}

		
	}
}
