using System;
using System.Diagnostics.Contracts;
using System.IO;
using Lucene.Net.Documents;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;
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
				case ProgramElementType.MethodPrototype:
					return new MethodPrototypeDocument(document).ReadProgramElementFromDocument();
                case ProgramElementType.Struct:
                    return new StructDocument(document).ReadProgramElementFromDocument();
                case ProgramElementType.Custom:
                    return new CustomDocument(document, GetTypeForCustom(document)).ReadProgramElementFromDocument();
				default:
					return null;
			}
		}

	    private static Type GetTypeForCustom(Document document)
	    {
            try
            {
                return Type.GetType(document.GetField(CustomProgramElement.CustomTypeTag).StringValue());

            }catch(Exception e)
            {
                return typeof (CustomProgramElement);
            }
	    }
	}
}
