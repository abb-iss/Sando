using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class EnumDocument : SandoDocument
	{
		public EnumDocument(EnumElement enumElement)
			: base(enumElement)
		{	
		}

		public EnumDocument(Document document)
			: base(document)
		{
		}

		protected override void AddDocumentFields()
		{
			EnumElement enumElement = (EnumElement) programElement;
			document.Add(new Field(SandoField.Namespace.ToString(), enumElement.Namespace.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.AccessLevel.ToString(), enumElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.Values.ToString(), enumElement.Values.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
		}

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			string namespaceName = document.GetField(SandoField.Namespace.ToString()).StringValue().ToSandoDisplayable();
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue());
			string values = document.GetField(SandoField.Values.ToString()).StringValue().ToSandoDisplayable();
			return new EnumElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, values);
		}
	}
}
