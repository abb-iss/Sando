using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class ClassDocument : SandoDocument
	{
		public static ClassDocument Create(ClassElement classElement)
		{
			return new ClassDocument(classElement);
		}

		protected override void CreateDocument()
		{
			document = new Document();
			ClassElement classElement = (ClassElement) programElement;
			document.Add(new Field("Id", classElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Name", classElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Namespace", classElement.Namespace, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", classElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DefinitionLineNumber", classElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("ExtendedClasses", classElement.ExtendedClasses, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ImplementedInterfaces", classElement.ImplementedInterfaces, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("FileName", classElement.FileName, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("FullFilePath", classElement.FullFilePath, Field.Store.YES, Field.Index.NO));
		}

		private ClassDocument(ClassElement classElement)
			: base(classElement)
		{	
		}
	}
}
