﻿using System;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Documents
{
	public class TextLineDocument : SandoDocument
	{
		public TextLineDocument(TextLineElement classElement)
			: base(classElement)
		{
		}

		public TextLineDocument(Document document)
			: base(document)
		{
		}

		public override void AddDocumentFields(Document luceneDocument)
		{
			var textLineElement = (TextLineElement) programElement;
            luceneDocument.Add(new Field(SandoField.Body.ToString(), textLineElement.Body, Field.Store.NO, Field.Index.ANALYZED));
		}

        public override ProgramElement ReadProgramElementFromDocument(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
		{
            string body = "not stored in index";//document.GetField(SandoField.Body.ToString()).StringValue();
			return new TextLineElement(name, definitionLineNumber, fullFilePath, snippet, body);
		}
	}
}
