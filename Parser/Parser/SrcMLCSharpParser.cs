using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Sando.Core.Logging;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using ABB.SrcML;
using Sando.Core.Logging.Persistence;

namespace Sando.Parser
{
    public class SrcMLCSharpParser : IParser
    {
        private static readonly int snippetSize = 5;

        public SrcMLArchive Archive { get; set; }           // should be deleted
        public SrcMLGenerator Generator { get; set; }

        public SrcMLCSharpParser() {
        }

        public SrcMLCSharpParser(SrcMLArchive archive) {
            this.Archive = archive;
        }

        public SrcMLCSharpParser(SrcMLGenerator generator) {
            this.Generator = generator;
        }
        
        public List<ProgramElement> Parse(string fileName) {
            var programElements = new List<ProgramElement>();
            XElement sourceElements;
            if(Archive != null) {
                sourceElements = Archive.GetXElementForSourceFile(fileName);
                if(sourceElements != null) {
                    programElements = Parse(fileName, sourceElements);
                } else {
                    FileLogger.DefaultLogger.ErrorFormat("SrcMLCSharpParser: File not found in archive: {0}", fileName);
                }
            } else if(Generator != null) {
                string outFile = Path.GetTempFileName();
                try {
                    //This is a CSharp parser, so we'll convert the input file as CSharp no matter what the file extension is
                    var srcmlfile = Generator.GenerateSrcMLFromFile(fileName, outFile, Language.CSharp);
                    sourceElements = srcmlfile.FileUnits.FirstOrDefault();
                    if(sourceElements != null) {
                        programElements = Parse(fileName, sourceElements);
                    }
                } finally {
                    File.Delete(outFile);
                }
            } else {
                throw new InvalidOperationException("SrcMLCSharpParser - Archive and Generator are both null");
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

            //classes and structs have to be parsed first
            ParseClasses(programElements, sourceElements, fileName);
            ParseStructs(programElements, sourceElements, fileName);

            ParseEnums(programElements, sourceElements, fileName, snippetSize);
            SrcMLParsingUtils.ParseFields(programElements, sourceElements, fileName);
            ParseConstructors(programElements, sourceElements, fileName);
            ParseMethods(programElements, sourceElements, fileName);
            ParseProperties(programElements, sourceElements, fileName);
            SrcMLParsingUtils.ParseComments(programElements, sourceElements, fileName);

            return programElements;
        }
        // End of code changes

        private void ParseProperties(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> props =
                from el in elements.Descendants(SRC.Declaration)
                where el.Element(SRC.Name) != null &&
                      el.Element(SRC.Type) != null &&
                      el.Element(SRC.Block) != null &&
                      el.Elements().Count() == 3
                select el;

            foreach (XElement prop in props)
            {
                string name;
                int definitionLineNumber;
                SrcMLParsingUtils.ParseNameAndLineNumber(prop, out name, out definitionLineNumber);

                ClassElement classElement = SrcMLParsingUtils.RetrieveClassElement(prop, programElements);
                Guid classId = classElement != null ? classElement.Id : Guid.Empty;
                string className = classElement != null ? classElement.Name : String.Empty;

                //parse access level and type
                XElement accessElement = prop.Element(SRC.Type);
                AccessLevel accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(accessElement);

                IEnumerable<XElement> types = prop.Element(SRC.Type).Elements(SRC.Name);

                //oops, namespaces have the same structure in srcml so need this check
                if (types.Count() == 0 || types.First().Value == "namespace") continue;

                string propertyType = String.Empty;
                foreach (XElement type in types)
                {
                    propertyType += type.Value + " ";
                }
                propertyType = propertyType.TrimEnd();

                string body = SrcMLParsingUtils.ParseBody(prop);

                string fullFilePath = System.IO.Path.GetFullPath(fileName);
                string source = SrcMLParsingUtils.RetrieveSource(prop);

                programElements.Add(new PropertyElement(name, definitionLineNumber, fullFilePath, source, accessLevel, propertyType, body, classId, className, String.Empty));
            }
        }

        private void ParseClasses(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> classes =
                from el in elements.Descendants(SRC.Class)
                select el;
            foreach (XElement cls in classes)
            {
                programElements.Add(ParseClass(cls, fileName));
            }
        }

        private void ParseStructs(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> structs =
                from el in elements.Descendants(SRC.Struct)
                //where el.Element(SourceNamespace + "type") != null &&
                //      el.Element(SourceNamespace + "type").Element(SourceNamespace + "name") != null &&
                //      el.Element(SourceNamespace + "type").Element(SourceNamespace + "name").Value == "struct"
                select el;
            foreach (XElement strct in structs)
            {
                programElements.Add(ParseStruct(strct, fileName));
            }
        }

        private StructElement ParseStruct(XElement strct, string fileName)
        {
            string name;
            int definitionLineNumber;
            SrcMLParsingUtils.ParseNameAndLineNumber(strct, out name, out definitionLineNumber);

            AccessLevel accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(strct);

            var anc = strct.Ancestors();
            var x = anc;
            //parse namespace
            IEnumerable<XElement> ownerNamespaces =
                from el in strct.Ancestors(SRC.Namespace)
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

            //TODO: extended structs are pretty difficult to parse in SrcML
            string extendedStructs = String.Empty;

            string fullFilePath = System.IO.Path.GetFullPath(fileName);
            string source = SrcMLParsingUtils.RetrieveSource(strct);

            string body = strct.Value;
            return new StructElement(name, definitionLineNumber, fileName, source, accessLevel, namespaceName, body, extendedStructs, String.Empty);
        }

        private ClassElement ParseClass(XElement cls, string fileName)
        {
            string name;
            int definitionLineNumber;
            SrcMLParsingUtils.ParseNameAndLineNumber(cls, out name, out definitionLineNumber);

            AccessLevel accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(cls);

            //parse namespace
            IEnumerable<XElement> ownerNamespaces =
                from el in cls.Ancestors(SRC.Namespace)
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

            //parse extended classes and implemented interfaces (interfaces are treated as extended classes in SrcML for now)
            string extendedClasses = String.Empty;
            XElement super = cls.Element(SRC.Super);
            if (super != null)
            {
                IEnumerable<XElement> impNames =
                    from el in super.Descendants(SRC.Name)
                    select el;
                foreach (XElement impName in impNames)
                {
                    extendedClasses += impName.Value + " ";
                }
                extendedClasses = extendedClasses.TrimEnd();
            }
            //interfaces are treated as extended classes in SrcML for now
            string implementedInterfaces = String.Empty;

            string fullFilePath = System.IO.Path.GetFullPath(fileName);
            string source = SrcMLParsingUtils.RetrieveSource(cls);

            string body = cls.Value;
            return new ClassElement(name, definitionLineNumber, fullFilePath, source, accessLevel, namespaceName, extendedClasses, implementedInterfaces, String.Empty, body);
        }

        private void ParseConstructors(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> constructors =
                from el in elements.Descendants(SRC.Constructor)
                select el;
            foreach (XElement cons in constructors)
            {
                programElements.Add(ParseMethod(cons, programElements, fileName, true));
            }
        }

        private void ParseMethods(List<ProgramElement> programElements, XElement elements, string fileName)
        {
            IEnumerable<XElement> functions =
                from el in elements.Descendants(SRC.Function)
                where el.Element(SRC.Name) != null
                select el;
            foreach (XElement func in functions)
            {
                programElements.Add(ParseMethod(func, programElements, fileName));
            }
        }

        private MethodElement ParseMethod(XElement method, List<ProgramElement> programElements, string fileName, bool isConstructor = false)
        {
            return ParseMethod(method, programElements, fileName, typeof(MethodElement), isConstructor);
        }

        public virtual MethodElement ParseMethod(XElement method, List<ProgramElement> programElements, string fileName, Type myType, bool isConstructor = false)
        {
            string name = String.Empty;
            int definitionLineNumber = 0;
            string returnType = String.Empty;

            SrcMLParsingUtils.ParseNameAndLineNumber(method, out name, out definitionLineNumber);

			AccessLevel accessLevel;
            XElement type = method.Element(SRC.Type);
            if (type != null)
            {
                accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(type);

                XElement typeName = type.Element(SRC.Name);
                if (typeName != null)
                {
                    returnType = typeName.Value;
                }
                else
                {
                    returnType = "void";
                }
            }
            else
            {
                accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(method);
            }

            if (String.IsNullOrEmpty(returnType))
            {
                if (name.Equals("get"))
                {
                    try
                    {
                        var myName =
                            method.Ancestors(SRC.DeclarationStatement).Descendants(SRC.Declaration).
                                Descendants(SRC.Type).Elements(SRC.Name);
                        returnType = myName.First().Value;
                    }
                    catch (NullReferenceException nre)
                    {
                        returnType = "";
                    }
                }
                else if (name.Equals("set"))
                {
                    returnType = "void";
                }
                else if (name.Equals("add") || name.Equals("remove"))
                {
                    try
                    {
                        var myName =
                            method.Parent.Parent.Elements(SRC.Type).First().Elements(SRC.Name).First();
                        returnType = myName.Value;
                    }
                    catch (NullReferenceException nre)
                    {
                        returnType = "";
                    }
                }
            }


            //parse arguments
            XElement paramlist = method.Element(SRC.ParameterList);
            string arguments = String.Empty;
            if (paramlist != null)
            {
                IEnumerable<XElement> argumentElements =
                    from el in paramlist.Descendants(SRC.Name)
                    select el;

                foreach (XElement elem in argumentElements)
                {
                    arguments += elem.Value + " ";
                }
            }
            arguments = arguments.TrimEnd();

            string body = SrcMLParsingUtils.ParseBody(method);

            ClassElement classElement = SrcMLParsingUtils.RetrieveClassElement(method, programElements);
            Guid classId = classElement != null ? classElement.Id : Guid.Empty;
            string className = classElement != null ? classElement.Name : String.Empty;

            string fullFilePath = System.IO.Path.GetFullPath(fileName);

            string source = SrcMLParsingUtils.RetrieveSource(method);

            return Activator.CreateInstance(myType, name, definitionLineNumber, fullFilePath, source, accessLevel, arguments, returnType, body,
                                        classId, className, String.Empty, isConstructor) as MethodElement;
        }


        public static void ParseEnums(List<ProgramElement> programElements, XElement elements, string fileName, int snippetSize)
        {
            IEnumerable<XElement> enums =
                from el in elements.Descendants(SRC.Enum)
                select el;

            foreach (XElement enm in enums)
            {
                AccessLevel accessLevel = SrcMLParsingUtils.RetrieveAccessLevel(enm);

                string name;
                int definitionLineNumber;
                SrcMLParsingUtils.ParseNameAndLineNumber(enm, out name, out definitionLineNumber);

                //parse namespace
                IEnumerable<XElement> ownerNamespaces =
                    from el in enm.Ancestors(SRC.Namespace)
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

                programElements.Add(new EnumElement(name, definitionLineNumber, fullFilePath, source, accessLevel, namespaceName, values));
            }
        }
    }

}
