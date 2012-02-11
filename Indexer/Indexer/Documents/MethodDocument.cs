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
			document.Add(new Field("Name", methodElement.Name, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", methodElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("Arguments", methodElement.Arguments, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Body", methodElement.Body, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", methodElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("ReturnType", methodElement.ReturnType, Field.Store.NO, Field.Index.ANALYZED));
		}
	}
}
