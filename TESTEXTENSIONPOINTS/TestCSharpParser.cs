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

        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// New Parse method that takes two arguments, due to modification of IParser
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceElements"></param>
        /// <returns></returns>
        public List<ProgramElement> Parse(string fileName, System.Xml.Linq.XElement sourceElements)
        {
            return Parse(fileName);
        }
        // End of code changes
	}
}
