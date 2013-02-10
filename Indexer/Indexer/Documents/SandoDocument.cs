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
                document = ProgramElementToDocumentConverter.Create(programElement,this).Convert();	
			}
			return document;
		}

	    public void AddCustomFields(Document luceneDocument)
	    {
	        var customProperties = programElement.GetCustomProperties();
	        foreach (var customProperty in customProperties)
	        {
                luceneDocument.Add(new Field(customProperty.Name, customProperty.GetValue(programElement, null) as string, Field.Store.YES, Field.Index.ANALYZED));
	        }
	    }

	    public ProgramElement ReadProgramElementFromDocument()
		{
            return HitToDocumentConverter.Create(this, document).Convert();
		}

        public virtual void AddDocumentFields(Document luceneDocument)
		{
		    //none
		}


		public virtual ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
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

		internal ProgramElement programElement;
		protected Document document;

        public static string StandardizeFilePath(string fullFilePath)
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
