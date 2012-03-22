using System;
using System.Diagnostics.Contracts;
using Lucene.Net.Documents;
using Sando.Core;
using Sando.Indexer.Documents;

namespace Sando.Indexer.Searching
{
	public static class ProgramElementReader
	{
		public static ProgramElement ReadProgramElementFromDocument(Document document)
		{
			Contract.Requires(document != null, "ProgramElementReader:ReadProgramElementFromDocument - document cannot be null!");
			Contract.Ensures(Contract.Result<ProgramElement>() != null, "ProgramElementReader:ReadProgramElementFromDocument - an object must be returned from this method!");

			ProgramElementType programElementType = (ProgramElementType)Enum.Parse(typeof(ProgramElementType), document.GetField(SandoField.ProgramElementType.ToString()).StringValue());
			switch(programElementType)
			{
				case ProgramElementType.Class:
					return new ClassDocument(document).ReadProgramElementFromDocument();
				case ProgramElementType.Comment:
					return new CommentDocument(document).ReadProgramElementFromDocument();
				case ProgramElementType.DocComment:
					return new DocCommentDocument(document).ReadProgramElementFromDocument();
				case ProgramElementType.Enum:
					return new EnumDocument(document).ReadProgramElementFromDocument();
				case ProgramElementType.Field:
					return new FieldDocument(document).ReadProgramElementFromDocument();
				case ProgramElementType.Method:
					return new MethodDocument(document).ReadProgramElementFromDocument();
				case ProgramElementType.Property:
					return new PropertyDocument(document).ReadProgramElementFromDocument();
				default:
					return null;
			}
		}
	}
}
