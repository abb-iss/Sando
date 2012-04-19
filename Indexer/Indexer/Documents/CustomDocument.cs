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

        public CustomDocument(CustomProgramElement programElement, Type type)
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
            var customElement = programElement as CustomProgramElement;
            foreach (var property in customElement.GetCustomProperties())
            {
                document.Add(new Field(property.Name, property.GetValue(customElement,null) as String, Field.Store.YES, Field.Index.ANALYZED));    
            }            
        }

        protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
        {            
            var parameters = new object[] {name,  definitionLineNumber, fullFilePath, snippet};
            var myElement = Activator.CreateInstance(_myType,parameters);
            foreach (var property in (myElement as CustomProgramElement).GetCustomProperties())
            {
                if (!property.Name.Equals(CustomProgramElement.CustomTypeTag))
                {
                    Field field = document.GetField(property.Name);
                    Contract.Assert(field != null, "Field " + property.Name + " was not populated");
                    property.SetValue(myElement, field.StringValue(), null);
                }
            }
            return myElement as ProgramElement;
        }


    }
}
