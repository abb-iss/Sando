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
		private readonly SrcMLGenerator Generator;

		private static readonly XNamespace SourceNamespace = "http://www.sdml.info/srcML/src";
		private static readonly XNamespace PositionNamespace = "http://www.sdml.info/srcML/position";

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
				from el in elements.Descendants(SourceNamespace + "function")
				select el;
			programElements.AddRange(functions.Select(ParseFunction).ToList());
		}

		private ProgramElement ParseFunction(XElement function)
		{
			var method = new MethodElement();			
			//get name
			XElement name = function.Element(SourceNamespace + "name");
			method.Name = name.Value;
			//get other stuff...
			method.DefinitionLineNumber = Int32.Parse(name.Attribute(PositionNamespace + "line").Value);
			XElement access = function.Element(SourceNamespace + "type").Element(SourceNamespace + "specifier");
			method.AccessLevel = StrToAccessLevel(access.Value);
			XElement type = function.Element(SourceNamespace + "type").Element(SourceNamespace + "name");
			method.ReturnType = type.Value;
			//still debating how to handle method parameters and bodies...
			return method;
		}

		private AccessLevel StrToAccessLevel(String level)
		{
			return (AccessLevel)Enum.Parse(typeof (AccessLevel), level);		
 		}
	}
}
