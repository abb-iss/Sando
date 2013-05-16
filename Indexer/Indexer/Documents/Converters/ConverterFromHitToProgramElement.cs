using Sando.ExtensionContracts.ProgramElementContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using System.Diagnostics.Contracts;

namespace Sando.Indexer.Documents.Converters
{
    public class ConverterFromHitToProgramElement
    {
        private SandoDocument sandoDocument;
        private Lucene.Net.Documents.Document luceneDocument;

        private ConverterFromHitToProgramElement()
        {
            throw new NotImplementedException();//don't call this, use Create
        }

        private ConverterFromHitToProgramElement(SandoDocument sandoDocument, Lucene.Net.Documents.Document document)
        {
            this.sandoDocument = sandoDocument;
            this.luceneDocument = document;
        }

        public static ConverterFromHitToProgramElement Create(Lucene.Net.Documents.Document document)
        {
            Contract.Requires(document != null, "ConverterFromHitToProgramElement:ReadProgramElementFromDocument - document cannot be null!");            
            return new ConverterFromHitToProgramElement(GetSandoDocument(document), document);
        }

        private static SandoDocument GetSandoDocument(Document document)
        {
            ProgramElementType programElementType = (ProgramElementType)Enum.Parse(typeof(ProgramElementType), document.GetField(SandoField.ProgramElementType.ToString()).StringValue(), true);
            switch (programElementType)
            {
                case ProgramElementType.Class:
                    return new ClassDocument(document);
                case ProgramElementType.Comment:
                    return new CommentDocument(document);
                case ProgramElementType.Enum:
                    return new EnumDocument(document);
                case ProgramElementType.Field:
                    return new FieldDocument(document);
                case ProgramElementType.Method:
                    return new MethodDocument(document);
                case ProgramElementType.Property:
                    return new PropertyDocument(document);
                case ProgramElementType.MethodPrototype:
                    return new MethodPrototypeDocument(document);
                case ProgramElementType.Struct:
                    return new StructDocument(document);
                case ProgramElementType.TextLine:
                    return new TextLineDocument(document);
                case ProgramElementType.Custom:
                    var type = GetMyType(document);
                    if(type.BaseType.Equals(typeof(MethodElement)))
                        return new MethodDocument(document);                    
                    return new SandoDocument(document);
                default:
                    return null;
            }
        }

        public ProgramElement Convert()
        {
            Contract.Ensures(Contract.Result<ProgramElement>() != null, "ConverterFromHitToProgramElement:ReadProgramElementFromDocument - an object must be returned from this method!");			

            //Get standard field values
            string name = luceneDocument.GetField(SandoField.Name.ToString()).StringValue().ToSandoDisplayable();
            ProgramElementType type = (ProgramElementType)Enum.Parse(typeof(ProgramElementType), luceneDocument.GetField(SandoField.ProgramElementType.ToString()).StringValue(), true);
            string fullFilePath = luceneDocument.GetField(SandoField.FullFilePath.ToString()).StringValue();
            int definitionLineNumber = int.Parse(luceneDocument.GetField(SandoField.DefinitionLineNumber.ToString()).StringValue());
            int definitionColumnNumber = int.Parse(luceneDocument.GetField(SandoField.DefinitionColumnNumber.ToString()).StringValue());
            string snippet = luceneDocument.GetField(SandoField.Source.ToString()).StringValue();
            
            //Add values that vary according to element type (e.g., a textline doesn't have a parent class whereas a method often does)
            //Note: Parameters must match the parameter list for corresponding program element.  See MethodDocument.GetParametersForConstructor and the MethodElement constructor
            var parameters = sandoDocument.GetParametersForConstructor(name, type, fullFilePath, definitionLineNumber, definitionColumnNumber, snippet, luceneDocument);
            
            //Create type from the collected values
            var myClassType = GetMyType(luceneDocument);
            var myElement = Activator.CreateInstance(myClassType, parameters);

            //Populate any custom fields from user-defined types, only happens when a third party extends Sando
            SetCustomFields(myElement, luceneDocument);
            return myElement as ProgramElement;
        }


        internal static void SetCustomFields(object myElement, Lucene.Net.Documents.Document luceneDocument)
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

        internal static Type GetMyType(Document luceneDocument)
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
   
    }
}
