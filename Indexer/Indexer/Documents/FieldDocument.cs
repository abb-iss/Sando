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
			document.Add(new Field(SandoField.AccessLevel.ToString(), fieldElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.DataType.ToString(), fieldElement.FieldType, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ClassId.ToString(), fieldElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
		}
	}
}
