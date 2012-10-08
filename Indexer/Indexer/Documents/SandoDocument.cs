using System;
using System.Diagnostics.Contracts;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class SandoDocument
	{
		public SandoDocument(ProgramElement programElement)
		{
			this.programElement = programElement;
		}
		
		public SandoDocument(Document document)
		{
			this.document = document;
		}

		public Document GetDocument()
		{
			if(document == null)
			{
				document = new Document();
				document.Add(new Field(SandoField.Id.ToString(), programElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
				document.Add(new Field(SandoField.Name.ToString(), programElement.Name.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
				document.Add(new Field(SandoField.ProgramElementType.ToString(), programElement.ProgramElementType.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
				document.Add(new Field(SandoField.FullFilePath.ToString(), StandardizeFilePath(programElement.FullFilePath), Field.Store.YES, Field.Index.NOT_ANALYZED));
				document.Add(new Field(SandoField.DefinitionLineNumber.ToString(), programElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
				document.Add(new Field(SandoField.Snippet.ToString(), programElement.Snippet, Field.Store.YES, Field.Index.NO));
                document.Add(new Field(ProgramElement.CustomTypeTag, programElement.GetType().AssemblyQualifiedName, Field.Store.YES, Field.Index.NO));
				document.Add(new Field(ProgramElement.ParentExperimentFlowTag, programElement.ParentExperimentFlow.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
				AddDocumentFields();
			    AddCustomFields();
			}
			return document;
		}

	    private void AddCustomFields()
	    {
	        var customProperties = programElement.GetCustomProperties();
	        foreach (var customProperty in customProperties)
	        {
                document.Add(new Field(customProperty.Name, customProperty.GetValue(programElement,null) as string, Field.Store.YES, Field.Index.ANALYZED));
	        }
	    }

	    public ProgramElement ReadProgramElementFromDocument()
		{

			//Guid id = new Guid(document.GetField(SandoField.Id.ToString()).StringValue());
			string name = document.GetField(SandoField.Name.ToString()).StringValue().ToSandoDisplayable();
			ProgramElementType type = (ProgramElementType)Enum.Parse(typeof(ProgramElementType), document.GetField(SandoField.ProgramElementType.ToString()).StringValue(), true);
			string fullFilePath = document.GetField(SandoField.FullFilePath.ToString()).StringValue();
			int definitionLineNumber = int.Parse(document.GetField(SandoField.DefinitionLineNumber.ToString()).StringValue());
			string snippet = document.GetField(SandoField.Snippet.ToString()).StringValue();		    
			programElement = ReadProgramElementFromDocument(name, type, fullFilePath, definitionLineNumber, snippet, document);
			return programElement;
		}

		protected virtual void AddDocumentFields()
		{
		    //none
		}


		protected virtual ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
            var parameters = new object[] { name, definitionLineNumber, fullFilePath, snippet };
            var myElement = Activator.CreateInstance(GetMyType(), parameters);
            SetCustomFields(myElement);
            return myElement as ProgramElement;
		}

	    protected void SetCustomFields(object myElement)
	    {
	        foreach (var property in (myElement as ProgramElement).GetCustomProperties())
	        {
	            if (!property.Name.Equals(ProgramElement.CustomTypeTag))
	            {
	                Field field = document.GetField(property.Name);
	                Contract.Assert(field != null, "Field " + property.Name + " was not populated");
	                property.SetValue(myElement, field.StringValue(), null);
	            }
	        }
	    }

	    protected Type GetMyType()
        {
            try
            {
                string typeId = document.GetField(ProgramElement.CustomTypeTag).StringValue();
                return Type.GetType(typeId);
            }
            catch
            {
                return typeof (ProgramElement);
            }
	        
        }

		protected ProgramElement programElement;
		protected Document document;

        internal static string StandardizeFilePath(string fullFilePath)
        {
            if (fullFilePath.Contains("/"))
            {
                string old = "/";
                string rep = "\\";
                var path = fullFilePath.Replace(old, rep);
                return path;
            }
            return fullFilePath;
        }


        internal ProgramElement ReadProgramElementFromDocument(Type type, object[] parameters)
        {
            var element = Activator.CreateInstance(type, parameters) as ProgramElement;
            SetCustomFields(element);
            return element;			
        }
    }
}
