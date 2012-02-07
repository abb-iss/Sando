using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class FieldDocument : SandoDocument
	{
		public static FieldDocument Create(FieldElement fieldElement)
		{
			if(fieldElement == null)
				throw new ArgumentException("FieldElement cannot be null");
			if(fieldElement.ClassId == null)
				throw new ArgumentException("Class id cannot be null");
			if(String.IsNullOrWhiteSpace(fieldElement.FieldType))
				throw new ArgumentException("Field type cannot be null");
			if(String.IsNullOrWhiteSpace(fieldElement.Name))
				throw new ArgumentException("Field name cannot be null");

			return new FieldDocument(fieldElement);
		}

		protected override void AddDocumentFields()
		{
			FieldElement fieldElement = (FieldElement) programElement;
			document.Add(new Field("Name", fieldElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", fieldElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DataType", fieldElement.FieldType, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", fieldElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
		}

		private FieldDocument(FieldElement fieldElement)
			: base(fieldElement)
		{	
		}
	}
}
