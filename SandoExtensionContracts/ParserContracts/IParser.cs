using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExtensionContracts.ParserContracts
{
	public interface IParser
	{
		ProgramElement[] Parse(string filename);
	}
}
