using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class PropertyDocument : SandoDocument
	{
		public PropertyDocument(PropertyElement propertyElement)
			: base(propertyElement)
		{	
		}

		protected override void AddDocumentFields()
		{
			PropertyElement propertyElement = (PropertyElement) programElement;
			document.Add(new Field(SandoField.AccessLevel.ToString(), propertyElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.Body.ToString(), propertyElement.Body, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.DataType.ToString(), propertyElement.PropertyType, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ClassId.ToString(), propertyElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
		}
	}
}
