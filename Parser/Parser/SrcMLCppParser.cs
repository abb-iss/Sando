using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
	public class SrcMLCppParser : IParser
	{
		private readonly SrcMLGenerator Generator;

		private static readonly XNamespace SourceNamespace = "http://www.sdml.info/srcML/src";
		private static readonly XNamespace PositionNamespace = "http://www.sdml.info/srcML/position";
		private static readonly int SnippetSize = 5;
	    public static readonly string StandardSrcMlLocation = Environment.CurrentDirectory + "\\..\\..\\LIBS\\srcML-Win";

	    public SrcMLCppParser()
		{
			//try to set this up automatically			
			Generator = new SrcMLGenerator(LanguageEnum.CPP);	        
	        Generator.SetSrcMLLocation(StandardSrcMlLocation);
		}

		public List<ProgramElement> Parse(string fileName)
		{
			var programElements = new List<ProgramElement>();
			string srcml = Generator.GenerateSrcML(fileName);

			if(srcml != String.Empty)
			{
				XElement sourceElements = XElement.Parse(srcml);

				//classes and structs have to parsed first
				ParseClasses(programElements, sourceElements, fileName);
                ParseStructs(programElements, sourceElements, fileName);

				ParseCppEnums(programElements, sourceElements, fileName, SnippetSize);
				SrcMLParsingUtils.ParseFields(programElements, sourceElements, fileName, SnippetSize);
				ParseConstructors(programElements, sourceElements, fileName);
				ParseFunctions(programElements, sourceElements, fileName);
				ParseCppFunctionPrototypes(programElements, sourceElements, fileName);
				ParseCppConstructorPrototypes(programElements, sourceElements, fileName);
			}

			return programElements;
		}

		private void ParseCppFunctionPrototypes(List<ProgramElement> programElements, XElement sourceElements, string fileName)
		{
			IEnumerable<XElement> functions =
				from el in sourceElements.Descendants(SourceNamespace + "function_decl")
				select el;
			ParseCppFunctionPrototype(programElements, functions, fileName, false);
		}

		private void ParseCppConstructorPrototypes(List<ProgramElement> programElements, XElement sourceElements, string fileName)
		{
			IEnumerable<XElement> functions =
				from el in sourceElements.Descendants(SourceNamespace + "constructor_decl")
				select el;
			ParseCppFunctionPrototype(programElements, functions, fileName, true);
		}

		private void ParseCppFunctionPrototype(List<ProgramElement> programElements, IEnumerable<XElement> functions, 
														string fileName, bool isConstructor)
		{
			foreach(XElement function in functions)
			{
				string name = String.Empty;
				int definitionLineNumber = 0;
				string returnType = String.Empty;

				SrcMLParsingUtils.ParseNameAndLineNumber(function, out name, out definitionLineNumber);
				AccessLevel accessLevel = RetrieveCppAccessLevel(function);
				XElement type = function.Element(SourceNamespace + "type");
				if(type != null)
				{
					XElement typeName = type.Element(SourceNamespace + "name");
					returnType = typeName.Value;
				}

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

				string fullFilePath = System.IO.Path.GetFullPath(fileName);
				string snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

				programElements.Add(new MethodPrototypeElement(name, definitionLineNumber, returnType, accessLevel, arguments, fullFilePath, snippet, isConstructor));
			}
		}


		private string[] ParseCppIncludes(XElement sourceElements)
		{
			List<string> includeFileNames = new List<string>();
			XNamespace CppNamespace = "http://www.sdml.info/srcML/cpp";
			IEnumerable<XElement> includeStatements =
				from el in sourceElements.Descendants(CppNamespace + "include")
				select el;

			foreach(XElement include in includeStatements)
			{
				string filename = include.Element(CppNamespace + "file").Value;
				if(filename.Substring(0, 1) == "<") continue; //ignore includes of system files -> they start with a bracket
				filename = filename.Substring(1, filename.Length - 2);	//remove quotes	
				includeFileNames.Add(filename);
			}

			return includeFileNames.ToArray();
		}

		private void ParseClasses(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			IEnumerable<XElement> classes =
				from el in elements.Descendants(SourceNamespace + "class")
				select el;
			foreach(XElement cls in classes)
			{
				programElements.Add((ClassElement)ParseClassOrStruct(cls, fileName, false));
			}
		}

        private void ParseStructs(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> classes =
                from el in elements.Descendants(SourceNamespace + "struct")
                select el;
            foreach (XElement cls in classes)
            {
                programElements.Add((StructElement)ParseClassOrStruct(cls, fileName, true));
            }
        }

		private ProgramElement ParseClassOrStruct(XElement cls, string fileName, bool parseStruct)
		{
			string name;
			int definitionLineNumber;
			SrcMLParsingUtils.ParseNameAndLineNumber(cls, out name, out definitionLineNumber);

            AccessLevel accessLevel = AccessLevel.Public; 
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

			//parse extended classes 
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

			string fullFilePath = System.IO.Path.GetFullPath(fileName);
			string snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

            if(parseStruct)
            {
                return new StructElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, String.Empty);
            }
            else
            {
                return new ClassElement(name, definitionLineNumber, fullFilePath, snippet, accessLevel, namespaceName,
                    extendedClasses, implementedInterfaces, String.Empty);
            }
		}

		private void ParseConstructors(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			string[] includedFiles = ParseCppIncludes(elements);
			IEnumerable<XElement> constructors =
				from el in elements.Descendants(SourceNamespace + "constructor")
				select el;
			foreach(XElement cons in constructors)
			{
				MethodElement methodElement = null;
				methodElement = ParseCppFunction(cons, programElements, fileName, includedFiles, true);
				programElements.Add(methodElement);
				DocCommentElement methodCommentsElement = SrcMLParsingUtils.ParseFunctionComments(cons, methodElement);
				if(methodCommentsElement != null)
				{
					programElements.Add(methodCommentsElement);
				}
			}
		}

		private void ParseFunctions(List<ProgramElement> programElements, XElement elements, string fileName)
		{
			string[] includedFiles = ParseCppIncludes(elements);
			IEnumerable<XElement> functions =
				from el in elements.Descendants(SourceNamespace + "function")
				select el;
			foreach(XElement func in functions)
			{
				MethodElement methodElement = null;
				methodElement = ParseCppFunction(func, programElements, fileName, includedFiles);
				programElements.Add(methodElement);
				DocCommentElement methodCommentsElement = SrcMLParsingUtils.ParseFunctionComments(func, methodElement);
				if(methodCommentsElement != null)
				{
					programElements.Add(methodCommentsElement);
				}
			}
		}

		private MethodElement ParseCppFunction(XElement function, List<ProgramElement> programElements, string fileName, 
												string[] includedFiles, bool isConstructor = false)
		{
			MethodElement methodElement = null;
			string snippet = String.Empty;
			int definitionLineNumber = 0;
			string returnType = String.Empty;

			XElement type = function.Element(SourceNamespace + "type");
			if(type != null)
			{
				XElement typeName = type.Element(SourceNamespace + "name");
				returnType = typeName.Value;
			}

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

			string body = SrcMLParsingUtils.ParseBody(function);
			string fullFilePath = System.IO.Path.GetFullPath(fileName);


			XElement nameElement = function.Element(SourceNamespace + "name");
			string wholeName = nameElement.Value;
			if(wholeName.Contains("::"))
			{
				//class function
				string[] twonames = wholeName.Split("::".ToCharArray());
				string funcName = twonames[2];
				string className = twonames[0];
				definitionLineNumber = Int32.Parse(nameElement.Element(SourceNamespace + "name").Attribute(PositionNamespace + "line").Value);
				snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);

				return new CppUnresolvedMethodElement(funcName, definitionLineNumber, fullFilePath, snippet, arguments, returnType, body, 
														className, isConstructor, includedFiles);
			}
			else
			{
				//regular C-type function, or an inlined class function
				string funcName = wholeName;
				definitionLineNumber = Int32.Parse(nameElement.Attribute(PositionNamespace + "line").Value);
				snippet = SrcMLParsingUtils.RetrieveSnippet(fileName, definitionLineNumber, SnippetSize);
				AccessLevel accessLevel = RetrieveCppAccessLevel(function);

                Guid classId = Guid.Empty;
                string className = String.Empty;
				ClassElement classElement = SrcMLParsingUtils.RetrieveClassElement(function, programElements);
                StructElement structElement = RetrieveStructElement(function, programElements);
                if (classElement != null)
                {
                    classId = classElement.Id;
                    className = classElement.Name;
                }
                else if (structElement != null)
                {
                    classId = structElement.Id;
                    className = structElement.Name;
                }

				methodElement = new MethodElement(funcName, definitionLineNumber, fullFilePath, snippet, accessLevel, arguments, returnType, body, 
													classId, className, String.Empty, isConstructor);
			}

			return methodElement;
		}

		public static void ParseCppEnums(List<ProgramElement> programElements, XElement elements, string fileName, int snippetSize)
		{
			IEnumerable<XElement> enums =
				from el in elements.Descendants(SourceNamespace + "enum")
				select el;

			foreach(XElement enm in enums)
			{
				//SrcML doesn't seem to parse access level specifiers for enums, so just pretend they are all public for now
				AccessLevel accessLevel = AccessLevel.Public;

				string name = "";
				int definitionLineNumber = 0;
				if(enm.Element(SourceNamespace + "name") != null)
				{
					SrcMLParsingUtils.ParseNameAndLineNumber(enm, out name, out definitionLineNumber);
				}
				else
				{
					//enums in C++ aren't required to have a name
					name = ProgramElement.UndefinedName;
					definitionLineNumber = Int32.Parse(enm.Attribute(PositionNamespace + "line").Value);
				}

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

        public static StructElement RetrieveStructElement(XElement field, List<ProgramElement> programElements)
        {
            IEnumerable<XElement> ownerStructs =
                from el in field.Ancestors(SourceNamespace + "struct")
                select el;
            if (ownerStructs.Count() > 0)
            {
                XElement name = ownerStructs.First().Element(SourceNamespace + "name");
                string ownerStructName = name.Value;
                //now find the StructElement object corresponding to ownerClassName, since those should have been gen'd by now
                ProgramElement ownerStruct = programElements.Find(element => element is StructElement && ((StructElement)element).Name == ownerStructName);
                return ownerStruct as StructElement;
            }
            else
            {
                //field is not contained by a class
                return null;
            }
        }

		private AccessLevel RetrieveCppAccessLevel(XElement field)
		{
			AccessLevel accessLevel = AccessLevel.Protected;

			XElement parent = field.Parent;
			if(parent.Name == (SourceNamespace + "public"))
			{
				accessLevel = AccessLevel.Public;
			}
			else if(parent.Name == (SourceNamespace + "private"))
			{
				accessLevel = AccessLevel.Private;
			}

			return accessLevel;
		}
	}
}
