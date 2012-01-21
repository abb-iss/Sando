using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sando.Core;

namespace Sando.Parser
{
	public class SrcMLParser : ParserInterface
	{
		private SrcMLGenerator Generator;

		public SrcMLParser()
		{
			//try to set this up automatically
			var currentDirectory = Environment.CurrentDirectory;
			Generator = new SrcMLGenerator();
			Generator.SetSrcMLLocation(currentDirectory + "\\..\\..\\..\\..\\LIBS\\srcML-Win");			
		}

		public SrcMLParser(SrcMLGenerator gen )
		{
			Generator = gen;
		}

		public ProgramElement[] parse(String filename)
		{
			String srcml = Generator.GenerateSrcML(filename);

			//now parse the important parts of the srcml and generate program elements

			return null;
		}

	}
}
