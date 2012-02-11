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
			document.Add(new Field("Name", propertyElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", propertyElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("Body", propertyElement.Body, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("DataType", propertyElement.PropertyType, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", propertyElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
		}
	}
}
