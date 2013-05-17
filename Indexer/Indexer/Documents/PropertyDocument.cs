using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Collections.Generic;

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

        public override List<Field> GetFieldsForLucene()
        {
            List<Field> fields = new List<Field>(); 
			PropertyElement propertyElement = (PropertyElement) programElement;
            fields.Add(new Field(SandoField.AccessLevel.ToString(), propertyElement.AccessLevel.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.Body.ToString(), propertyElement.Body.ToSandoSearchable(), Field.Store.NO, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.DataType.ToString(), propertyElement.PropertyType.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.ClassId.ToString(), propertyElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            fields.Add(new Field(SandoField.ClassName.ToString(), propertyElement.ClassName.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            fields.Add(new Field(SandoField.Modifiers.ToString(), propertyElement.Modifiers, Field.Store.YES, Field.Index.ANALYZED));
            return fields;
		}

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, int definitionColumnNumber, string snippet, Document document)
		{
			AccessLevel accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), document.GetField(SandoField.AccessLevel.ToString()).StringValue(), true);
			string propertyType = document.GetField(SandoField.DataType.ToString()).StringValue().ToSandoDisplayable();
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue().ToSandoDisplayable();
			Guid classId = new Guid(document.GetField(SandoField.ClassId.ToString()).StringValue());
			string className = document.GetField(SandoField.ClassName.ToString()).StringValue().ToSandoDisplayable();
			string modifiers = document.GetField(SandoField.Modifiers.ToString()).StringValue();
            return new object[] { name, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet, accessLevel, propertyType, body, classId, className, modifiers };
		}
	}
}
