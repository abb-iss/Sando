using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
    public class CustomDocument : SandoDocument
    {
        private Type _myType;
        private static String CustomPrefix = "CustomField++";

        public CustomDocument(ProgramElement programElement, Type type)
            : base(programElement)
        {
            this._myType = type;
        }

        public CustomDocument(Document document, Type type) : base(document)
        {            
            this._myType = type;
        }

        protected override void AddDocumentFields()
        {
            var customElement = programElement;
            foreach (var property in GetCustomProperties())
            {
                document.Add(new Field(CustomPrefix + property.Name, property.GetValue(customElement,null) as String, Field.Store.YES, Field.Index.ANALYZED));    
            }            
        }

        protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
        {            
            var parameters = new object[] {name,  definitionLineNumber, fullFilePath, snippet};
            var programElement = Activator.CreateInstance(_myType,parameters);
            foreach (var property in GetCustomProperties())
            {
                    Field field = document.GetField(property.Name);
                    Contract.Assert(field != null, "Field " + property.Name + " was not populated");
                    property.SetValue(programElement, field.StringValue(), null);                
            }
            return programElement as ProgramElement;
        }

        private IEnumerable<PropertyInfo> GetCustomProperties()
        {
            var propertyInfos = new List<PropertyInfo>();
            foreach (var property in _myType.GetProperties())
            {
                if (property.DeclaringType != typeof(ProgramElement) && property.DeclaringType != typeof(CustomProgramElement))
                {
                    propertyInfos.Add(property);
                }
            }
            return propertyInfos;
        }
    }
}
