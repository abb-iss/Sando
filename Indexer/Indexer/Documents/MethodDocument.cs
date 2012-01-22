using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class MethodDocument : SandoDocument
	{
		public static MethodDocument Create(MethodElement methodElement)
		{
			return new MethodDocument(methodElement);
		}

		protected override void CreateDocument()
		{
			document = new Document();
			MethodElement methodElement = (MethodElement)programElement;
			document.Add(new Field("Id", methodElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Name", methodElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", methodElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DefinitionLineNumber", methodElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Arguments", methodElement.Arguments, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Body", methodElement.Body, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", methodElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("ReturnType", methodElement.ReturnType, Field.Store.NO, Field.Index.ANALYZED));
		}

		private MethodDocument(MethodElement methodElement)
			: base(methodElement)
		{	
		}
	}
}
