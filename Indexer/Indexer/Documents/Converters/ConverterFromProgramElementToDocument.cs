using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.Documents.Converters
{
    public class ConverterFromProgramElementToDocument
    {
        private ProgramElement programElement;
        private SandoDocument sandoDocument;

        public ConverterFromProgramElementToDocument(ProgramElement programElement, SandoDocument sandoDocument)
        {
            this.programElement = programElement;
            this.sandoDocument = sandoDocument;
        }

        internal static ConverterFromProgramElementToDocument Create(ProgramElement programElement, SandoDocument sandoDocument)
        {
            return new ConverterFromProgramElementToDocument(programElement,sandoDocument);
        }

        internal Lucene.Net.Documents.Document Convert()
        {
            var document = new Document();
            document.Add(new Field(SandoField.Id.ToString(), programElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Name.ToString(), programElement.Name.ToSandoSearchable(), Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(SandoField.ProgramElementType.ToString(), programElement.ProgramElementType.ToString().ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.FullFilePath.ToString(), SandoDocument.StandardizeFilePath(programElement.FullFilePath), Field.Store.YES, Field.Index.NOT_ANALYZED));
            document.Add(new Field(SandoField.FileExtension.ToString(), programElement.FileExtension, Field.Store.NO, Field.Index.ANALYZED));
            document.Add(new Field(SandoField.DefinitionLineNumber.ToString(), programElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
            document.Add(new Field(SandoField.Source.ToString(), programElement.RawSource, Field.Store.YES, Field.Index.ANALYZED));
            document.Add(new Field(ProgramElement.CustomTypeTag, programElement.GetType().AssemblyQualifiedName, Field.Store.YES, Field.Index.NO));            
            sandoDocument.AddDocumentFields(document);
            AddCustomFields(document);
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
    }
}
