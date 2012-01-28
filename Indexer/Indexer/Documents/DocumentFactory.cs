using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core;

namespace Sando.Indexer.Documents
{
	public class DocumentFactory
	{
		public static SandoDocument Create(ProgramElement programElement)
		{
			//TODO - should we use contracts or assertions here instead?
			if(programElement == null)
			{
				return null;
			}

			//TODO - only processing methods for now
			var method = programElement as MethodElement;
			if(method != null)
			{
				return MethodDocument.Create(programElement as MethodElement);
			}

			//TODO - should this return null or throw an exception?
			return null;
		}
	}
}
