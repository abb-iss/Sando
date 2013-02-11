﻿using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class FieldDocument : SandoDocument
	{
		public FieldDocument(FieldElement fieldElement)
			: base(fieldElement)
		{	
		}

		public FieldDocument(Document document)
			: base(document)
		{
		}

		public override void AddDocumentFields(Document luceneDocument)
		{
			FieldElement fieldElement = (FieldElement) programElement;
            luceneDocument.Add(new Field(SandoField.AccessLevel.ToString(), fieldElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            luceneDocument.Add(new Field(SandoField.DataType.ToString(), fieldElement.FieldType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            luceneDocument.Add(new Field(SandoField.ClassId.ToString(), fieldElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            luceneDocument.Add(new Field(SandoField.ClassName.ToString(), fieldElement.ClassName.ToSandoSearchable(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            luceneDocument.Add(new Field(SandoField.Modifiers.ToString(), fieldElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
            luceneDocument.Add(new Field(SandoField.InitialValue.ToString(), fieldElement.InitialValue.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
		}

        public override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string fieldType = document.GetField(SandoField.DataType.ToString()).StringValue().ToSandoDisplayable();
			Guid classId = new Guid(document.GetField(SandoField.ClassId.ToString()).StringValue());
			string className = document.GetField(SandoField.ClassName.ToString()).StringValue().ToSandoDisplayable();
			string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
			string initialValue = document.GetField(SandoField.InitialValue.ToString()).StringValue().ToSandoDisplayable();
            return base.ReadProgramElementFromDocument(GetMyType(document), new object[] { name, definitionLineNumber, fullFilePath, snippet, accessLevel, fieldType, classId, className, modifiers, initialValue },document);
		}
	}
}
