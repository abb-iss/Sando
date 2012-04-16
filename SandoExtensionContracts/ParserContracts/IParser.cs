using System.Collections.Generic;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExtensionContracts.ParserContracts
{
	public interface IParser
	{
		List<ProgramElement> Parse(string filename);
	}
}
