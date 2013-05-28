using System.Diagnostics.Contracts;
using Sando.ExtensionContracts.ProgramElementContracts;

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
				return new ClassDocument(programElement as ClassElement);
			}
			if(programElement as CommentElement != null)
			{
				return new CommentDocument(programElement as CommentElement);
			}
			if(programElement as EnumElement != null)
			{
				return new EnumDocument(programElement as EnumElement);
			}
			if(programElement as FieldElement != null)
			{
				return new FieldDocument(programElement as FieldElement);
			}
			if(programElement as MethodElement != null)
			{
				return new MethodDocument(programElement as MethodElement);
			}
			if(programElement as MethodPrototypeElement != null)
			{
				return new MethodPrototypeDocument(programElement as MethodPrototypeElement);
			}
			if(programElement as PropertyElement != null)
			{
				return new PropertyDocument(programElement as PropertyElement);
			}
            if (programElement as StructElement != null)
            {
                return new StructDocument(programElement as StructElement);
            }
			if(programElement as TextLineElement != null)
			{
				return new TextLineDocument(programElement as TextLineElement);
			}
            if (programElement as XmlXElement!= null)
            {
                return new XmlXElementDocument(programElement as XmlXElement);
            }

            if(programElement.GetCustomProperties().Count>0)
            {
                return new SandoDocument(programElement);
            }

			//if this code is reached, contract will fail
			return null;
		}
	}

  
}
