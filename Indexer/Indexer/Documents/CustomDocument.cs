using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
    public class CustomDocument : SandoDocument
    {
        private Type _myType;

        public CustomDocument(ProgramElement programElement) : base(programElement)
        {
        }

        public CustomDocument(Document document, Type type) : base(document)
        {            
            this._myType = type;
        }

        protected override void AddDocumentFields()
        {
            throw new NotImplementedException();
        }

        protected override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
        {            
            var parameters = new object[] {name,  definitionLineNumber, fullFilePath, snippet};
            var programElement = Activator.CreateInstance(_myType,parameters);
            foreach (var property in _myType.GetProperties())
            {
                if (property.DeclaringType != typeof(ProgramElement) && property.DeclaringType != typeof(CustomProgramElement))
                {
                    Field field = document.GetField(property.Name);
                    Contract.Assert(field != null, "Field " + property.Name + " was not populated");
                    property.SetValue(programElement, field.StringValue(), null);
                }
            }
            return programElement as ProgramElement;
        }
    }
}
