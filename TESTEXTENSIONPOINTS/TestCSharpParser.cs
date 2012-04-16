using System.Collections.Generic;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.TestExtensionPoints
{
	public class TestCSharpParser : IParser
	{
		public List<ProgramElement> Parse(string filename)
		{
			return new List<ProgramElement>() { new TestElement("TestCSharpName", 1, filename, "TestElement snippet") };
		}
	}
}
