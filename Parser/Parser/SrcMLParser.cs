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

			ParseEnums(programElements, sourceElements);
			ParseClasses(programElements, sourceElements);
			ParseFunctions(programElements, sourceElements);

			foreach(ProgramElement pe in programElements) 
			{
				pe.FullFilePath = System.IO.Path.GetFullPath(filename);
			}

			return programElements.ToArray();
		}

		private void ParseEnums(List<ProgramElement> programElements, XElement elements)
		{
			IEnumerable<XElement> enums =
				from el in elements.Descendants(SourceNamespace + "enum")
				select el;
			foreach(XElement enm in enums)
			{
				EnumElement enumElement = new EnumElement();
				enumElement.Id = System.Guid.NewGuid();

				//SrcML doesn't parse access level specifiers for enums, so just pretend they are all public for now
				enumElement.AccessLevel = AccessLevel.Public;

				//parse name, and definition line number
				XElement name = enm.Element(SourceNamespace + "name");
				enumElement.Name = name.Value;
				enumElement.DefinitionLineNumber = Int32.Parse(name.Attribute(PositionNamespace + "line").Value);

				//parse namespace
				IEnumerable<XElement> ownerNamespaces =
					from el in enm.Ancestors(SourceNamespace + "decl")
					where el.Element(SourceNamespace + "type").Element(SourceNamespace + "name").Value == "namespace"
					select el;
				foreach(XElement ownerNamespace in ownerNamespaces)
				{
					foreach(XElement spc in ownerNamespace.Elements(SourceNamespace + "name"))
					{
						enumElement.Namespace = enumElement.Namespace + spc.Value + " ";
					}
				}
				if(enumElement.Namespace != null && enumElement.Namespace.Length > 0)
					enumElement.Namespace = enumElement.Namespace.TrimEnd();

				//parse values
				XElement block = enm.Element(SourceNamespace + "block");
				if(block != null)
				{
					IEnumerable<XElement> exprs =
						from el in block.Descendants(SourceNamespace + "expr")
						select el;
					foreach(XElement expr in exprs)
					{
						enumElement.Values = enumElement.Values + expr.Element(SourceNamespace + "name").Value + " ";
					}
					if(enumElement.Values != null && enumElement.Values.Length > 0)
					{
						enumElement.Values = enumElement.Values.TrimEnd();
					}
				}

				programElements.Add(enumElement);
			}
		}

		private void ParseClasses(List<ProgramElement> programElements, XElement elements)
		{
			IEnumerable<XElement> classes =
				from el in elements.Descendants(SourceNamespace + "class") 
				select el;
			foreach(XElement cls in classes)
			{
				programElements.Add(ParseClass(cls));
			}
		} 

		private ClassElement ParseClass(XElement cls)
		{
			var classElement = new ClassElement();
			classElement.Id = System.Guid.NewGuid();

			//parse stuff in class definition
			XElement name = cls.Element(SourceNamespace + "name");
			classElement.Name = name.Value;
			classElement.DefinitionLineNumber = Int32.Parse(name.Attribute(PositionNamespace + "line").Value);
			XElement access = cls.Element(SourceNamespace + "specifier");
			classElement.AccessLevel = StrToAccessLevel(access.Value);

			//parse namespace
			IEnumerable<XElement> ownerNamespaces =
				from el in cls.Ancestors(SourceNamespace + "decl")
				where el.Element(SourceNamespace + "type").Element(SourceNamespace + "name").Value == "namespace"
				select el;
			foreach(XElement ownerNamespace in ownerNamespaces)
			{
				foreach(XElement spc in ownerNamespace.Elements(SourceNamespace + "name"))
				{
					classElement.Namespace = classElement.Namespace + spc.Value + " ";
				}
			}
			if(classElement.Namespace != null && classElement.Namespace.Length > 0) 
				classElement.Namespace = classElement.Namespace.TrimEnd();

			//parse extended classes and implemented interfaces (interfaces are treated as extended classes in SrcML for now)
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
						classElement.ImplementedInterfaces = classElement.ImplementedInterfaces + impName.Value + " ";
					}
					classElement.ImplementedInterfaces = classElement.ImplementedInterfaces.TrimEnd();
				}
			}

			return classElement;
		}

		private void ParseFunctions(List<ProgramElement> programElements, XElement elements)
		{
			IEnumerable<XElement> functions =
				from el in elements.Descendants(SourceNamespace + "function")
				select el;
			foreach(XElement func in functions)
			{
				MethodElement methodElement = ParseFunction(func);
				AssociateMethodWithClass(methodElement, func, programElements);
				programElements.Add(methodElement);
			}
		}

		private MethodElement ParseFunction(XElement function)
		{
			var method = new MethodElement();
			method.Id = System.Guid.NewGuid();

			//parse name etc.
			XElement name = function.Element(SourceNamespace + "name");
			method.Name = name.Value;
			method.DefinitionLineNumber = Int32.Parse(name.Attribute(PositionNamespace + "line").Value);
			XElement access = function.Element(SourceNamespace + "type").Element(SourceNamespace + "specifier");
			method.AccessLevel = StrToAccessLevel(access.Value);
			XElement type = function.Element(SourceNamespace + "type").Element(SourceNamespace + "name");
			method.ReturnType = type.Value;

			//parse arguments
			XElement paramlist = function.Element(SourceNamespace + "parameter_list");
			IEnumerable<XElement> arguments =
				from el in paramlist.Descendants(SourceNamespace + "name")
				select el;
			method.Arguments = ""; 
			foreach(XElement elem in arguments)
			{
				method.Arguments = method.Arguments + elem.Value + " ";
			}
			method.Arguments = method.Arguments.TrimEnd();

			//parse function body
			XElement block = function.Element(SourceNamespace + "block");
			IEnumerable<XElement> bodyNames =
				from el in block.Descendants(SourceNamespace + "name")
				select el;
			foreach(XElement elem in bodyNames)
			{
				method.Body = method.Body + elem.Value + " ";
			}
			method.Body = method.Body.TrimEnd();

			return method;
		}

		private void AssociateMethodWithClass(MethodElement method, XElement function, List<ProgramElement> programElements)
		{
			IEnumerable<XElement> ownerClasses =
				from el in function.Ancestors(SourceNamespace + "class")
				select el;
			if(ownerClasses.Count() > 0)
			{
				//this ignores the possibility that a method may be part of an inner class
				XElement name = ownerClasses.First().Element(SourceNamespace + "name");
				String ownerClassName = name.Value;
				//now find the ClassElement object corresponding to ownerClassName, since those should have been gen'd by now
				ProgramElement ownerClass = programElements.Find(element => element is ClassElement && ((ClassElement)element).Name == ownerClassName);
				if(ownerClass != null)
				{
					method.ClassId = ownerClass.Id;
				}
				else
				{
					method.ClassId = System.Guid.Empty;
				}
			}
			else
			{
				//method is not contained by a class
				method.ClassId = System.Guid.Empty;
			}
		}

		private AccessLevel StrToAccessLevel(String level)
		{
			return (AccessLevel)Enum.Parse(typeof (AccessLevel), level, true);		
 		}
	}
}
