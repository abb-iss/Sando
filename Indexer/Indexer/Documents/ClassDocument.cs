using Lucene.Net.Documents;

namespace Sando.Indexer.Documents
{
	public class ClassDocument : SandoDocument
	{
		public static ClassDocument Create(string className)//Add all required fields
		{
			return new ClassDocument(className);
		}

		public virtual string ClassName { get; protected set; }

		private ClassDocument(string className)
		{
			ClassName = className;
			CreateDocument();
		}

		protected override void CreateDocument()
		{
			document = new Document();
			document.Add(new Field("className", ClassName, Field.Store.YES, Field.Index.ANALYZED));
		}
	}
}
