using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class FieldDocument : SandoDocument
	{
		public FieldDocument(FieldElement fieldElement)
			: base(fieldElement)
		{	
		}

		protected override void AddDocumentFields()
		{
			FieldElement fieldElement = (FieldElement) programElement;
			document.Add(new Field("Name", fieldElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", fieldElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DataType", fieldElement.FieldType, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", fieldElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
		}
	}
}
