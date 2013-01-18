// Code changed by JZ: solution monitor integration
using System.Xml;
using System.Xml.Linq;
// End of code changes
using System.Collections.Generic;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExtensionContracts.ParserContracts
{
	public interface IParser
	{
		List<ProgramElement> Parse(string filename);

        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// New Parse method that takes both source file path and the XElement representation of the source file as input arguments.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sourceElements"></param>
        /// <returns></returns>
        List<ProgramElement> Parse(string filename, XElement sourceElements);
        // End of code changes
	}
}
