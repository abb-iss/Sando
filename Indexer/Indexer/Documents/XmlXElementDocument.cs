using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
    public class XmlXElementDocument : SandoDocument
    {
        public XmlXElementDocument(ProgramElement programElement) : base(programElement)
        {
        }

        public XmlXElementDocument(Document document) : base(document)
        {
        }

        public override List<Field> GetFieldsForLucene()
        {
            var fields = new List<Field>();
            var element = (XmlXElement)programElement;
            AddField(fields, new Field(SandoField.Body.ToString(), element.Body.ToSandoSearchable(), Field.Store.NO,
                   Field.Index.ANALYZED));
            return fields;
        }

        public override object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath,
            int definitionLineNumber, int definitionColumnNumber, string snippet, Document document)
        {   
            return new object[] {name, String.Empty, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet};
        }
    }
}
