using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class MethodDocument : SandoDocument
	{
		public static MethodDocument Create(MethodElement methodElement)
		{
			if(methodElement == null)
				throw new ArgumentException("MethodElement cannot be null");
			if(methodElement.ClassId == null)
				throw new ArgumentException("Class id cannot be null");
			if(methodElement.Id == null)
				throw new ArgumentException("Method id cannot be null");
			if(String.IsNullOrWhiteSpace(methodElement.Name))
				throw new ArgumentException("Method name cannot be null");
			if(String.IsNullOrWhiteSpace(methodElement.ReturnType))
				throw new ArgumentException("Method return type cannot be null");

			return new MethodDocument(methodElement);
		}

		protected override void AddDocumentFields()
		{
			MethodElement methodElement = (MethodElement)programElement;
			document.Add(new Field("Name", methodElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", methodElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DefinitionLineNumber", methodElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Arguments", methodElement.Arguments ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Body", methodElement.Body ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("ClassId", methodElement.ClassId.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("ReturnType", methodElement.ReturnType, Field.Store.NO, Field.Index.ANALYZED));
		}

		private MethodDocument(MethodElement methodElement)
			: base(methodElement)
		{	
		}
	}
}
