using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ABB.SrcML.VisualStudio.SrcMLService;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using ABB.SrcML;
using Sando.Core.Logging.Events;
using Sando.Core.Logging.Persistence;

namespace Sando.Parser
{
    public class SrcMLCppParser : IParser
    {
        public ISrcMLGlobalService SrcMLService;
        public SrcMLArchive Archive { get; set; }           // should be deleted
        public SrcMLGenerator Generator { get; set; }

        public SrcMLCppParser() {
        }

        public SrcMLCppParser(ISrcMLGlobalService srcmlService) {
            this.SrcMLService = srcmlService;
        }

        public SrcMLCppParser(SrcMLArchive archive) {
            this.Archive = archive;
        }

        public SrcMLCppParser(SrcMLGenerator generator) {
            this.Generator = generator;
        }

        public List<ProgramElement> Parse(string fileName) {
            var programElements = new List<ProgramElement>();
            XElement sourceElements;
            if(SrcMLService != null) {
                sourceElements = SrcMLService.GetXElementForSourceFile(fileName);
                if(sourceElements != null) {
                    programElements = Parse(fileName, sourceElements);
                } else {
                    FileLogger.DefaultLogger.ErrorFormat("SrcMLCppParser: File not found in SrcMLService: {0}", fileName);
                }
            } else if(Archive != null) {
                sourceElements = Archive.GetXElementForSourceFile(fileName);
                if(sourceElements != null) {
                    programElements = Parse(fileName, sourceElements);
                } else {
                    LogEvents.ParserFileNotFoundInArchiveError(this, fileName);
                }
            } else if(Generator != null) {
                string outFile = Path.GetTempFileName();
                try {
                    //This is a C++ parser, so we'll convert the input file as C++ no matter what the file extension is
                    Generator.GenerateSrcMLFromFile(fileName, outFile, Language.CPlusPlus);
                    sourceElements = SrcMLElement.Load(outFile);                                        
                    if(sourceElements != null) {
                        programElements = Parse(fileName, sourceElements);
                    }
                } finally {
                    File.Delete(outFile);
                }
            } else {
                throw new InvalidOperationException("SrcMLCppParser - Archive and Generator are both null");
            }

            return programElements;
        }

        

        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// New Parse method that takes both source file path and the XElement representation of the source file as input arguments.
        /// TODO: what if the XElement is null?
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceElements"></param>
        /// <returns></returns>
        public List<ProgramElement> Parse(string fileName, XElement sourceElements)
        {            
            var programElements = new List<ProgramElement>();

            //classes and structs have to parsed first
            ParseClasses(programElements, sourceElements, fileName);
            ParseStructs(programElements, sourceElements, fileName);

            SrcMLParsingUtils.ParseFields(programElements, sourceElements, fileName);
            ParseCppEnums(programElements, sourceElements, fileName);
            ParseConstructors(programElements, sourceElements, fileName);
            ParseFunctions(programElements, sourceElements, fileName);
            ParseCppFunctionPrototypes(programElements, sourceElements, fileName);
            ParseCppConstructorPrototypes(programElements, sourceElements, fileName);
            SrcMLParsingUtils.ParseComments(programElements, sourceElements, fileName);

            return programElements;
        }
        // End of code changes

        private void ParseCppFunctionPrototypes(List<ProgramElement> programElements, XElement sourceElements, string fileName)
        {
            IEnumerable<XElement> functions =
                from el in sourceElements.Descendants(SRC.FunctionDeclaration)
                select el;
            foreach (XElement function in functions)
            {
                var func = ParseCppFunctionPrototype(function, fileName, false);
                if(func!=null)
                    programElements.Add(func);
            }
        }

        private void ParseCppConstructorPrototypes(List<ProgramElement> programElements, XElement sourceElements, string fileName)
        {
            IEnumerable<XElement> functions =
                from el in sourceElements.Descendants(SRC.ConstructorDeclaration)
                select el;
            foreach (XElement function in functions)
            {
                var prototype = ParseCppFunctionPrototype(function, fileName, true);
                if(prototype!=null)
                    programElements.Add(prototype);
            }
        }

        private MethodPrototypeElement ParseCppFunctionPrototype(XElement function, string fileName, bool isConstructor)
        {
            try
            {
                string name = String.Empty;
                int definitionLineNumber = 0;
                int definitionColumnNumber = 0;
                string returnType = String.Empty;                
                SrcMLParsingUtils.ParseNameAndLineNumber(function, out name, out definitionLineNumber, out definitionColumnNumber);
                if (name.Contains("::"))
                {
                    name = name.Substring(name.LastIndexOf("::") + 2);
                }
                AccessLevel accessLevel = RetrieveCppAccessLevel(function);
                XElement type = function.Element(SRC.Type);
                if (type != null)
                {
                    XElement typeName = type.Element(SRC.Name);
                    returnType = typeName.Value;
                }

                XElement paramlist = function.Element(SRC.ParameterList);
                IEnumerable<XElement> argumentElements =
                    from el in paramlist.Descendants(SRC.Name)
                    select el;
                string arguments = String.Empty;
                foreach (XElement elem in argumentElements)
                {
                    arguments += elem.Value + " ";
                }
                arguments = arguments.TrimEnd();

                string fullFilePath = System.IO.Path.GetFullPath(fileName);
                string source = SrcMLParsingUtils.RetrieveSource(function);

                return new MethodPrototypeElement(name, definitionLineNumber, definitionColumnNumber, returnType, accessLevel, arguments, fullFilePath, source, isConstructor);
            }
            catch (Exception error)
            {
                FileLogger.DefaultLogger.Info("Exception in SrcMLCppParser " + error.Message + "\n" + error.StackTrace);
                return null;
            }
        }


        private string[] ParseCppIncludes(XElement sourceElements)
        {
            List<string> includeFileNames = new List<string>();
            try
            {
                IEnumerable<XElement> includeStatements =
                    from el in sourceElements.Descendants(CPP.Include)
                    select el;

                foreach (XElement include in includeStatements)
                {
                    string filename = include.Element(CPP.File).Value;
                    if (filename.Substring(0, 1) == "<") continue; //ignore includes of system files -> they start with a bracket
                    filename = filename.Substring(1, filename.Length - 2);	//remove quotes	
                    includeFileNames.Add(filename);
                }
            }
            catch (Exception error)
            {
                FileLogger.DefaultLogger.Info("Exception in SrcMLCppParser.ParseCppIncludes() " + error.Message + "\n" + error.StackTrace);
            }
            return includeFileNames.ToArray();
        }

        private void ParseClasses(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> classes =
                from el in elements.Descendants(SRC.Class)
                select el;
            foreach (XElement cls in classes)
            {
                var classElement = (ClassElement)ParseClassOrStruct(cls, fileName, false);
                if(classElement!=null)
                    programElements.Add(classElement);
            }
        }

        private void ParseStructs(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> classes =
                from el in elements.Descendants(SRC.Struct)
                select el;
            foreach (XElement cls in classes)
            {
                var structure = (StructElement)ParseClassOrStruct(cls, fileName, true);
                if(structure!=null)
                    programElements.Add(structure);
            }
        }

        private ProgramElement ParseClassOrStruct(XElement cls, string fileName, bool parseStruct)
        {
            string name;
            int definitionLineNumber;
            int definitionColumnNumber;
            SrcMLParsingUtils.ParseNameAndLineNumber(cls, out name, out definitionLineNumber, out definitionColumnNumber);

            AccessLevel accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(cls, AccessLevel.Public);

            //parse namespace
            IEnumerable<XElement> ownerNamespaces =
                from el in cls.Ancestors(SRC.Declaration)
                where el.Element(SRC.Type).Element(SRC.Name).Value == "namespace"
                select el;
            string namespaceName = String.Empty;
            foreach (XElement ownerNamespace in ownerNamespaces)
            {
                foreach (XElement spc in ownerNamespace.Elements(SRC.Name))
                {
                    namespaceName += spc.Value + " ";
                }
            }
            namespaceName = namespaceName.TrimEnd();

            //parse extended classes 
            string extendedClasses = String.Empty;
            XElement super = cls.Element(SRC.Super);
            if (super != null)
            {
                XElement implements = super.Element(SRC.Implements);
                if (implements != null)
                {
                    IEnumerable<XElement> impNames =
                        from el in implements.Descendants(SRC.Name)
                        select el;
                    foreach (XElement impName in impNames)
                    {
                        extendedClasses += impName.Value + " ";
                    }
                    extendedClasses = extendedClasses.TrimEnd();
                }
            }

            string fullFilePath = System.IO.Path.GetFullPath(fileName);
            string source = SrcMLParsingUtils.RetrieveSource(cls);

            string body = cls.Value;
            if (parseStruct)
            {
                return new StructElement(name, definitionLineNumber, definitionColumnNumber, fullFilePath, source, accessLevel, namespaceName, body, extendedClasses, String.Empty);
            }
            else
            {
                string implementedInterfaces = String.Empty;
                return new ClassElement(name, definitionLineNumber, definitionColumnNumber, fullFilePath, source, accessLevel, namespaceName,
                    extendedClasses, implementedInterfaces, String.Empty, body);
            }
        }

        private void ParseConstructors(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            string[] includedFiles = ParseCppIncludes(elements);
            IEnumerable<XElement> constructors =
                from el in elements.Descendants(SRC.Constructor)
                select el;
            foreach (XElement cons in constructors)
            {
                var constructor = ParseCppFunction(cons, programElements, fileName, includedFiles, typeof(MethodElement), typeof(CppUnresolvedMethodElement), true);
                if(constructor!=null)
                    programElements.Add(constructor);
            }
        }

        private void ParseFunctions(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            string[] includedFiles = ParseCppIncludes(elements);
            IEnumerable<XElement> functions =
                from el in elements.Descendants(SRC.Function)
                select el;
            foreach (XElement func in functions)
            {
                var method = ParseCppFunction(func, programElements, fileName, includedFiles, typeof(MethodElement), typeof(CppUnresolvedMethodElement));
                if(method!=null)
                    programElements.Add(method);
            }
        }

        public virtual MethodElement ParseCppFunction(XElement function, List<ProgramElement> programElements, string fileName,
                                                string[] includedFiles, Type resolvedType, Type unresolvedType, bool isConstructor = false)
        {
            try
            {
                MethodElement methodElement = null;
                string source = String.Empty;
                int definitionLineNumber = 0;
                int definitionColumnNumber = 0;
                string returnType = String.Empty;

                XElement type = function.Element(SRC.Type);
                if (type != null)
                {
                    XElement typeName = type.Element(SRC.Name);
                    returnType = typeName.Value;
                }

                XElement paramlist = function.Element(SRC.ParameterList);
                IEnumerable<XElement> argumentElements =
                    from el in paramlist.Descendants(SRC.Name)
                    select el;
                string arguments = String.Empty;
                foreach (XElement elem in argumentElements)
                {
                    arguments += elem.Value + " ";
                }
                arguments = arguments.TrimEnd();

                string body = SrcMLParsingUtils.ParseBody(function);
                string fullFilePath = System.IO.Path.GetFullPath(fileName);


                XElement nameElement = function.Element(SRC.Name);
                string wholeName = nameElement.Value;
                if (wholeName.Contains("::"))
                {
                    //class function
                    string[] twonames = wholeName.Split("::".ToCharArray());
                    string funcName = twonames[2];
                    string className = twonames[0];
                    definitionLineNumber = Int32.Parse(nameElement.Element(SRC.Name).Attribute(POS.Line).Value);
                    definitionColumnNumber = Int32.Parse(nameElement.Element(SRC.Name).Attribute(POS.Column).Value);
                    source = SrcMLParsingUtils.RetrieveSource(function);

                    return Activator.CreateInstance(unresolvedType, funcName, definitionLineNumber, definitionColumnNumber, fullFilePath, source, arguments, returnType, body,
                                                            className, isConstructor, includedFiles) as MethodElement;
                }
                else
                {
                    //regular C-type function, or an inlined class function
                    string funcName = wholeName;
                    definitionLineNumber = Int32.Parse(nameElement.Attribute(POS.Line).Value);
                    definitionColumnNumber = Int32.Parse(nameElement.Attribute(POS.Column).Value);
                    source = SrcMLParsingUtils.RetrieveSource(function);
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
                    methodElement = Activator.CreateInstance(resolvedType, funcName, definitionLineNumber, definitionColumnNumber, fullFilePath, source, accessLevel, arguments,
                                             returnType, body,
                                             classId, className, String.Empty, isConstructor) as MethodElement;
                }

                return methodElement;
            }
            catch (Exception error)
            {
                FileLogger.DefaultLogger.Info("Exception in SrcMLCppParser " + error.Message + "\n" + error.StackTrace);
                return null;
            }
        }

        public static void ParseCppEnums(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> enums =
                from el in elements.Descendants(SRC.Enum)
                select el;

            foreach (XElement enm in enums)
            {
                var enumParsed = ParseEnum(fileName, enm);
                if(enumParsed!=null)
                    programElements.Add(enumParsed);
            }
        }

        private static EnumElement ParseEnum(string fileName, XElement enm)
        {
            try
            {
                //SrcML doesn't seem to parse access level specifiers for enums, so just pretend they are all public for now
                AccessLevel accessLevel = AccessLevel.Public;

                string name = "";
                int definitionLineNumber = 0;
                int definitionColumnNumber=0;
                if (enm.Element(SRC.Name) != null)
                {
                    SrcMLParsingUtils.ParseNameAndLineNumber(enm, out name, out definitionLineNumber, out definitionColumnNumber);
                }
                else
                {
                    //enums in C++ aren't required to have a name
                    name = ProgramElement.UndefinedName;
                    definitionLineNumber = Int32.Parse(enm.Attribute(POS.Line).Value);
                }

                //parse namespace
                IEnumerable<XElement> ownerNamespaces =
                    from el in enm.Ancestors(SRC.Declaration)
                    where el.Element(SRC.Type) != null &&
                            el.Element(SRC.Type).Element(SRC.Name) != null &&
                            el.Element(SRC.Type).Element(SRC.Name).Value == "namespace"
                    select el;
                string namespaceName = String.Empty;
                foreach (XElement ownerNamespace in ownerNamespaces)
                {
                    foreach (XElement spc in ownerNamespace.Elements(SRC.Name))
                    {
                        namespaceName += spc.Value + " ";
                    }
                }
                namespaceName = namespaceName.TrimEnd();


                //parse values
                XElement block = enm.Element(SRC.Block);
                string values = String.Empty;
                if (block != null)
                {
                    IEnumerable<XElement> exprs =
                        from el in block.Descendants(SRC.Expression)
                        select el;
                    foreach (XElement expr in exprs)
                    {
                        IEnumerable<XElement> enames = expr.Elements(SRC.Name);
                        foreach (XElement ename in enames)
                        {
                            values += ename.Value + " ";
                        }
                    }
                    values = values.TrimEnd();
                }

                string fullFilePath = System.IO.Path.GetFullPath(fileName);
                string source = SrcMLParsingUtils.RetrieveSource(enm);
                var enumParsed = new EnumElement(name, definitionLineNumber, definitionColumnNumber, fullFilePath, source, accessLevel, namespaceName, values);
                return enumParsed;
            }
            catch (Exception error)
            {
                FileLogger.DefaultLogger.Info("Exception in SrcMLCppParser " + error.Message + "\n" + error.StackTrace);
                return null;
            }
        }

        public static StructElement RetrieveStructElement(XElement field, List<ProgramElement> programElements)
        {
            IEnumerable<XElement> ownerStructs =
                from el in field.Ancestors(SRC.Struct)
                select el;
            if (ownerStructs.Count() > 0)
            {
                XElement name = ownerStructs.First().Element(SRC.Name);
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
            if (parent.Name == (SRC.Public))
            {
                accessLevel = AccessLevel.Public;
            }
            else if (parent.Name == (SRC.Private))
            {
                accessLevel = AccessLevel.Private;
            }

            return accessLevel;
        }

    }
}
