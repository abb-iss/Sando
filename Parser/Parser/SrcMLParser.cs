using System;
using Sando.Core;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

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
			XDocument xDoc = XDocument.Parse(srcml);

			var a = from namespc in xDoc.Descendants("decl")
					where namespc.Element("type").Value == "namespace"
					select namespc.Element("block");

			String nmspace = "";
			ProgramElement[] classes = parseClass(xDoc, filename, nmspace);

			return classes;
		}

		private ProgramElement[] parseClass(XDocument xDoc, String filename, String nmspace) {
			
			List<ClassElement> classElements = 
				(from cls in xDoc.Descendants("class")
				 select new ClassElement {
					Namespace = nmspace,
					FileName = filename,
					Name = cls.Element("name").Value,
					DefinitionLineNumber = Int32.Parse(cls.Attribute("pos::line").Value),
					AccessLevel = strToAccessLevel(cls.Element("specifier").Value)
				 }).ToList<ClassElement>();

			var Elements = from cls in xDoc.Descendants()
						   select cls;

			//foreach(String name in classNames)
			//{
			//    ClassElement classElement = new ClassElement();
			//    classElement.Name = name;
			//    classElement.FileName = filename;

			//}

			return classElements.ToArray();
		}

		private AccessLevel strToAccessLevel(String level) 
		{
			switch(level.ToLower())
			{
				case "private":
					return AccessLevel.Private;
				case "public":
					return AccessLevel.Public;
				case "protected":
					return AccessLevel.Protected;
				default:
					return AccessLevel.Protected;			
			}
		}

	}
}
