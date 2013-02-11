using System;
using System.Diagnostics.Contracts;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents.Converters;

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
                document = ConverterFromProgramElementToDocument.Create(programElement,this).Convert();	
			}
			return document;
		}



	    public ProgramElement ReadProgramElementFromDocument()
		{
            return ConverterFromHitToDocument.Create(this, GetDocument()).Convert();
		}

        public virtual void AddDocumentFields(Document luceneDocument)
		{
		    //none
		}


        public virtual ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document luceneDocument)
		{
            var parameters = new object[] { name, definitionLineNumber, fullFilePath, snippet };
            var myElement = Activator.CreateInstance(GetMyType(luceneDocument), parameters);
            SetCustomFields(myElement, luceneDocument);
            return myElement as ProgramElement;
		}

	    protected void SetCustomFields(object myElement, Document luceneDocument)
	    {
	        foreach (var property in (myElement as ProgramElement).GetCustomProperties())
	        {
	            if (!property.Name.Equals(ProgramElement.CustomTypeTag))
	            {
                    Field field = luceneDocument.GetField(property.Name);
	                Contract.Assert(field != null, "Field " + property.Name + " was not populated");
	                property.SetValue(myElement, field.StringValue(), null);
	            }
	        }
	    }

	    protected Type GetMyType(Document luceneDocument)
        {
            try
            {
                string typeId = luceneDocument.GetField(ProgramElement.CustomTypeTag).StringValue();
                return Type.GetType(typeId);
            }
            catch
            {
                return typeof (ProgramElement);
            }
	        
        }

		internal ProgramElement programElement;
        private Document document;		

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


        internal ProgramElement ReadProgramElementFromDocument(Type type, object[] parameters, Document luceneDocument)
        {
            var element = Activator.CreateInstance(type, parameters) as ProgramElement;
            SetCustomFields(element,luceneDocument);
            return element;			
        }
    }
}
