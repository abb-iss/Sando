using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class PropertyDocument : SandoDocument
	{
		public PropertyDocument(PropertyElement propertyElement)
			: base(propertyElement)
		{	
		}

		public PropertyDocument(Document document)
			: base(document)
		{
		}

		protected override void AddDocumentFields()
		{
			PropertyElement propertyElement = (PropertyElement) programElement;
			document.Add(new Field(SandoField.AccessLevel.ToString(), propertyElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.Body.ToString(), propertyElement.Body, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.DataType.ToString(), propertyElement.PropertyType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ClassId.ToString(), propertyElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.ClassName.ToString(), propertyElement.ClassName, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.Modifiers.ToString(), propertyElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
		}

		protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string propertyType = document.GetField(SandoField.DataType.ToString()).StringValue().ToSandoDisplayable();
			string body = document.GetField(SandoField.Body.ToString()).StringValue();
			Guid classId = new Guid(document.GetField(SandoField.ClassId.ToString()).StringValue());
			string className = document.GetField(SandoField.ClassName.ToString()).StringValue();
			string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
			return new PropertyElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, propertyType, body, classId, className, modifiers);
		}
	}
}
