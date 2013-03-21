using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Collections.Generic;

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

        public override List<Field> GetFieldsForLucene()
		{
            List<Field> fields = new List<Field>();
			MethodElement methodElement = (MethodElement)programElement;
            fields.Add(new Field(SandoField.AccessLevel.ToString(), methodElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.Arguments.ToString(), methodElement.Arguments.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.Body.ToString(), methodElement.Body.ToSandoSearchable(), Field.Store.NO, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.ClassId.ToString(), methodElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.ClassName.ToString(), methodElement.ClassName.ToSandoSearchable(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.ReturnType.ToString(), methodElement.ReturnType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.Modifiers.ToString(), methodElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.IsConstructor.ToString(), methodElement.IsConstructor.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            return fields;
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string arguments = document.GetField(SandoField.Arguments.ToString()).StringValue().ToSandoDisplayable();
			string returnType = document.GetField(SandoField.ReturnType.ToString()).StringValue().ToSandoDisplayable();
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue();
			Guid classId = new Guid(document.GetField(SandoField.ClassId.ToString()).StringValue());
			string className = document.GetField(SandoField.ClassName.ToString()).StringValue().ToSandoDisplayable();
			string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
			bool isConstructor = bool.Parse(document.GetField(SandoField.IsConstructor.ToString()).StringValue());
            return new object[] { name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, classId, className, modifiers, isConstructor };
		}
	}
}
