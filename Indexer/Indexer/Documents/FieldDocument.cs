using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class FieldDocument : SandoDocument
	{
		public static FieldDocument Create(FieldElement fieldElement)
		{
			return new FieldDocument(fieldElement);
		}

		protected override void CreateDocument()
		{
			document = new Document();
			FieldElement fieldElement = (FieldElement) programElement;
			document.Add(new Field("Id", fieldElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Name", fieldElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", fieldElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DefinitionLineNumber", fieldElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("FieldType", fieldElement.FieldType, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", fieldElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
		}

		private FieldDocument(FieldElement fieldElement)
			: base(fieldElement)
		{	
		}
	}
}
