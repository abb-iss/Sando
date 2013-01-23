using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ABB.SrcML;
using System.Runtime.CompilerServices;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace LocalSearch
{   
    public class GraphBuilder
    {
        private const String SrcmlLibSharp = @"..\..\..\..\LIBS\srcML-Win-cSharp";
        private const String SrcmlLib = @"..\..\..\..\LIBS\srcML-Win";
        private SrcMLFile srcmlFile;
        private XElement[] FieldDecs;
        private XElement[] Methods;

        /// <summary>
        /// Constructing a new Graph Builder.
        /// </summary>
        /// <param name="srcPath">The full path to the given Source Code or XML file.</param>
        public GraphBuilder(String srcPath)
        {
            Src2SrcMLRunner srcmlConverter;
            String fileExt = Path.GetExtension(srcPath);

            if (fileExt.Equals(".xml", StringComparison.CurrentCultureIgnoreCase) )
                srcmlFile = new SrcMLFile(srcPath);
            else
            {
                if (fileExt.Equals(".cs", StringComparison.CurrentCultureIgnoreCase))
                    srcmlConverter = new Src2SrcMLRunner(SrcmlLibSharp);
                else
                    srcmlConverter = new Src2SrcMLRunner(SrcmlLib);

                String tmpFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml";
                srcmlFile = srcmlConverter.GenerateSrcMLFromFile(srcPath, tmpFile);
            }
        }

        /// <summary>
        /// Get all field declaration statements in the source file.
        /// </summary>
        /// <returns>An array of all field declaration statements in XElement.</returns>
        public XElement[] GetFieldDecs()
        {
            List<XElement> listAllFields = new List<XElement>();
            
            foreach (XElement file in srcmlFile.FileUnits)
            {
                var fields = from field in file.Descendants()
                                where field.Name.Equals(SRC.DeclarationStatement)
                                && !field.Ancestors(SRC.Function).Any()
                                && !field.Ancestors(SRC.Constructor).Any()
                                && !field.Ancestors(SRC.Destructor).Any()
                                select field;
                
                if (fields != null)
                    listAllFields.AddRange(fields);
            }

            //FieldDecs = AllFields.ToArray();
            return listAllFields.ToArray();
        }

        /// <summary>
        /// Get all field names declared in the source file.
        /// </summary>
        /// <returns>An array of all field NAMES in XElement.</returns>
        public XElement[] GetFieldNames()
        {   
            List<XElement> listAllFieldNames = new List<XElement>();

            var fields = GetFieldDecs();

            foreach (XElement field in fields)
            {
                var fieldname = field.Element(SRC.Declaration).Element(SRC.Name);                
                if (fieldname != null)
                    listAllFieldNames.Add(fieldname);
            }

            return listAllFieldNames.ToArray();
        }

        /// <summary>
        /// Get all functions (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array of all the FULL methods/constructors/destructors in XElement.</returns>
        public XElement[] GetMethods()
        {
            List<XElement> listAllmethods = new List<XElement>();

            foreach (XElement file in srcmlFile.FileUnits)
            {
                var methods = from method in file.Descendants()
                              where method.Name.Equals(SRC.Function)
                             || method.Name.Equals(SRC.Constructor)
                             || method.Name.Equals(SRC.Destructor)
                             select method;

                if (methods != null)
                    listAllmethods.AddRange(methods);
            }

            //Methods = listAllmethods.ToArray();
            return listAllmethods.ToArray();
        }

        /// <summary>
        /// Get names of all functions (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array of all the function NAMES in XElement.</returns>
        public XElement[] GetMethodNames()
        {
            List<XElement> listAllMethodNames = new List<XElement>();

            var methods = GetMethods();

            foreach (XElement method in methods)
            {
                var methodname = method.Element(SRC.Name);
                if (methodname != null)
                    listAllMethodNames.Add(methodname);
            }

            return listAllMethodNames.ToArray();
        }

        /// <summary>
        /// Get all local variables declared in a given method XElement.
        /// </summary>
        /// <param name="method">A FULL method in XElement format.</param>
        public XElement[] GetAllLocalVarsinMethod(XElement method)
        {
            XElement methodbody = method.Element(SRC.Block);
            var localdeclares =
                methodbody.Descendants(SRC.DeclarationStatement);
               
            List<XElement> listLocalVars = new List<XElement>();
            
            foreach (var localdeclare in localdeclares)
            {
                XElement localvar = localdeclare.Element(SRC.Declaration).Element(SRC.Name);
                listLocalVars.Add(localvar);
            }
            
            return listLocalVars.ToArray();
        }

        /// <summary>
        /// Get all parameters for a given method XElement.
        /// </summary>
        /// <param name="method">A FULL method in XElement format.</param>
        public XElement[] GetAllParametersinMethod(XElement method)
        {
            XElement paralist = method.Element(SRC.ParameterList);
            var parameters = paralist.Elements(SRC.Parameter);

            List<XElement> listParas = new List<XElement>();

            foreach (var parameter in parameters)
            {
                var paradec = parameter.Element(SRC.Declaration);
                var para = paradec.Element(SRC.Name);
                listParas.Add(para);
            }

            return listParas.ToArray();
        }

        //public XElement[] GetAllVarsUseinMethod(XElement method)
        //{
        //    XElement methodbody = method.Element(SRC.Block);
            
        //    var allvars1 = from variable in methodbody.Descendants()
        //                   where variable.Name.Equals(SRC.Name)
        //                   && !variable.Ancestors(SRC.Type).Any()
        //                   && !variable.Parent.Name.Equals(SRC.Call)
        //                   select variable;

        //    var allvars2 = from variable in methodbody.Descendants()
        //                   where variable.Parent.Name.Equals(SRC.Call)
        //                   where variable.Name.Equals(SRC.Name)
        //                   select variable;

        //    XElement allvars3;

        //    foreach (var x in allvars2)
        //    {
        //        if (x.Elements(SRC.Name).Count() >= 2)
        //            allvars3 = x.Element(SRC.Name);
        //    }

        //    List<XElement> nonlocalvars = new List<XElement>();
        //    foreach (var variable in allvars) //allvars = allvars1 + allvars3
        //    {
        //        if (!(localvars.Contains(variable.Value)))
        //            nonlocalvars.Add(variable);
        //    }

        //    return nonlocalvars.ToArray();


        //}

        
        /// <summary>
        /// Determine if a field is used in a given method.
        /// </summary>
        /// <param name="method">FULL method in XElement</param>
        /// <param name="field">field NAME in String</param>
        /// <returns>true if it is used.</returns>
        public bool ifFieldUsedinMethod(XElement method, String field)
        {
            XElement methodbody = method.Element(SRC.Block);
            var allnames = methodbody.Descendants(SRC.Name);

            var localvars = GetAllLocalVarsinMethod(method);
            var paras = GetAllParametersinMethod(method);

            List<String> listNames = new List<String>();
            foreach (var name in allnames)
                listNames.Add(name.Value);

            List<String> listLocalVars = new List<string>();
            foreach (var localvar in localvars)
                listLocalVars.Add(localvar.Value);

            List<String> listParas = new List<string>();
            foreach (var para in paras)
                listParas.Add(para.Value);

            String filedname = field;

            if (listNames.Contains(filedname) &&
                !(listLocalVars.Contains(filedname)) &&
                !(listParas.Contains(filedname)))
                return true;
            else
                return false;
        }

        public bool ifFieldUsedinMethod(XElement method, XElement field)
        {
            return ifFieldUsedinMethod(method, field.Value);

        }

        /// <summary>
        /// Get fields used in a given method.
        /// </summary>
        /// <param name="method">FULL method in XElement</param>
        /// <returns>an array of field NAMES in XElement</returns>
        public XElement[] GetFieldsUsedinMethod(XElement method)
        {
            List<XElement> listFieldsUsed = new List<XElement>();

            XElement[] allfields = GetFieldNames();
            foreach (var field in allfields)
            {
                if (ifFieldUsedinMethod(method, field))
                    listFieldsUsed.Add(field);
            }

            return listFieldsUsed.ToArray();
        }

        /// <summary>
        /// Get methods that uses a given field.
        /// </summary>
        /// <param name="field">field name in String</param>
        /// <returns>an array of method NAMES in XElement</returns>
        public XElement[] GetMethodsUseField(String field)
        {
            List<XElement> listMethods = new List<XElement>();

            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                if (ifFieldUsedinMethod(method, field))
                {
                    var methodname = method.Element(SRC.Name);
                    listMethods.Add(methodname);
                }
            }

            return listMethods.ToArray();
        }

        public List<CodeSearchResult> GetRelatedMethods(CodeSearchResult codeSearchResult)
        {
            return this.GetMethodsAsMethodElements();
        }

        public List<CodeSearchResult> GetFieldsAsFieldElements()
        {
            var elements = new List<CodeSearchResult>();

            var fields = GetFieldDecs();

            foreach (XElement field in fields)
            {
                var fieldname = field.Element(SRC.Declaration).Element(SRC.Name);
                var definitionLineNumber = fieldname.GetSrcLineNumber();
                var fullFilePath = srcmlFile.FileName;
                var snippet = String.Empty;
                var accessLevel = AccessLevel.Public;
                var fieldType = String.Empty;
                var classId = Guid.Empty;
                var className = String.Empty;
                var initialValue = String.Empty;
                var element = new FieldElement(fieldname.Value, definitionLineNumber, fullFilePath, snippet, accessLevel, fieldType, classId, className, String.Empty, initialValue);
                CodeSearchResult result = new CodeSearchResult(element, 1.0);
                elements.Add(result);
            }

            return elements;
        }


        private List<CodeSearchResult> GetMethodsAsMethodElements()
        {
            var elements = new List<CodeSearchResult>();

            var methods = GetMethodNames();

            foreach (XElement method in methods)
            {
                var fieldname = "";
                if (method.Element(SRC.Name) != null)
                {
                    fieldname = method.Element(SRC.Name).Value;
                }
                else
                {
                    fieldname = method.Value;
                }
                var definitionLineNumber = method.GetSrcLineNumber();
                var fullFilePath = srcmlFile.FileName;
                var snippet = String.Empty;
                var accessLevel = AccessLevel.Public;
                var fieldType = String.Empty;
                var classId = Guid.Empty;
                var className = String.Empty;
                var initialValue = String.Empty;
                bool isconstructor = false;
                var args = String.Empty;
                var body = string.Empty;
                var element = new MethodElement(fieldname, definitionLineNumber, fullFilePath, snippet, accessLevel, args, fieldType, body, classId, className, String.Empty, isconstructor);
                CodeSearchResult result = new CodeSearchResult(element, 1.0);
                elements.Add(result);
            }

            return elements;
        }
    }
}
