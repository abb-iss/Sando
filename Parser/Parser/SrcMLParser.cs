using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sando.Core;

namespace Sando.Parser
{
	public class SrcMLParser : ParserInterface
	{
		private SrcMLGenerator Generator;

		private static readonly XNamespace Namespace = "http://www.sdml.info/srcML/src";

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

		public ProgramElement[] Parse(String filename)
		{
			var programElements = new List<ProgramElement>();
			String srcml = Generator.GenerateSrcML(filename);

			//now Parse the important parts of the srcml and generate program elements
			XElement sourceElements = XElement.Parse(srcml);
			
			ParseFunctions(programElements, sourceElements);

			return programElements.ToArray();
		}

		private void ParseFunctions(List<ProgramElement> programElements, XElement elements)
		{
			IEnumerable<XElement> functions =
				from el in elements.Descendants(Namespace + "function")
				select el;
			programElements.AddRange(functions.Select(ParseFunction).ToList());
		}

		private ProgramElement ParseFunction(XElement function)
		{
			var method = new MethodElement();			
			//get name
			IEnumerable<XElement> name =
				from el in function.Elements(Namespace + "name")
				select el;
			method.Name = name.First().Value;
			//get other stuff...

			return method;
		}

	}
}
