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
			document.Add(new Field(SandoField.Namespace.ToString(), enumElement.Namespace, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.AccessLevel.ToString(), enumElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.Values.ToString(), enumElement.Values, Field.Store.YES, Field.Index.ANALYZED));
		}
	}
}
