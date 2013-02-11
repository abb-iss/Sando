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

		public override void AddFieldsToDocument(Document luceneDocument)
		{
			MethodPrototypeElement methodPrototypeElement = (MethodPrototypeElement)programElement;
            luceneDocument.Add(new Field(SandoField.AccessLevel.ToString(), methodPrototypeElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            luceneDocument.Add(new Field(SandoField.Arguments.ToString(), methodPrototypeElement.Arguments.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            luceneDocument.Add(new Field(SandoField.ReturnType.ToString(), methodPrototypeElement.ReturnType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            luceneDocument.Add(new Field(SandoField.IsConstructor.ToString(), methodPrototypeElement.IsConstructor.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string arguments = document.GetField(SandoField.Arguments.ToString()).StringValue().ToSandoDisplayable();
			string returnType = document.GetField(SandoField.ReturnType.ToString()).StringValue().ToSandoDisplayable();
			bool isConstructor = bool.Parse(document.GetField(SandoField.IsConstructor.ToString()).StringValue().ToSandoDisplayable());
            return new object[] { name, definitionLineNumber, returnType, accessLevel, arguments, fullFilePath, snippet, isConstructor };			
		}
	}
}
