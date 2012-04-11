using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class MethodPrototypeDocument : SandoDocument
	{
		public MethodPrototypeDocument(MethodPrototypeElement methodPrototypeElement)
			: base(methodPrototypeElement)
		{	
		}

		public MethodPrototypeDocument(Document document)
			: base(document)
		{
		}

		protected override void AddDocumentFields()
		{
			MethodPrototypeElement methodPrototypeElement = (MethodPrototypeElement)programElement;
			document.Add(new Field(SandoField.AccessLevel.ToString(), methodPrototypeElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.Arguments.ToString(), methodPrototypeElement.Arguments.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ReturnType.ToString(), methodPrototypeElement.ReturnType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
		}

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue());
			string arguments = document.GetField(SandoField.Arguments.ToString()).StringValue().ToSandoDisplayable();
			string returnType = document.GetField(SandoField.ReturnType.ToString()).StringValue().ToSandoDisplayable();
			return new MethodPrototypeElement(name, definitionLineNumber, returnType, accessLevel, arguments, fullFilePath, snippet);
		}
	}
}
