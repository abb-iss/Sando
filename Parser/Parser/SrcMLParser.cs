using System;
using Sando.Core;

namespace Sando.Parser
{
	class SrcMLParser : ParserInterface
	{
		public ProgramElement[] parse(String filename)
		{
			var generator = new SrcMLGenerator();
			generator.SetSrcMLLocation("'");
			String srcml = generator.GenerateSrcML(filename);

			//now parse the important parts of the srcml and generate program elements

			return null;
		}

	}
}
