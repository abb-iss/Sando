using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class MethodDocument : SandoDocument
	{
		public MethodDocument(MethodElement methodElement)
			: base(methodElement)
		{	
		}

		protected override void AddDocumentFields()
		{
			MethodElement methodElement = (MethodElement)programElement;
			document.Add(new Field(SandoField.AccessLevel.ToString(), methodElement.AccessLevel.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.Arguments.ToString(), methodElement.Arguments, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.Body.ToString(), methodElement.Body, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field(SandoField.ClassId.ToString(), methodElement.ClassId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field(SandoField.ReturnType.ToString(), methodElement.ReturnType, Field.Store.YES, Field.Index.ANALYZED));
		}
	}
}
