using Lucene.Net.Documents;
using Sando.Core;
using System;

namespace Sando.Indexer.Documents
{
	public abstract class SandoDocument
	{
		protected SandoDocument(ProgramElement programElement)
		{
			this.programElement = programElement;
		}
		
		protected SandoDocument(Document document)
		{
			this.document = document;
		}

		public Document GetDocument()
		{
			if(document == null)
			{
				document = new Document();
				document.Add(new Field(SandoField.Id.ToString(), programElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
				document.Add(new Field(SandoField.Name.ToString(), programElement.Name, Field.Store.YES, Field.Index.ANALYZED));
				document.Add(new Field(SandoField.ProgramElementType.ToString(), programElement.ProgramElementType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
				document.Add(new Field(SandoField.FullFilePath.ToString(), StandardizeFilePath(programElement.FullFilePath), Field.Store.YES, Field.Index.NOT_ANALYZED));
				document.Add(new Field(SandoField.DefinitionLineNumber.ToString(), programElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
				document.Add(new Field(SandoField.Snippet.ToString(), programElement.Snippet, Field.Store.YES, Field.Index.NO));
				AddDocumentFields();
			}
			return document;
		}

		public ProgramElement ReadProgramElementFromDocument()
		{
			//Guid id = new Guid(document.GetField(SandoField.Id.ToString()).StringValue());
			string name = document.GetField(SandoField.Name.ToString()).StringValue();
			ProgramElementType type = (ProgramElementType)Enum.Parse(typeof(ProgramElementType), document.GetField(SandoField.ProgramElementType.ToString()).StringValue());
			string fullFilePath = document.GetField(SandoField.FullFilePath.ToString()).StringValue();
			int definitionLineNumber = int.Parse(document.GetField(SandoField.DefinitionLineNumber.ToString()).StringValue());
			string snippet = document.GetField(SandoField.Snippet.ToString()).StringValue();
			programElement = ReadProgramElementFromDocument(name, type, fullFilePath, definitionLineNumber, snippet, document);
			return programElement;
		}

		protected abstract void AddDocumentFields();
		protected abstract ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document);

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

	}
}
