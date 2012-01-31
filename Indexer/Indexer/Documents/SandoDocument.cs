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

		public Document GetDocument()
		{
			if(document == null)
			{
				if(programElement.Id == null || programElement.Id == Guid.Empty)
					throw new ArgumentException("Program element id cannot be null or an empty guid");
				//TODO - add back once parser is working better
				//if(String.IsNullOrWhiteSpace(programElement.FileName))
				//    throw new ArgumentException("Program element file name cannot be null");
				//if(String.IsNullOrWhiteSpace(programElement.FullFilePath))
				//    throw new ArgumentException("Program element full file path cannot be null");

				document = new Document();
				document.Add(new Field("Id", programElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
				document.Add(new Field("ProgramElementType", programElement.ProgramElementType.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
				document.Add(new Field("FileName", programElement.FileName, Field.Store.YES, Field.Index.NO));
				document.Add(new Field("FullFilePath", programElement.FullFilePath, Field.Store.YES, Field.Index.NO));
				document.Add(new Field("DefinitionLineNumber", programElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
				AddDocumentFields();
			}
			return document;
		}

		protected abstract void AddDocumentFields();

		protected ProgramElement programElement;
		protected Document document;
	}
}
