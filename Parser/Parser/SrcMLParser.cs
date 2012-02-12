using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sando.Core;

namespace Sando.Parser
{
	public class SrcMLParser : ParserInterface
	{
		private readonly SrcMLGenerator Generator;

		private static readonly XNamespace SourceNamespace = "http://www.sdml.info/srcML/src";
		private static readonly XNamespace PositionNamespace = "http://www.sdml.info/srcML/position";
		private static readonly int SnippetSize = 5;

		public SrcMLParser()
		{
			//try to set this up automatically
			var currentDirectory = Environment.CurrentDirectory;
			Generator = new SrcMLGenerator();
			Generator.SetSrcMLLocation(currentDirectory + "\\..\\..\\..\\..\\LIBS\\srcML-Win");			
		}

		public SrcMLParser(SrcMLGenerator gen)
		{
			Generator = gen;
		}

		public ProgramElement[] Parse(string fileName)
		{
			var programElements = new List<ProgramElement>();
			String srcml = Generator.GenerateSrcML(fileName);

			//now Parse the important parts of the srcml and generate program elements
			XElement sourceElements = XElement.Parse(srcml);

			ParseEnums(programElements, sourceElements, fileName);
			ParseClasses(programElements, sourceElements, fileName);
			ParseFunctions(programElements, sourceElements, fileName);
			ParseProperties(programElements, sourceElements, fileName);

			return programElements.ToArray();
		}

		private void ParseProperties(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> props =
				from el in elements.Descendants(SourceNamespace + "decl")
				where el.Element(SourceNamespace + "name") != null &&
					  el.Element(SourceNamespace + "type") != null &&
					  el.Element(SourceNamespace + "block") != null &&
					  el.Elements().Count() == 3
				select el;

			foreach(XElement prop in props)
			{
				//parse name, and definition line number
				XElement nameElement = prop.Element(SourceNamespace + "name");
				string name = nameElement.Value;
				int definitionLineNumber = Int32.Parse(nameElement.Attribute(PositionNamespace + "line").Value);

				//get the class guid
				Guid classId = RetrieveClassGuid(prop, programElements);

				//parse access level and type
				XElement accessElement = prop.Element(SourceNamespace + "type").Element(SourceNamespace + "specifier");
				AccessLevel accessLevel = accessElement != null ? StrToAccessLevel(accessElement.Value) : AccessLevel.Internal;
				
				IEnumerable<XElement> types = prop.Element(SourceNamespace + "type").Elements(SourceNamespace + "name");
				string propertyType = String.Empty;
				foreach(XElement type in types)
				{
					propertyType += type.Value + " ";
				}
				propertyType = propertyType.TrimEnd();

				//parse property body
				XElement block = prop.Element(SourceNamespace + "block");
				IEnumerable<XElement> bodyNames =
					from el in block.Descendants(SourceNamespace + "name")
					select el;
				string body = String.Empty;
				foreach(XElement bodyname in bodyNames)
				{
					body += bodyname.Value + " ";
				}
				body = body.TrimEnd();
				
				string fullFilePath = System.IO.Path.GetFullPath(fileName);
				string snippet = RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

				programElements.Add(new PropertyElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, propertyType, body, classId));
			}
		}

		private void ParseEnums(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> enums =
				from el in elements.Descendants(SourceNamespace + "enum")
				select el;
			
			foreach(XElement enm in enums)
			{
				//SrcML doesn't parse access level specifiers for enums, so just pretend they are all public for now
				AccessLevel accessLevel = AccessLevel.Public;

				//parse name, and definition line number
				XElement nameElement = enm.Element(SourceNamespace + "name");
				string name = nameElement.Value;
				int definitionLineNumber = Int32.Parse(nameElement.Attribute(PositionNamespace + "line").Value);

				//parse namespace
				IEnumerable<XElement> ownerNamespaces =
					from el in enm.Ancestors(SourceNamespace + "decl")
					where el.Element(SourceNamespace + "type").Element(SourceNamespace + "name").Value == "namespace"
					select el;
				string namespaceName = String.Empty;
				foreach(XElement ownerNamespace in ownerNamespaces)
				{
					foreach(XElement spc in ownerNamespace.Elements(SourceNamespace + "name"))
					{
						namespaceName += spc.Value + " ";
					}
				}
				namespaceName = namespaceName.TrimEnd();

				//parse values
				XElement block = enm.Element(SourceNamespace + "block");
				string values = String.Empty;
				if(block != null)
				{
					IEnumerable<XElement> exprs =
						from el in block.Descendants(SourceNamespace + "expr")
						select el;
					foreach(XElement expr in exprs)
					{
						values += expr.Element(SourceNamespace + "name").Value + " ";
					}
					values = values.TrimEnd();
				}
				
				string fullFilePath = System.IO.Path.GetFullPath(fileName);
				string snippet = RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

				programElements.Add(new EnumElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, values));
			}
		}

		private void ParseClasses(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> classes =
				from el in elements.Descendants(SourceNamespace + "class") 
				select el;
			foreach(XElement cls in classes)
			{
				programElements.Add(ParseClass(cls, fileName));
			}
		}

		private ClassElement ParseClass(XElement cls, string fileName)
		{
			//parse stuff in class definition
			XElement nameElement = cls.Element(SourceNamespace + "name");
			string name = nameElement.Value;
			int definitionLineNumber = Int32.Parse(nameElement.Attribute(PositionNamespace + "line").Value);
			
			XElement accessElement = cls.Element(SourceNamespace + "specifier");
			AccessLevel accessLevel = StrToAccessLevel(accessElement.Value);

			//parse namespace
			IEnumerable<XElement> ownerNamespaces =
				from el in cls.Ancestors(SourceNamespace + "decl")
				where el.Element(SourceNamespace + "type").Element(SourceNamespace + "name").Value == "namespace"
				select el;
			string namespaceName = String.Empty;
			foreach(XElement ownerNamespace in ownerNamespaces)
			{
				foreach(XElement spc in ownerNamespace.Elements(SourceNamespace + "name"))
				{
					namespaceName += spc.Value + " ";
				}
			}
			namespaceName = namespaceName.TrimEnd();

			//parse extended classes and implemented interfaces (interfaces are treated as extended classes in SrcML for now)
			string extendedClasses = String.Empty;
			XElement super = cls.Element(SourceNamespace + "super");
			if(super != null)
			{
				XElement implements = super.Element(SourceNamespace + "implements");
				if(implements != null)
				{
					IEnumerable<XElement> impNames =
						from el in implements.Descendants(SourceNamespace + "name")
						select el;
					foreach(XElement impName in impNames)
					{
						extendedClasses += impName.Value + " ";
					}
					extendedClasses = extendedClasses.TrimEnd();
				}
			}
			//interfaces are treated as extended classes in SrcML for now
			string implementedInterfaces = String.Empty;

			string fullFilePath = System.IO.Path.GetFullPath(fileName);
			string snippet = RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

			return new ClassElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, implementedInterfaces);
		}

		private void ParseFunctions(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> functions =
				from el in elements.Descendants(SourceNamespace + "function")
				select el;
			foreach(XElement func in functions)
			{
				MethodElement methodElement = ParseFunction(func, programElements, fileName);
				programElements.Add(methodElement);
			}
		}

		private MethodElement ParseFunction(XElement function, List<ProgramElement> programElements, string fileName)
		{
			//parse name etc.
			XElement nameElement = function.Element(SourceNamespace + "name");
			string name = nameElement.Value;
			int definitionLineNumber = Int32.Parse(nameElement.Attribute(PositionNamespace + "line").Value);
			
			XElement access = function.Element(SourceNamespace + "type").Element(SourceNamespace + "specifier");
			AccessLevel accessLevel = StrToAccessLevel(access.Value);
			
			XElement type = function.Element(SourceNamespace + "type").Element(SourceNamespace + "name");
			string returnType = type.Value;

			//parse arguments
			XElement paramlist = function.Element(SourceNamespace + "parameter_list");
			IEnumerable<XElement> argumentElements =
				from el in paramlist.Descendants(SourceNamespace + "name")
				select el;
			string arguments = String.Empty;
			foreach(XElement elem in argumentElements)
			{
				arguments += elem.Value + " ";
			}
			arguments = arguments.TrimEnd();

			//parse function body
			XElement block = function.Element(SourceNamespace + "block");
			IEnumerable<XElement> bodyNames =
				from el in block.Descendants(SourceNamespace + "name")
				select el;
			string body = String.Empty;
			foreach(XElement elem in bodyNames)
			{
				body += elem.Value + " ";
			}
			body = body.TrimEnd();

			Guid classId = RetrieveClassGuid(function, programElements);

			string fullFilePath = System.IO.Path.GetFullPath(fileName);
			string snippet = RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

			return new MethodElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, classId);
		}

		private Guid RetrieveClassGuid(XElement field, List<ProgramElement> programElements)
		{
			IEnumerable<XElement> ownerClasses =
				from el in field.Ancestors(SourceNamespace + "class")
				select el;
			if(ownerClasses.Count() > 0)
			{
				//this ignores the possibility that a field may be part of an inner class
				XElement name = ownerClasses.First().Element(SourceNamespace + "name");
				String ownerClassName = name.Value;
				//now find the ClassElement object corresponding to ownerClassName, since those should have been gen'd by now
				ProgramElement ownerClass = programElements.Find(element => element is ClassElement && ((ClassElement)element).Name == ownerClassName);
				if(ownerClass != null)
				{
					return ownerClass.Id;
				}
				else
				{
					return System.Guid.Empty;
				}
			}
			else
			{
				//field is not contained by a class
				return System.Guid.Empty;
			}
		}

		private string RetrieveSnippet(String filename, int line, int snippetNumLines)
		{
			string[] lines = System.IO.File.ReadAllLines(System.IO.Path.GetFullPath(filename));

			//start at one line above the definition of the program element
			int linesAbove = 1;

			int startLine = line - linesAbove - 1;
			IEnumerable<string> snipLines = lines.Skip(startLine).Take(snippetNumLines - linesAbove);
			return snipLines.Aggregate((snip, nextLine) => snip + Environment.NewLine + nextLine);
		}

		private AccessLevel StrToAccessLevel(String level)
		{
			return (AccessLevel)Enum.Parse(typeof (AccessLevel), level, true);		
 		}
	}
}
