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

			//TODO - only processing methods for now
			var method = programElement as MethodElement;
			if(method != null)
			{
				return MethodDocument.Create(programElement as MethodElement);
			}

			//if this code is reached, contract will fail
			return null;
		}
	}
}
