using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class EnumDocument : SandoDocument
	{
		public EnumDocument(EnumElement enumElement)
			: base(enumElement)
		{	
		}

		protected override void AddDocumentFields()
		{
			EnumElement enumElement = (EnumElement) programElement;
			document.Add(new Field("Name", enumElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Namespace", enumElement.Namespace, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", enumElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("Values", enumElement.Values, Field.Store.NO, Field.Index.ANALYZED));
		}
	}
}
