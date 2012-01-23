using System;
using Lucene.Net.Documents;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class EnumDocument : SandoDocument
	{
		public static EnumDocument Create(EnumElement enumElement)
		{
			if(enumElement == null)
				throw new ArgumentException("EnumElement cannot be null");
			if(String.IsNullOrWhiteSpace(enumElement.FileName))
				throw new ArgumentException("File name cannot be null");
			if(String.IsNullOrWhiteSpace(enumElement.FullFilePath))
				throw new ArgumentException("Full file path name cannot be null");
			if(enumElement.Id == null)
				throw new ArgumentException("Enum id cannot be null");
			if(String.IsNullOrWhiteSpace(enumElement.Name))
				throw new ArgumentException("Enum name cannot be null");

			return new EnumDocument(enumElement);
		}

		protected override void CreateDocument()
		{
			document = new Document();
			EnumElement enumElement = (EnumElement) programElement;
			document.Add(new Field("Id", enumElement.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Name", enumElement.Name, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("Namespace", enumElement.Namespace ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("AccessLevel", enumElement.AccessLevel.ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
			document.Add(new Field("DefinitionLineNumber", enumElement.DefinitionLineNumber.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("Values", enumElement.Values ?? String.Empty, Field.Store.NO, Field.Index.ANALYZED));
			document.Add(new Field("FileName", enumElement.FileName, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("FullFilePath", enumElement.FullFilePath, Field.Store.YES, Field.Index.NO));
		}

		private EnumDocument(EnumElement enumElement)
			: base(enumElement)
		{	
		}
	}
}
