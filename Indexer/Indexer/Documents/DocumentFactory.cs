using System.Diagnostics.Contracts;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class DocumentFactory
	{
		public static SandoDocument Create(ProgramElement programElement)
		{
			Contract.Requires(programElement != null, "DocumentFactory:Create - programElement cannot be null!");
			Contract.Ensures(Contract.Result<SandoDocument>() != null, "DocumentFactory:Create - an object must be returned from this method!");

			if(programElement as ClassElement != null)
			{
				return ClassDocument.Create(programElement as ClassElement);
			}
			if(programElement as CommentElement != null)
			{
				return CommentDocument.Create(programElement as CommentElement);
			}
			if(programElement as DocCommentElement != null)
			{
				return DocCommentDocument.Create(programElement as DocCommentElement);
			}
			if(programElement as EnumElement != null)
			{
				return EnumDocument.Create(programElement as EnumElement);
			}
			if(programElement as FieldElement != null)
			{
				return FieldDocument.Create(programElement as FieldElement);
			}
			if(programElement as MethodElement != null)
			{
				return MethodDocument.Create(programElement as MethodElement);
			}
			if(programElement as PropertyElement != null)
			{
				return PropertyDocument.Create(programElement as PropertyElement);
			}

			//if this code is reached, contract will fail
			return null;
		}
	}
}
