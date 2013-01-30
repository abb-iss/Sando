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
using System.Diagnostics.Contracts;
using System.Xml;
using System.Text.RegularExpressions;

namespace LocalSearch
{   
    public class GraphBuilder
    {
        private String SrcmlLibSharp = @"..\..\LIBS\srcML-Win-cSharp";
        private const String SrcmlLib = @"..\..\LIBS\srcML-Win";
        private SrcMLFile srcmlFile;
        private XElement[] FieldDecs;
        private XElement[] Methods;
        private Dictionary<int, List<Tuple<XElement, int>>> Calls = new Dictionary<int, List<Tuple<XElement, int>>>();

        /// <summary>
        /// Constructing a new Graph Builder.
        /// </summary>
        /// <param name="srcPath">The full path to the given Source Code or XML file.</param>
        public GraphBuilder(String srcPath, string SrcMLForCSharp = null)
        {
            if (SrcMLForCSharp != null)
            {
                SrcmlLibSharp = SrcMLForCSharp;
            }
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

                String tmpFile = Path.GetTempFileName();
                srcmlFile = srcmlConverter.GenerateSrcMLFromFile(srcPath, tmpFile);
            }
            CreateCallGraph();
        }

        private void CreateCallGraph()
        {
            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                var allCalls = SrcMLHelper.GetCallsFromFunction(method);
                foreach (var call in allCalls)
                {
                    var methodCallAsDeclaration = GetMethod(call);
                    if (methodCallAsDeclaration != null)
                    {
                        List<Tuple<XElement,int>> calls = null;
                        Calls.TryGetValue(method.GetSrcLineNumber(), out calls);
                        if (calls == null)
                            calls = new List<Tuple<XElement, int>>();
                        calls.Add(Tuple.Create(methodCallAsDeclaration,call.GetSrcLineNumber()));
                        Calls[method.GetSrcLineNumber()] = calls;
                    }
                }

                //foreach (var anotherMethod in allmethods)
                //{
                //    if (IfCalledByMe(anotherMethod, allCalls) != -1)
                //    {
                //        List<Tuple<XElement, int>> calls = null;
                //        Calls.TryGetValue(method.GetSrcLineNumber(), out calls);
                //        if (calls == null)
                //            calls = new List<Tuple<XElement, int>>();
                //        calls.Add(anotherMethod);
                //    }
                //}
            }            
        }

        private List<ProgramElementWithRelation> GetCallees(CodeSearchResult codeSearchResult)
        {         
            
            var method = GetMethod(codeSearchResult.Element as MethodElement);
            return GetCallees( method);
        }

        public List<ProgramElementWithRelation> GetCallees( XElement method)
        {
            List<ProgramElementWithRelation> listFiledRelated = new List<ProgramElementWithRelation>();
            List<Tuple<XElement, int>> myCallees = null;
            Calls.TryGetValue(method.GetSrcLineNumber(), out myCallees);
            if (myCallees != null)
            {
                foreach (var callee in myCallees)
                {
                    var fullmethod = callee;
                    Contract.Requires((fullmethod != null), "Method " + method.Element(SRC.Name).Value + " does not belong to this local file.");
                    var methodaselement = GetMethodElementWRelationFromXElement(fullmethod.Item1);
                    methodaselement.ProgramElementRelation = ProgramElementRelation.CallBy;
                    listFiledRelated.Add(methodaselement);
                    methodaselement.RelationLineNumber = fullmethod.Item2;
                }
            }
            return listFiledRelated;
        }

        private XElement GetMethod(XElement call)
        {
            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                if (Matches(call,method))
                    return method;
            }
            return null;
        }

        private XElement GetMethod(MethodElement programElement)
        {
            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                if (programElement.DefinitionLineNumber == method.GetSrcLineNumber())
                    return method;
            }
            return null;
        }

        private int IfCalledByMe(XElement method,  IEnumerable<XElement> allCalls)
        {            
            foreach (var call in allCalls)
            {
                if(Matches(call,method))
                    return call.GetSrcLineNumber();
            }
            return -1;
        }

        private bool Matches(XElement call, XElement methodElement)
        {
            var name = "";
            try
            {
                name = call.Elements(SRC.Name).Elements(SRC.Name).Last().Value;	                
            }catch(InvalidOperationException e){
                name = call.Elements(SRC.Name).Last().Value;
            }
            var argCount = call.Element(SRC.ArgumentList).Elements(SRC.Argument).Count();
            
            var paramList = methodElement.Element(SRC.ParameterList);
            var elementArgCount = 0;
            if(paramList!=null)
                elementArgCount =   paramList.Elements(SRC.Parameter).Count();
            var elementName = methodElement.Element(SRC.Name).Value;
            if (name.Equals(elementName) && elementArgCount == argCount)
                return true;
            else
                return false;
        }
          
        /// <summary>
        /// Get all field "declaration statements" in the source file.
        /// </summary>
        /// <returns>An array of all field declaration statements in XElement.</returns>
        public XElement[] GetFieldDeclStatements()
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

            var fields = GetFieldDeclStatements();

            foreach (XElement field in fields)
            {
                var fieldname = field.Element(SRC.Declaration).Element(SRC.Name);                
                if (fieldname != null)
                    listAllFieldNames.Add(fieldname);
            }

            return listAllFieldNames.ToArray();
        }

        /// <summary>
        /// Get all methods (including constructor and destructor) in the source file.
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
        /// Get names of all methods (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array of all the method NAMES in XElement.</returns>
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
            try
            {
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
            catch (NullReferenceException nre)
            {
                return new XElement[0];
            }
        }
        
        /// <summary>
        /// Determine if a field is used in a given method.
        /// </summary>
        /// <param name="method">FULL method in XElement</param>
        /// <param name="fieldname">field NAME in String</param>
        /// <returns>true if it is used.</returns>
        /// <retruns name="UsedLineNumber"> number of line on which the field is used </retruns>
        public bool ifFieldUsedinMethod(XElement method, String fieldname, ref int UsedLineNumber)
        {
            XElement methodbody = method.Element(SRC.Block);
            if (methodbody == null)
                return false;
            var allnames = methodbody.Descendants(SRC.Name);

            var localvars = GetAllLocalVarsinMethod(method);
            var paras = GetAllParametersinMethod(method);

            List<String> listNames = new List<String>();  
            List<XElement> listNameElements = new List<XElement>();
            foreach (var name in allnames)
            {
                listNameElements.Add(name);
                listNames.Add(name.Value);
            }

            List<String> listLocalVars = new List<string>();
            foreach (var localvar in localvars)
                listLocalVars.Add(localvar.Value);

            List<String> listParas = new List<string>();
            foreach (var para in paras)
                listParas.Add(para.Value);

            if (listNames.Contains(fieldname) &&
                !(listLocalVars.Contains(fieldname)) &&
                !(listParas.Contains(fieldname)))
            {
                //now just search for the first use for now [todo later]
                int index = listNames.FindIndex(name => name == fieldname ); 
                UsedLineNumber = listNameElements[index].GetSrcLineNumber();
                return true;
            }
            else
                return false;
        }

        public bool ifFieldUsedinMethod(XElement method, XElement fieldname, ref int UsedLineNumber)
        {
            return ifFieldUsedinMethod(method, fieldname.Value, ref UsedLineNumber);

        }

        /// <summary>
        /// Get fields that are used in a given method.
        /// </summary>
        /// <param name="method">FULL method in XElement</param>
        /// <returns>an array of fields in ProgramElementWithRelation</returns>
        public ProgramElementWithRelation[] GetFieldElementsUsedinMethod(XElement method)
        {
            List<ProgramElementWithRelation> listFieldElementsUsed = new List<ProgramElementWithRelation>();

            XElement[] allfields = GetFieldNames();
            foreach (var field in allfields)
            {
                int useLineNum = 0;
                if (ifFieldUsedinMethod(method, field, ref useLineNum))
                {
                    var fielddecl = GetFieldDeclFromName(field.Value);
                    Contract.Requires((fielddecl != null), "Field " + field.Value + " does not belong to this local file.");
                    var fieldelement = GetFieldElementWRelationFromDecl(fielddecl);
                    fieldelement.RelationLineNumber = useLineNum;
                    fieldelement.ProgramElementRelation = ProgramElementRelation.UseBy;

                    listFieldElementsUsed.Add(fieldelement);
                }
            }

            return listFieldElementsUsed.ToArray();
        }

        /// <summary>
        /// Get methods that use a given field.
        /// </summary>
        /// <param name="fieldname">field NAME in String</param>
        /// <returns>an array of FULL methods in ProgramElementWithRelation</returns>
        public ProgramElementWithRelation[] GetMethodElementsUseField(String fieldname)
        {
            List<ProgramElementWithRelation> listMethodElements = new List<ProgramElementWithRelation>();

            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                int useLineNum = 0;
                if (ifFieldUsedinMethod(method, fieldname, ref useLineNum))
                {
                    var methodaselement = GetMethodElementWRelationFromXElement(method);
                    methodaselement.ProgramElementRelation = ProgramElementRelation.Use;
                    methodaselement.RelationLineNumber = useLineNum;
                    listMethodElements.Add(methodaselement);
                }
            }

            return listMethodElements.ToArray();
        }

        /// <summary>
        /// Get full method XElement from a given method name.
        /// </summary>
        /// <param name="methodname">method NAME in XElement</param>
        /// <returns>Full method in XElement</returns>
        public XElement GetFullMethodFromName(XElement methodname)
        {
            //var methods = GetMethods();

            //foreach (XElement method in methods)
            //{
            //    if (methodname.Equals(method.Element(SRC.Name)))
            //        return method;
            //}

            //return null;
            int srcLineNum = methodname.GetSrcLineNumber();
            String methodName = methodname.Value;

            return GetFullMethodFromName(methodName, srcLineNum);
        }

        public XElement GetFullMethodFromName(String methodname, int srclinenum)
        {
            var methods = GetMethods();
            foreach (XElement method in methods)
            {
                if ( (methodname.Equals(method.Element(SRC.Name).Value))
                    && (srclinenum == method.Element(SRC.Name).GetSrcLineNumber() ))
                    return method;
            }

            return null; 
        }

        /// <summary>
        /// Get field declaration XElement from a given field name.
        /// </summary>
        /// <param name="fieldname">field NAME in String</param>
        /// <returns>field declaration in XElement</returns>
        public XElement GetFieldDeclFromName(String fieldname)
        {
            var fields = GetFieldDeclStatements();
            foreach (XElement field in fields)
            {
                if (fieldname.Equals(field.Element(SRC.Declaration).Element(SRC.Name).Value))
                    return field.Element(SRC.Declaration);
            }

            return null;
        }
        
        public List<ProgramElementWithRelation> GetRelatedInfo(CodeSearchResult codeSearchResult)
        {            
            ProgramElementType elementtype = codeSearchResult.ProgramElementType;
            if (elementtype.Equals(ProgramElementType.Field))
                return GetFieldRelatedInfo(codeSearchResult);
            else // if(elementtype.Equals(ProgramElementType.Method))
                return GetMethodRelatedInfo(codeSearchResult);
        }

        public List<ProgramElementWithRelation> GetFieldRelatedInfo(CodeSearchResult codeSearchResult)
        {
            List<ProgramElementWithRelation> listFiledRelated 
                = new List<ProgramElementWithRelation>();
            String fieldname = codeSearchResult.Name;

            if ((codeSearchResult as ProgramElementWithRelation) == null || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Use)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Call)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.CallBy)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.UseBy))
            {
                var fieldDeclaration = GetFieldDeclFromName(codeSearchResult.Element.Name);
                listFiledRelated.Add(GetFieldElementWRelationFromDecl(fieldDeclaration));
            }

            //var methodDeclaration = getfie(codeSearchResult.Element as MethodElement);

            //listMethodRelated.Add(GetMethodElementWRelationFromXElement(methodDeclaration));

            //relation 1: get methods that use this field
            listFiledRelated.AddRange(GetMethodElementsUseField(fieldname));

            //there may be other relations that will be considered in the future
            // todo

            return listFiledRelated;

        }      

        public List<ProgramElementWithRelation> GetMethodRelatedInfo(CodeSearchResult codeSearchResult)
        {
            List<ProgramElementWithRelation> listMethodRelated
                = new List<ProgramElementWithRelation>();
            if ((codeSearchResult as ProgramElementWithRelation) == null || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Use)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Call)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.CallBy)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.UseBy))
            {
                var methodDeclaration = GetMethod(codeSearchResult.Element as MethodElement);
                listMethodRelated.Add(GetMethodElementWRelationFromXElement(methodDeclaration));
            }
            String methodname = codeSearchResult.Name;
            int srcLineNumber = codeSearchResult.Element.DefinitionLineNumber;
            var method = GetFullMethodFromName(methodname, srcLineNumber);

            Contract.Requires((method != null), "Method "+ methodname + " does not belong to this local file.");

            //relation 1: get methods that are called by this method
            listMethodRelated.AddRange(GetCallees(codeSearchResult));

            //relation 2: get fields that are used by this method
            listMethodRelated.AddRange(GetFieldElementsUsedinMethod(method));

            //get other method related info. 
            //todo

            return listMethodRelated;
        }

        private ProgramElementWithRelation GetFieldElementWRelationFromDecl(XElement fielddecl)
        {
            var definitionLineNumber = fielddecl.Element(SRC.Name).GetSrcLineNumber();
            var fullFilePath = srcmlFile.FileName;
            var snippet = fielddecl.ToSource();
            var relation = ProgramElementRelation.Other; //by default

            AccessLevel accessLevel = AccessLevel.Internal; //by default
            var specifier = fielddecl.Element(SRC.Type).Element(SRC.Specifier);
            if (specifier != null) 
                accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), specifier.Value,true); 
            
            var fieldType = fielddecl.Element(SRC.Type).Element(SRC.Name);
            var classId = Guid.Empty;
            var className = String.Empty;
            var initialValue = String.Empty;

            var element = new FieldElement(fielddecl.Element(SRC.Name).Value, 
                definitionLineNumber, fullFilePath, snippet, 
                accessLevel, fieldType.Value, classId, className, String.Empty, initialValue);

            var elementwrelation = new ProgramElementWithRelation(element, 1.0, relation);

            return elementwrelation;

            //var element = new FieldElementWithRelation(fielddecl.Element(SRC.Name).Value, 
            //    definitionLineNumber, fullFilePath, snippet, relation,
            //    accessLevel, fieldType.Value, classId, className, String.Empty, initialValue);

            //return element;
        }

        private ProgramElementWithRelation GetMethodElementWRelationFromXElement(XElement fullmethod)
        {
            var definitionLineNumber = fullmethod.Element(SRC.Name).GetSrcLineNumber();
            var fullFilePath = srcmlFile.FileName;
            var snippet = fullmethod.Element(SRC.Block).ToSource(); //todo: only show related lines  
            var relation = ProgramElementRelation.Other; //by default
            
            AccessLevel accessLevel = AccessLevel.Internal; //by default
            var specifier = fullmethod.Element(SRC.Specifier); //for constructor
            try
            {
                if (specifier == null)
                    specifier = fullmethod.Element(SRC.Type).Element(SRC.Specifier); //for other functions
                if (specifier != null)
                    accessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), specifier.Value, true);
            }
            catch (NullReferenceException nre)
            {
                //TODO: handle properties, add, get, etc.
            }
            
            var returnType = String.Empty;
            bool isconstructor = true;
            var type = fullmethod.Element(SRC.Type);
            if (type != null)
            {
                returnType = type.Element(SRC.Name).Value;
                isconstructor = false;
            }

            var classId = Guid.Empty;
            var className = String.Empty;
            var myParams = fullmethod.Element(SRC.ParameterList);
            var args = "";
            if(myParams!=null)
                args = myParams.ToSource();
            var body = fullmethod.Element(SRC.Block).ToSource();

            var element = new MethodElement(fullmethod.Element(SRC.Name).Value,
                definitionLineNumber, fullFilePath, snippet, 
                accessLevel, args, returnType, body, classId, className, String.Empty, isconstructor);
            var elementwrelation = new ProgramElementWithRelation(element, 1.0, relation);

            return elementwrelation;
            
            //var element = new MethodElementWithRelation(fullmethod.Element(SRC.Name).Value, 
            //    definitionLineNumber, fullFilePath, snippet, relation,
            //    accessLevel, args, returnType, body, classId, className, String.Empty, isconstructor);
            //return element;
        }

        #region maybe obsoleted

        /// <summary>
        /// Get methods that uses a given field.
        /// </summary>
        /// <param name="fieldname">field NAME in String</param>
        /// <returns>an array of FULL methods in XElement</returns>
        public XElement[] GetMethodsUseField(String fieldname)
        {
            List<XElement> listMethods = new List<XElement>();

            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                int useLineNum = 0;
                if (ifFieldUsedinMethod(method, fieldname, ref useLineNum))
                {
                    listMethods.Add(method);
                }
            }

            return listMethods.ToArray();
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
                int useLineNum = 0;
                if (ifFieldUsedinMethod(method, field, ref useLineNum))
                    listFieldsUsed.Add(field);
            }

            return listFieldsUsed.ToArray();
        }

        /// <summary>
        /// Get methods that uses a given field.
        /// </summary>
        /// <param name="fieldname">field NAME in String</param>
        /// <returns>an array of method NAMES in XElement</returns>
        public XElement[] GetMethodNamesUseField(String fieldname)
        {
            List<XElement> listMethods = new List<XElement>();

            XElement[] allmethods = GetMethods();
            foreach (var method in allmethods)
            {
                int useLineNum = 0;
                if (ifFieldUsedinMethod(method, fieldname, ref useLineNum))
                {
                    var methodname = method.Element(SRC.Name);
                    listMethods.Add(methodname);
                }
            }

            return listMethods.ToArray();
        }

        public XElement[] GetMethodNamesUseField(XElement fieldname)
        {
            return GetMethodNamesUseField(fieldname.Value);
        }

        #endregion

        #region testzone

        public List<CodeSearchResult> GetRelatedMethods(CodeSearchResult codeSearchResult)
        {
            return this.GetMethodsAsMethodElements();
        }

        public List<CodeSearchResult> GetFieldsAsFieldElements()
        {
            var elements = new List<CodeSearchResult>();

            var fields = GetFieldDeclStatements();

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

        public List<CodeSearchResult> GetMethodsAsMethodElements()
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

        #endregion
    }
}
