using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Collections.Generic;
using System.Linq;

namespace Sando.Indexer.Documents
{
	public class MethodDocument : SandoDocument
	{
		public MethodDocument(MethodElement methodElement)
			: base(methodElement)
		{	
		}

		public MethodDocument(Document document)
			: base(document)
		{
		}

		protected override void AddDocumentFields()
		{
			MethodElement methodElement = (MethodElement)programElement;
			document.Add(new Field(SandoField.AccessLevel.ToString(), methodElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.Arguments.ToString(), methodElement.Arguments.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.Body.ToString(), methodElement.Body.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ClassId.ToString(), methodElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.ClassName.ToString(), methodElement.ClassName.ToSandoSearchable(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.ReturnType.ToString(), methodElement.ReturnType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.Modifiers.ToString(), methodElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.IsConstructor.ToString(), methodElement.IsConstructor.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.UnsplitIdentifiers.ToString(), CreateUnsplitTermsField(methodElement), Field.Store.NO, Field.Index.ANALYZED));
		}

        private static string CreateUnsplitTermsField(MethodElement methodElement)
        {
            List<string> idList = new List<string>();
            string identifierWords = methodElement.Name + " " + methodElement.Body + " " + methodElement.Arguments + " " + methodElement.ReturnType;
            string[] idWords = identifierWords.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            idList.AddRange(idWords);
            return string.Join(" ", idList.Distinct().ToArray());
        }

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string arguments = document.GetField(SandoField.Arguments.ToString()).StringValue().ToSandoDisplayable();
			string returnType = document.GetField(SandoField.ReturnType.ToString()).StringValue().ToSandoDisplayable();
			string body = document.GetField(SandoField.Body.ToString()).StringValue();
			Guid classId = new Guid(document.GetField(SandoField.ClassId.ToString()).StringValue());
			string className = document.GetField(SandoField.ClassName.ToString()).StringValue().ToSandoDisplayable();
			string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
			bool isConstructor = bool.Parse(document.GetField(SandoField.IsConstructor.ToString()).StringValue());
            return base.ReadProgramElementFromDocument(GetMyType(), new object[] { name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, classId, className, modifiers, isConstructor });
		}
	}
}
