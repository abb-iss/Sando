using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
	public class SrcMLCSharpParser : IParser
	{
		private readonly SrcMLGenerator Generator;
		private static readonly int SnippetSize = 5;
		private static readonly XNamespace SourceNamespace = "http://www.sdml.info/srcML/src";
		private static readonly XNamespace PositionNamespace = "http://www.sdml.info/srcML/position";

	    public static readonly string StandardSrcMlLocation = Environment.CurrentDirectory + "\\..\\..\\LIBS\\srcML-Win";

	    public SrcMLCSharpParser(string pluginDirectory = null)
		{
			//try to set this up automatically			
			Generator = new SrcMLGenerator(LanguageEnum.CSharp);	   
            if(pluginDirectory!=null)
                Generator.SetSrcMLLocation(pluginDirectory);
            else
	            Generator.SetSrcMLLocation(StandardSrcMlLocation);
		}

		public List<ProgramElement> Parse(string fileName)
		{
			var programElements = new List<ProgramElement>();
			string srcml = Generator.GenerateSrcML(fileName);

			if(srcml != String.Empty)
			{
				XElement sourceElements = XElement.Parse(srcml);

				//classes have to be parsed first
				ParseClasses(programElements, sourceElements, fileName);

				ParseEnums(programElements, sourceElements, fileName, SnippetSize);
				SrcMLParsingUtils.ParseFields(programElements, sourceElements, fileName, SnippetSize);
				ParseConstructors(programElements, sourceElements, fileName);
				ParseMethods(programElements, sourceElements, fileName);
				ParseProperties(programElements, sourceElements, fileName);
			}

			return programElements;
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
				string name;
				int definitionLineNumber;
				SrcMLParsingUtils.ParseNameAndLineNumber(prop, out name, out definitionLineNumber);

				ClassElement classElement = SrcMLParsingUtils.RetrieveClassElement(prop, programElements);
				Guid classId = classElement != null ? classElement.Id : Guid.Empty;
				string className = classElement != null ? classElement.Name : String.Empty;

				//parse access level and type
				XElement accessElement = prop.Element(SourceNamespace + "type").Element(SourceNamespace + "specifier");
				AccessLevel accessLevel = accessElement != null ? SrcMLParsingUtils.StrToAccessLevel(accessElement.Value) : AccessLevel.Internal;

				IEnumerable<XElement> types = prop.Element(SourceNamespace + "type").Elements(SourceNamespace + "name");

				//oops, namespaces have the same structure in srcml so need this check
				if(types.First().Value == "namespace") continue;

				string propertyType = String.Empty;
				foreach(XElement type in types)
				{
					propertyType += type.Value + " ";
				}
				propertyType = propertyType.TrimEnd();

				string body = SrcMLParsingUtils.ParseBody(prop);

				string fullFilePath = System.IO.Path.GetFullPath(fileName);
				string snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

				programElements.Add(new PropertyElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, propertyType, body, classId, className, String.Empty));
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
			string name;
			int definitionLineNumber;
			SrcMLParsingUtils.ParseNameAndLineNumber(cls, out name, out definitionLineNumber);

			AccessLevel accessLevel = AccessLevel.Public; //default
			XElement accessElement = cls.Element(SourceNamespace + "specifier");
			if(accessElement != null)
			{
				accessLevel = SrcMLParsingUtils.StrToAccessLevel(accessElement.Value);
			}

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
			string snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

			return new ClassElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, implementedInterfaces, String.Empty);
		}

		private void ParseConstructors(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> constructors =
				from el in elements.Descendants(SourceNamespace + "constructor")
				select el;
			foreach(XElement cons in constructors)
			{
				MethodElement methodElement = null;
				methodElement = ParseMethod(cons, programElements, fileName, true);
				programElements.Add(methodElement);
				DocCommentElement methodCommentsElement = SrcMLParsingUtils.ParseFunctionComments(cons, methodElement);
				if(methodCommentsElement != null)
				{
					programElements.Add(methodCommentsElement);
				}
			}
		}

		private void ParseMethods(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> functions =
				from el in elements.Descendants(SourceNamespace + "function")
				select el;
			foreach(XElement func in functions)
			{
				MethodElement methodElement = null;
				methodElement = ParseMethod(func, programElements, fileName);
				programElements.Add(methodElement);
				DocCommentElement methodCommentsElement = SrcMLParsingUtils.ParseFunctionComments(func, methodElement);
				if(methodCommentsElement != null)
				{
					programElements.Add(methodCommentsElement);
				}
			}
		}

        private MethodElement ParseMethod(XElement method, List<ProgramElement> programElements, string fileName, bool isConstructor = false)
        {
            return ParseMethod(method, programElements, fileName, typeof (MethodElement), isConstructor);
        }

		public virtual MethodElement ParseMethod(XElement method, List<ProgramElement> programElements, string fileName, Type myType, bool isConstructor = false)
		{
			string name = String.Empty;
			int definitionLineNumber = 0;
			string returnType = String.Empty;

			SrcMLParsingUtils.ParseNameAndLineNumber(method, out name, out definitionLineNumber);

			AccessLevel accessLevel = AccessLevel.Protected; //default
			XElement type = method.Element(SourceNamespace + "type");
			if(type != null)
			{
				XElement access = type.Element(SourceNamespace + "specifier");
				if(access != null)
				{
					accessLevel = SrcMLParsingUtils.StrToAccessLevel(access.Value);
				}

				XElement typeName = type.Element(SourceNamespace + "name");
				returnType = typeName.Value;
			}
			else
			{
				XElement access = method.Element(SourceNamespace + "specifier");
				if(access != null)
				{
					accessLevel = SrcMLParsingUtils.StrToAccessLevel(access.Value);
				}
			}


			//parse arguments
			XElement paramlist = method.Element(SourceNamespace + "parameter_list");
			IEnumerable<XElement> argumentElements =
				from el in paramlist.Descendants(SourceNamespace + "name")
				select el;
			string arguments = String.Empty;
			foreach(XElement elem in argumentElements)
			{
				arguments += elem.Value + " ";
			}
			arguments = arguments.TrimEnd();

			string body = SrcMLParsingUtils.ParseBody(method);

			ClassElement classElement = SrcMLParsingUtils.RetrieveClassElement(method, programElements);
			Guid classId = classElement != null ? classElement.Id : Guid.Empty;
			string className = classElement != null ? classElement.Name : String.Empty;

			string fullFilePath = System.IO.Path.GetFullPath(fileName);
			
            string snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

            return Activator.CreateInstance(myType, name, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body,
                                        classId, className, String.Empty, isConstructor) as MethodElement;			
		}


		public static void ParseEnums(List<ProgramElement> programElements, XElement elements, string fileName, int snippetSize)
		{
			IEnumerable<XElement> enums =
				from el in elements.Descendants(SourceNamespace + "enum")
				select el;

			foreach(XElement enm in enums)
			{
				AccessLevel accessLevel = AccessLevel.Public; //default
				XElement access = enm.Element(SourceNamespace + "specifier");
				if(access != null)
				{
					accessLevel = SrcMLParsingUtils.StrToAccessLevel(access.Value);
				}

				string name;
				int definitionLineNumber;
				SrcMLParsingUtils.ParseNameAndLineNumber(enm, out name, out definitionLineNumber);

				//parse namespace
				IEnumerable<XElement> ownerNamespaces =
					from el in enm.Ancestors(SourceNamespace + "decl")
					where el.Element(SourceNamespace + "type") != null &&
							el.Element(SourceNamespace + "type").Element(SourceNamespace + "name") != null &&
							el.Element(SourceNamespace + "type").Element(SourceNamespace + "name").Value == "namespace"
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
						IEnumerable<XElement> enames = expr.Elements(SourceNamespace + "name");
						foreach(XElement ename in enames)
						{
							values += ename.Value + " ";
						}
					}
					values = values.TrimEnd();
				}

				string fullFilePath = System.IO.Path.GetFullPath(fileName);
				string snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, snippetSize);

				programElements.Add(new EnumElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, values));
			}
		}
	}

}
