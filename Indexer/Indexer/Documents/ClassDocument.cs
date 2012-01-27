using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class ClassDocument : SandoDocument
	{
		public static ClassDocument Create(ClassElement classElement)
		{
			if(classElement == null) 
				throw new ArgumentException("ClassElement cannot be null");
			if(String.IsNullOrWhiteSpace(classElement.FileName)) 
				throw new ArgumentException("File name cannot be null");
			if(String.IsNullOrWhiteSpace(classElement.FullFilePath)) 
				throw new ArgumentException("Full file path name cannot be null");
			if(classElement.Id == null) 
				throw new ArgumentException("Class id cannot be null");
			if(String.IsNullOrWhiteSpace(classElement.Name)) 
				throw new ArgumentException("Class name cannot be null");
				
			return new ClassDocument(classElement);
		}

		protected override void AddDocumentFields()
		{
			ClassElement classElement = (ClassElement) programElement;
			document.Add(new Field("Name", classElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Namespace", classElement.Namespace ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", classElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DefinitionLineNumber", classElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("ExtendedClasses", classElement.ExtendedClasses ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ImplementedInterfaces", classElement.ImplementedInterfaces ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("FileName", classElement.FileName, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("FullFilePath", classElement.FullFilePath, Field.Store.YES, Field.Index.NO));
		}

		private ClassDocument(ClassElement classElement)
			: base(classElement)
		{	
		}
	}
}
