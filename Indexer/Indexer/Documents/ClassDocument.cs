using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class ClassDocument : SandoDocument
	{
		public ClassDocument(ClassElement classElement)
			: base(classElement)
		{
		}

		public ClassDocument(Document document)
			: base(document)
		{
		}

		protected override void AddDocumentFields()
		{
			ClassElement classElement = (ClassElement) programElement;
			document.Add(new Field(SandoField.Namespace.ToString(), classElement.Namespace, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.AccessLevel.ToString(), classElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.ExtendedClasses.ToString(), classElement.ExtendedClasses, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ImplementedInterfaces.ToString(), classElement.ImplementedInterfaces, Field.Store.YES, Field.Index.ANALYZED));
		}

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			string namespaceName = document.GetField(SandoField.Namespace.ToString()).StringValue();
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue());
			string extendedClasses = document.GetField(SandoField.ExtendedClasses.ToString()).StringValue();
			string implementedInterfaces = document.GetField(SandoField.ImplementedInterfaces.ToString()).StringValue();
			return new ClassElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, implementedInterfaces);
		}
	}
}
