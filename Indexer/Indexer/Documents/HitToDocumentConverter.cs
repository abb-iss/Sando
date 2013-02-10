using Sando.ExtensionContracts.ProgramElementContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.Documents
{
    public class HitToDocumentConverter
    {
        private SandoDocument sandoDocument;
        private Lucene.Net.Documents.Document luceneDocument;

        public HitToDocumentConverter(SandoDocument sandoDocument, Lucene.Net.Documents.Document document)
        {
            this.sandoDocument = sandoDocument;
            this.luceneDocument = document;
        }

        public static HitToDocumentConverter Create(SandoDocument sandoDocument, Lucene.Net.Documents.Document document)
        {
            return new HitToDocumentConverter(sandoDocument, document);
        }

        internal ExtensionContracts.ProgramElementContracts.ProgramElement Convert()
        {
            //Guid id = new Guid(document.GetField(SandoField.Id.ToString()).StringValue());
            string name = luceneDocument.GetField(SandoField.Name.ToString()).StringValue().ToSandoDisplayable();
            ProgramElementType type = (ProgramElementType)Enum.Parse(typeof(ProgramElementType), luceneDocument.GetField(SandoField.ProgramElementType.ToString()).StringValue(), true);
            string fullFilePath = luceneDocument.GetField(SandoField.FullFilePath.ToString()).StringValue();
            int definitionLineNumber = int.Parse(luceneDocument.GetField(SandoField.DefinitionLineNumber.ToString()).StringValue());
            string snippet = luceneDocument.GetField(SandoField.Source.ToString()).StringValue();
            sandoDocument.programElement = sandoDocument.ReadProgramElementFromDocument(name, type, fullFilePath, definitionLineNumber, snippet, luceneDocument);
            return sandoDocument.programElement;
        }

    }
}
