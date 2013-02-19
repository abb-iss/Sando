using System;
using System.Diagnostics.Contracts;
using Lucene.Net.Documents;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents.Converters;
using System.Collections.Generic;

namespace Sando.Indexer.Documents
{
	public class SandoDocument
	{
		public SandoDocument(ProgramElement programElement)
		{
			this.programElement = programElement;
		}
		
		public SandoDocument(Document document)
		{
			this.document = document;
		}

		public Document GetDocument()
		{
			if(document == null)
			{
                document = ConverterFromProgramElementToDocument.Create(programElement,this).Convert();	
			}
			return document;
		}


        public virtual List<Field> GetFieldsForLucene()
		{
            return new List<Field>();
		}

        public virtual object[] GetParametersForConstructor(string name, ProgramElementType programElementType, string fullFilePath, int definitionLineNumber, string snippet, Document document)
        {
            return new object[]{name,definitionLineNumber,fullFilePath,snippet};
        }

		internal ProgramElement programElement;
        private Document document;		

  
    }
}
