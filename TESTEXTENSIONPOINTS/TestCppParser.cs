using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.TestExtensionPoints
{
	public class TestCppParser : IParser
	{
		public ProgramElement[] Parse(string filename)
		{
			return new ProgramElement[1] { new TestElement("TestCppName", 1, filename, "TestElement snippet") };
		}
	}
}
