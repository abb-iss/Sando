using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class EnumDocument : SandoDocument
	{
		public static EnumDocument Create(EnumElement enumElement)
		{
			if(enumElement == null)
				throw new ArgumentException("EnumElement cannot be null");
			if(String.IsNullOrWhiteSpace(enumElement.Name))
				throw new ArgumentException("Enum name cannot be null");

			return new EnumDocument(enumElement);
		}

		protected override void AddDocumentFields()
		{
			EnumElement enumElement = (EnumElement) programElement;
			document.Add(new Field("Name", enumElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Namespace", enumElement.Namespace ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", enumElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("Values", enumElement.Values ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
		}

		private EnumDocument(EnumElement enumElement)
			: base(enumElement)
		{	
		}
	}
}
