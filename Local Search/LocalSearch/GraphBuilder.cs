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

namespace Sando.LocalSearch
{   
    public class GraphBuilder
    {
        private String SrcmlLibSharp = @"..\..\LIBS\srcML-Win-cSharp";
        private const String SrcmlLib = @"..\..\LIBS\srcML-Win";
        private SrcMLFile srcmlFile;
        protected XElement[] FieldDecs;
        protected XElement[] FullMethods;
        protected String[] ClassNames;
        protected Dictionary<int, List<Tuple<XElement, int>>> Calls = new Dictionary<int, List<Tuple<XElement, int>>>();
        protected Dictionary<int, List<Tuple<XElement, int>>> Callers = new Dictionary<int, List<Tuple<XElement, int>>>();
        protected Dictionary<int, List<Tuple<XElement, int>>> FieldUses = new Dictionary<int, List<Tuple<XElement, int>>>();
        protected Dictionary<int, List<Tuple<XElement, int>>> FieldUsers = new Dictionary<int, List<Tuple<XElement, int>>>();
        private string origPath;

        /// <summary>
        /// Constructing a new Graph Builder.
        /// </summary>
        /// <param name="srcPath">The full path to the given Source Code or XML file.</param>
        public GraphBuilder(String srcPath, string SrcMLForCSharp = null)
        {
            origPath = srcPath;
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

        }

        public void Initialize()
        {
            ClassNames = GetClassNames(); 
            
            FullMethods = GetFullMethods();

            FieldDecs = GetFieldDecs();

            CreateCallGraph();

            CreateFieldUseGraph();
        }


        #region callgraph related
        private void CreateCallGraph()
        {
            XElement[] allmethods = FullMethods;
            
            foreach (var method in allmethods)
            {
                var allCalls = SrcMLHelper.GetCallsFromFunction(method);
                foreach (var call in allCalls)
                {
                    var methodCallAsDeclaration = GetMethodFromCall(call);
                    if (methodCallAsDeclaration != null)
                    {
                        List<Tuple<XElement,int>> calls = null;
                        Calls.TryGetValue(method.GetSrcLineNumber(), out calls);
                        if (calls == null)
                            calls = new List<Tuple<XElement, int>>();
                        calls.Add(Tuple.Create(methodCallAsDeclaration,call.GetSrcLineNumber()));
                        Calls[method.GetSrcLineNumber()] = calls;

                        List<Tuple<XElement, int>> callers = null;
                        Callers.TryGetValue(methodCallAsDeclaration.GetSrcLineNumber(), out callers);
                        if(callers == null)
                            callers = new List<Tuple<XElement, int>>();
                        callers.Add(Tuple.Create(method, call.GetSrcLineNumber()));
                        Callers[methodCallAsDeclaration.GetSrcLineNumber()] = callers;
                    }
                }
               
            }            
        }

        public List<CodeNavigationResult> GetCallees(CodeSearchResult codeSearchResult)
        {

            var method = GetMethod(codeSearchResult.ProgramElement as MethodElement);
            return GetCallees( method);
        }

        public List<CodeNavigationResult> GetCallers(CodeSearchResult codeSearchResult)
        {

            var method = GetMethod(codeSearchResult.ProgramElement as MethodElement);
            return GetCallers(method);
        }

        private List<CodeNavigationResult> GetCallees(XElement method)
        {
            List<CodeNavigationResult> listCallees = new List<CodeNavigationResult>();
            List<Tuple<XElement, int>> myCallees = null;
            Calls.TryGetValue(method.GetSrcLineNumber(), out myCallees);
            if (myCallees != null)
            {
                foreach (var callee in myCallees)
                {
                    var methodaselement = XElementToProgramElementConverter.GetMethodElementWRelationFromXElement(callee.Item1, origPath);
                    methodaselement.ProgramElementRelation = ProgramElementRelation.CallBy;
                    methodaselement.RelationLineNumber[0] = callee.Item2;
                    methodaselement.RelationCode = GetXElementFromLineNum(callee.Item2);
                    listCallees.Add(methodaselement);                    
                }
            }
            return listCallees;
        }

        private List<CodeNavigationResult> GetCallers(XElement method)
        {
            List<CodeNavigationResult> listCallers = new List<CodeNavigationResult>();
            List<Tuple<XElement, int>> myCallers = null;
            Callers.TryGetValue(method.GetSrcLineNumber(), out myCallers);
            if (myCallers != null)
            {
                foreach (var caller in myCallers)
                {
                    var methodaselement = XElementToProgramElementConverter.GetMethodElementWRelationFromXElement(caller.Item1, origPath);
                    methodaselement.ProgramElementRelation = ProgramElementRelation.Call;
                    methodaselement.RelationLineNumber[0] = caller.Item2;
                    methodaselement.RelationCode = GetXElementFromLineNum(caller.Item2);
                    listCallers.Add(methodaselement);                    
                }
            }
            return listCallers;
        }

        //find a method call's definition if it is defined in the same file
        public XElement GetMethodFromCall(XElement call)
        {
            XElement[] allmethods = FullMethods;

            foreach (var method in allmethods)
            {
                if (Matches(call,method))
                    return method;
            }
            return null;
        }

        private bool Matches(XElement call, XElement methodElement)
        {
            //first make sure the call and the method is in the same class
            var callclasses = call.Ancestors(SRC.Class);
            var methodclasses = methodElement.Ancestors(SRC.Class);
            String callhappenclassname = "";
            String methodclassname = "";
            try
            {
                callhappenclassname = callclasses.First().Element(SRC.Name).Value;
            }
            //catch (NullReferenceException e)
            catch (Exception e)
            {
                //anonymous class
                callhappenclassname = "anonymous";
            }
            try
            {
                methodclassname = methodclasses.First().Element(SRC.Name).Value;
            }
            catch (Exception e)
            {
                //anonymous class
                methodclassname = "anonymous";
            }

            var name = "";

            var names = call.Element(SRC.Name).Elements(SRC.Name);
            if (names.Count() != 0)
            {
                name = names.Last().Value;                
                int count = names.Count();
                if (count > 1) 
                {
                    var dec = names.ElementAt(count - 2).Value;
                    var obj = names.ElementAt(0).Value;
                    String realcallclassname = "";

                    if (obj.Equals("this"))
                        realcallclassname = callhappenclassname;
                    else
                        realcallclassname = GetClassName(obj);

                    if(realcallclassname != methodclassname)    
                        return false;
                }
            }
            else
            {
                name = call.Element(SRC.Name).Value;
            }

            var argCount = call.Element(SRC.ArgumentList).Elements(SRC.Argument).Count();

            var paramList = methodElement.Element(SRC.ParameterList);
            var elementArgCount = 0;
            var optParaCount = 0;
            if (paramList != null)
            {
                elementArgCount = paramList.Elements(SRC.Parameter).Count();
                var parameters = paramList.Elements(SRC.Parameter);
                foreach (var para in parameters)
                {
                    if (para.Element(SRC.Declaration).Element(SRC.Init) != null)
                        optParaCount++;
                }
            }

            var elementName = methodElement.Element(SRC.Name).Value;
            if (name.Equals(elementName)
                && argCount <= elementArgCount
                && argCount >= (elementArgCount - optParaCount))
                return true;
            else
                return false;
        }

        //if an object is defined in the local file 
        //1. an instance of a locally defined class
        //2. a class
        bool isDefinedLocally(String obj)
        {   
            if (ClassNames.Contains(obj))
                return true;

            foreach (var field in FieldDecs)
            {
                String type = field.Element(SRC.Type).Element(SRC.Name).Value;
                String name = field.Element(SRC.Name).Value;

                if (name.Equals(obj))
                {
                    if(ClassNames.Contains(type))
                        return true;
                }                   
            }

            return false;
        }

        String GetClassName(String obj)
        {
            if (ClassNames.Contains(obj))
                return obj;

            foreach (var field in FieldDecs)
            {
                String type = field.Element(SRC.Type).Element(SRC.Name).Value;
                String name = field.Element(SRC.Name).Value;

                if (name.Equals(obj))
                {
                    return type;
                }
            }

            return "";

        }

        //return MethodElement as its definition in full method XElement
        public XElement GetMethod(MethodElement programElement)
        {
            //XElement[] allmethods = GetFullMethods();
            XElement[] allmethods = FullMethods;
            
            foreach (var method in allmethods)
            {
                //line number is checked, no need to worry about different class issue
                if ((programElement.DefinitionLineNumber == method.GetSrcLineNumber())
                    && (programElement.Name == method.Element(SRC.Name).Value))
                    return method;
            }
            return null;
        }
             
        #endregion callgraph related

        #region field use related
        private void CreateFieldUseGraph() 
        {
            foreach (var field in FieldDecs)
            {
                foreach (var method in FullMethods)
                {
                    List<int> linenumbers = new List<int>();
                    var fieldclasses = field.Ancestors(SRC.Class);
                    String fieldclassname = fieldclasses.First().Element(SRC.Name).Value;
                    bool used = ifFieldUsedinMethod(method, field.Element(SRC.Name).Value, fieldclassname, ref linenumbers);
                    
                    if (used) //linenumbers.Count() > 0
                    {   
                        List<Tuple<XElement, int>> fielduses = null;
                        FieldUses.TryGetValue(method.GetSrcLineNumber(), out fielduses);
                        if (fielduses == null)
                            fielduses = new List<Tuple<XElement, int>>();
                        foreach(var line in linenumbers)
                            fielduses.Add(Tuple.Create(field, line));
                        FieldUses[method.GetSrcLineNumber()] = fielduses;

                        List<Tuple<XElement, int>> fieldusers = null;
                        FieldUsers.TryGetValue(field.GetSrcLineNumber(), out fieldusers);
                        if (fieldusers == null)
                            fieldusers = new List<Tuple<XElement, int>>();
                        foreach(var line in linenumbers)
                            fieldusers.Add(Tuple.Create(method, line));
                        FieldUsers[field.GetSrcLineNumber()] = fieldusers;
                    }
                }
            }

        }

        public List<CodeNavigationResult> GetFieldUses(CodeSearchResult codeSearchResult)
        {
            var method = GetMethod(codeSearchResult.ProgramElement as MethodElement);
            return GetFieldUses(method);
        }

        private List<CodeNavigationResult> GetFieldUses(XElement method)
        {
            List<CodeNavigationResult> listUses = new List<CodeNavigationResult>();
            List<Tuple<XElement, int>> myUses = null;
            FieldUses.TryGetValue(method.GetSrcLineNumber(), out myUses);
            if (myUses != null)
            {
                foreach (var use in myUses)
                {
                    var fieldaselement = XElementToProgramElementConverter.GetFieldElementWRelationFromDecl(use.Item1, origPath);
                    fieldaselement.ProgramElementRelation = ProgramElementRelation.UseBy;
                    fieldaselement.RelationLineNumber[0] = use.Item2;
                    fieldaselement.RelationCode = GetXElementFromLineNum(use.Item2);
                    listUses.Add(fieldaselement);
                }
            }
            return listUses;
        }

        public List<CodeNavigationResult> GetFieldUsers(CodeSearchResult codeSearchResult)
        {
            var field = GetField(codeSearchResult.ProgramElement as FieldElement);
            return GetFieldUsers(field);
        }        

        private List<CodeNavigationResult> GetFieldUsers(XElement field)
        {
            List<CodeNavigationResult> listUsers = new List<CodeNavigationResult>();
            List<Tuple<XElement, int>> myUsers = null;
            FieldUsers.TryGetValue(field.GetSrcLineNumber(), out myUsers);
            if (myUsers != null)
            {
                foreach (var user in myUsers)
                {
                    var methodaselement = XElementToProgramElementConverter.GetMethodElementWRelationFromXElement(user.Item1,origPath);
                    methodaselement.ProgramElementRelation = ProgramElementRelation.Use;
                    methodaselement.RelationLineNumber[0] = user.Item2;
                    methodaselement.RelationCode = GetXElementFromLineNum(user.Item2);
                    listUsers.Add(methodaselement);
                }
            }
            return listUsers;
        }

        //return FieldElement as its declaration in XElement
        public XElement GetField(FieldElement programElement)
        {
            foreach (var field in FieldDecs)
            {
                //by checking the line number, it avoids referring to a different class
                if ((programElement.DefinitionLineNumber == field.GetSrcLineNumber())
                    && (programElement.Name == field.Element(SRC.Name).Value))
                    return field;
            }
            return null;
        }

        #endregion field use related


        #region basic information collection

        public List<CodeSearchResult> GetFieldsAsFieldElements()
        {
            var elements = new List<CodeSearchResult>();

            //var fields = GetFieldDecs();
            var fields = FieldDecs;

            foreach (XElement field in fields)
            {
                var fieldaselement = XElementToProgramElementConverter.GetFieldElementWRelationFromDecl(field, origPath);

                CodeSearchResult result = fieldaselement as CodeSearchResult;
                elements.Add(result);
            }

            return elements;
        }

        public List<CodeSearchResult> GetMethodsAsMethodElements()
        {
            var elements = new List<CodeSearchResult>();

            //var methods = GetMethodNames();
            var fullmethods = FullMethods;

            //foreach (XElement method in methods)
            foreach(XElement fullmethod in fullmethods)
            {
                //var fullmethod = GetFullMethodFromName(method);
                var methodaselement = XElementToProgramElementConverter.GetMethodElementWRelationFromXElement(fullmethod, origPath);
                CodeSearchResult result = methodaselement as CodeSearchResult;
                elements.Add(result);
            }

            return elements;
        }

        /// <summary>
        /// Get full method XElement from a given method name.
        /// </summary>
        /// <param name="methodname">method NAME in XElement</param>
        /// <returns>Full method in XElement</returns>
        public XElement GetFullMethodFromName(XElement methodname)
        {
            int srcLineNum = methodname.GetSrcLineNumber();
            String methodName = methodname.Value;

            return GetFullMethodFromName(methodName, srcLineNum);
        }

        public XElement GetFullMethodFromName(String methodname, int srclinenum)
        {
            //var methods = GetFullMethods();
            var methods = FullMethods;
            foreach (XElement method in methods)
            {
                if ((methodname.Equals(method.Element(SRC.Name).Value))
                    && (srclinenum == method.Element(SRC.Name).GetSrcLineNumber()))
                    return method;
            }

            return null;
        }

        /// <summary>
        /// Get corresponding XElement given the code line number in the source file.
        /// </summary>
        /// <param name="lineNum">The code line number.</param>
        /// <returns>An XElement.</returns>
        public XElement GetXElementFromLineNum(int lineNum)
        {
            foreach (XElement file in srcmlFile.FileUnits)
            {
                foreach (var element in file.Descendants())
                {
                    if (element.GetSrcLineNumber() == lineNum)
                        return element;
                }
            }

            return null;
                            
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

            return listAllFields.ToArray();
        }

        /// <summary>
        /// Get all field declarations in the source file.
        /// </summary>
        /// <returns>An array of all field declarations in XElement.</returns>
        public XElement[] GetFieldDecs()
        {
            List<XElement> listAllFieldDecs = new List<XElement>();

            var fields = GetFieldDeclStatements();

            foreach (XElement field in fields)
            {
                var fielddec = field.Element(SRC.Declaration);
                if (fielddec != null)
                    listAllFieldDecs.Add(fielddec);
            }

            return listAllFieldDecs.ToArray();
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
        /// Get all class names in the source file.
        /// </summary>
        /// <returns>An array of all the class names in String.</returns>
        public String[] GetClassNames()
        {
            //List<String> listClassNames = new List<string>();
            List<XElement> listClassNames = new List<XElement>();
            List<String> nameStrings = new List<string>();

            foreach (XElement file in srcmlFile.FileUnits)
            {
                var classes = from classdef in file.Descendants()
                              where classdef.Name.Equals(SRC.Class)
                              select classdef.Element(SRC.Name);

                if (classes != null)
                    listClassNames.AddRange(classes);

                foreach (var classname in listClassNames)
                {
                    if (classname == null)
                        continue;
                    String classnameStr = classname.Value;
                    nameStrings.Add(classnameStr);
                }
            }            

            return nameStrings.ToArray();
        }

        /// <summary>
        /// Get all methods (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array of all the FULL methods/constructors/destructors in XElement.</returns>
        public XElement[] GetFullMethods()
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

            return listAllmethods.ToArray();
        }

        /// <summary>
        /// Get names of all methods (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array of all the method NAMES in XElement.</returns>
        public XElement[] GetMethodNames()
        {
            List<XElement> listAllMethodNames = new List<XElement>();

            var methods = GetFullMethods();

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
        public bool ifFieldUsedinMethod(XElement method, String fieldname, String fieldclassname, ref List<int> UsedLineNumber)
        {
            var methodclass = method.Ancestors(SRC.Class);
            String methodclassname = "";
            try
            {
                methodclassname = methodclass.First().Element(SRC.Name).Value;
            }
            catch
            {
                methodclassname = "anonymous";
            }

            XElement methodbody = method.Element(SRC.Block);
            if (methodbody == null)
                return false;

             //get all "Names" that appear in the method body
             //except: Name of Type
            var allnames = from name in methodbody.Descendants(SRC.Name)
                            where !name.Ancestors(SRC.Type).Any()
                            select name;

            if (allnames.Count() == 0) 
                 return false;

            bool res = false;
            
            var localvars = GetAllLocalVarsinMethod(method);
            var paras = GetAllParametersinMethod(method);

            List<String> NameStrings = new List<String>();
            List<XElement> NameElements = new List<XElement>();
            List<XElement> OrigExprElements = new List<XElement>();
            List<XElement> ExprNameElements = new List<XElement>();

            foreach (var name in allnames)
            {
                if (!name.Parent.Name.Equals(SRC.Expression)) //single name element
                {
                    NameElements.Add(name);
                    NameStrings.Add(name.Value);
                }
                else //name elements wrapped in expression 
                {
                    var expr = name.Parent;
                    if (!OrigExprElements.Contains(expr))
                    {
                        OrigExprElements.Add(expr);

                        var separateNameExprs = GetSeparateNamesInExpression(expr);
                        foreach (var nameExpr in separateNameExprs)
                        {
                            String fullname = nameExpr.Value;

                            //single name element wrapped in expression
                            // or case of field.xxx
                            NameElements.Add(nameExpr.Element(SRC.Name));
                            NameStrings.Add(nameExpr.Element(SRC.Name).Value);

                            if (fullname.IndexOf('.') > 0)
                                ExprNameElements.Add(nameExpr);                            
                        }
                    }
                }
            }
            
            List<String> listLocalVars = new List<string>();
            foreach (var localvar in localvars)
                listLocalVars.Add(localvar.Value);

            List<String> listParas = new List<string>();
            foreach (var para in paras)
                listParas.Add(para.Value);            
            
            //handle single names
            if (NameStrings.Contains(fieldname) 
            && ! ( listLocalVars.Contains(fieldname) || listParas.Contains(fieldname))) 
            {
                int index = 0;
                while (index < NameStrings.Count())
                {
                    index = NameStrings.FindIndex(index, name => name == fieldname);
                    if (index == -1)
                        break;
                    int line = NameElements[index].GetSrcLineNumber();
                    if (line == -1)
                        line = Convert.ToInt32(NameElements[index].Attribute(POS.Line).Value);
                    if (!UsedLineNumber.Contains(line)) //avoid adding multiple uses on the same line
                        UsedLineNumber.Add(line);
                    index++;
                }
                res = true;                
            }

            //handle names wrapped in expressions
            foreach (var expr in ExprNameElements)
            {
                var separatenames = expr.Elements(SRC.Name);

                if (separatenames.Count() == 1) //TODO: shouldn't happen
                    continue;

                String type = separatenames.ElementAt(0).Value;
                String rootname = separatenames.ElementAt(separatenames.Count() - 1).Value;
                //String dec = separatenames.ElementAt(separatenames.Count() - 2).Value;

                String fielduseclassname = "";

                if (rootname == fieldname)
                {
                    if (type == "this")
                        fielduseclassname = methodclassname;
                    else
                        fielduseclassname = GetClassName(type);

                    if (fielduseclassname == fieldclassname)
                    {
                        XElement rootelement = separatenames.ElementAt(separatenames.Count() - 1);
                        int line = Convert.ToInt32(rootelement.Attribute(POS.Line).Value); //rootelement.GetSrcLineNumber();
                        if (!UsedLineNumber.Contains(line))
                            UsedLineNumber.Add(line);

                        res = true;
                    }
                }
            }

            return res;
        }

        //pull out full names as "x (or x.y.z)" from the whole expression
        XElement[] GetSeparateNamesInExpression(XElement expr)
        {
            List<XElement> separateNames = new List<XElement>();

            //var allelements = expr.Elements();
            var allelements = from element in expr.Descendants()
                              where !element.Elements().Any()
                              select element;

            List<XElement> singleNames = new List<XElement>();
            int flag = 0;
            foreach (var eachelement in allelements)
            {
                if (eachelement.Name.Equals(SRC.Name) && (flag == 0))
                {
                    singleNames.Add(eachelement);
                    flag = 1;
                    continue;
                }

                if (eachelement.Name.Equals(OP.Operator) 
                    && (eachelement.Value == ".") && (flag == 1))
                {
                    singleNames.Add(eachelement);
                    flag = 0;
                    continue;
                }

                XElement mergeName = MergeSeparateNames(singleNames);
                if(mergeName != null)
                    separateNames.Add(mergeName);
                singleNames.Clear();
                flag = 0; //reset flag
            }

            XElement lastmergeName = MergeSeparateNames(singleNames);
            if (lastmergeName != null)
                separateNames.Add(lastmergeName);

            return separateNames.ToArray();
        }

        XElement MergeSeparateNames(List<XElement> separatenames)
        {
            if (separatenames.Count() == 0)
                return null;

            XElement mergedName = new XElement(SRC.Expression);

            mergedName.Add(separatenames);

            return mergedName;            
        }

        #endregion basic information collection


        #region core functionality
        
        public ProgramElementRelation GetRelation(CodeSearchResult element1, CodeSearchResult element2, ref List<int> UsedLineNumber)
        {
            ProgramElementRelation relation = ProgramElementRelation.No;
            ProgramElementType eletype1 = element1.ProgramElementType;
            ProgramElementType eletype2 = element2.ProgramElementType;

            if (eletype1.Equals(ProgramElementType.Field))
            {
                if (eletype2.Equals(ProgramElementType.Method))
                {
                    var methodDeclaration = GetMethod(element2.ProgramElement as MethodElement);
                    if (ifFieldUsedinMethod(methodDeclaration, element1.Name, element1.Parent, ref UsedLineNumber))
                    {
                        relation = ProgramElementRelation.UseBy;
                        return relation;
                    }
                }
            }
            else // eletype1.Equals(ProgramElementType.Method)
            {
                if (eletype2.Equals(ProgramElementType.Method))
                {
                    var methodDec1 = GetMethod(element1.ProgramElement as MethodElement);
                    var methodDec2 = GetMethod(element2.ProgramElement as MethodElement);

                    List<Tuple<XElement, int>> myCallers = null;
                    List<Tuple<XElement, int>> myCallees = null;
                    Callers.TryGetValue(methodDec1.GetSrcLineNumber(), out myCallers);
                    Calls.TryGetValue(methodDec1.GetSrcLineNumber(), out myCallees);

                    if (myCallers != null)
                    {
                        foreach (var caller in myCallers)
                        {
                            if ((caller.Item1.GetSrcLineNumber() == methodDec2.GetSrcLineNumber())
                            && (caller.Item1.Element(SRC.Name).Value == methodDec2.Element(SRC.Name).Value))
                            {
                                relation = ProgramElementRelation.CallBy;
                                UsedLineNumber.Add(caller.Item2);
                            }
                        }
                        return relation;
                    }

                    if (myCallees != null)
                    {
                        foreach (var callee in myCallees)
                        {
                            if ((callee.Item1.GetSrcLineNumber() == methodDec2.GetSrcLineNumber())
                            && (callee.Item1.Element(SRC.Name).Value == methodDec2.Element(SRC.Name).Value))
                            {
                                relation = ProgramElementRelation.Call;
                                UsedLineNumber.Add(callee.Item2);
                            }
                        }

                        return relation;
                    }

                }
                else //eletype2.Equals(ProgramElementType.Field)
                {
                    var methodDeclaration = GetMethod(element1.ProgramElement as MethodElement);
                    if (ifFieldUsedinMethod(methodDeclaration, element2.Name, element2.Parent, ref UsedLineNumber))
                    {
                        relation = ProgramElementRelation.Use;
                        return relation;
                    }
                }
            }

            return relation;

        }
        
        #endregion core functionality


        #region maybe obsoleted
        
        ///// <summary>
        ///// Get field declaration XElement from a given field name.
        ///// </summary>
        ///// <param name="fieldname">field NAME in String</param>
        ///// <returns>field declaration in XElement</returns>
        //public XElement GetFieldDeclFromName(String fieldname)
        //{
        //    //var fields = GetFieldDecs();
        //    var fields = FieldDecs;

        //    foreach (XElement field in fields)
        //    {
        //        if (fieldname.Equals(field.Element(SRC.Name).Value))
        //            return field;
        //    }

        //    return null;
        //}

        ///// <summary>
        ///// Get fields that are used in a given method.
        ///// </summary>
        ///// <param name="method">FULL method in XElement</param>
        ///// <returns>an array of fields in ProgramElementWithRelation</returns>
        //public CodeNavigationResult[] GetFieldElementsUsedinMethod(XElement method)
        //{
        //    List<CodeNavigationResult> listFieldElementsUsed = new List<CodeNavigationResult>();

        //    XElement[] allfields = GetFieldNames();
        //    foreach (var field in allfields)
        //    {
        //        List<int> useLineNum = new List<int>();
        //        if (ifFieldUsedinMethod(method, field.Value, ref useLineNum))
        //        {
        //            var fielddecl = GetFieldDeclFromName(field.Value);
        //            Contract.Requires((fielddecl != null), "Field " + field.Value + " does not belong to this local file.");

        //            foreach (var linenumber in useLineNum)
        //            {
        //                var fieldelement = XElementToProgramElementConverter.GetFieldElementWRelationFromDecl(fielddecl,origPath);
        //                fieldelement.RelationLineNumber.Clear();
        //                fieldelement.RelationLineNumber.Add(linenumber);
        //                fieldelement.ProgramElementRelation = ProgramElementRelation.UseBy;
        //                listFieldElementsUsed.Add(fieldelement);
        //            }
        //        }
        //    }

        //    return listFieldElementsUsed.ToArray();
        //}

        ///// <summary>
        ///// Get methods that use a given field.
        ///// </summary>
        ///// <param name="fieldname">field NAME in String</param>
        ///// <returns>an array of FULL methods in ProgramElementWithRelation</returns>
        //public CodeNavigationResult[] GetMethodElementsUseField(String fieldname)
        //{
        //    List<CodeNavigationResult> listMethodElements = new List<CodeNavigationResult>();

        //    //XElement[] allmethods = GetFullMethods();
        //    XElement[] allmethods = FullMethods;

        //    foreach (var method in allmethods)
        //    {
        //        List<int> useLineNum = new List<int>();
        //        if (ifFieldUsedinMethod(method, fieldname, ref useLineNum))
        //        {
        //            foreach (var line in useLineNum)
        //            {
        //                var methodaselement = XElementToProgramElementConverter.GetMethodElementWRelationFromXElement(method, origPath);
        //                methodaselement.ProgramElementRelation = ProgramElementRelation.Use;
        //                methodaselement.RelationLineNumber.Clear();
        //                methodaselement.RelationLineNumber.Add(line);
        //                listMethodElements.Add(methodaselement);
        //            }
        //        }
        //    }

        //    return listMethodElements.ToArray();
        //}

        ///// <summary>
        ///// Get methods that uses a given field.
        ///// </summary>
        ///// <param name="fieldname">field NAME in String</param>
        ///// <returns>an array of FULL methods in XElement</returns>
        //public XElement[] GetMethodsUseField(String fieldname)
        //{
        //    List<XElement> listMethods = new List<XElement>();

        //    //XElement[] allmethods = GetFullMethods();
        //    XElement[] allmethods = FullMethods;

        //    foreach (var method in allmethods)
        //    {
        //        List<int> useLineNum = new List<int>();
        //        if (ifFieldUsedinMethod(method, fieldname, ref useLineNum))
        //        {
        //            listMethods.Add(method);
        //        }
        //    }

        //    return listMethods.ToArray();
        //}

        ///// <summary>
        ///// Get fields used in a given method.
        ///// </summary>
        ///// <param name="method">FULL method in XElement</param>
        ///// <returns>an array of field NAMES in XElement</returns>
        //public XElement[] GetFieldsUsedinMethod(XElement method)
        //{
        //    List<XElement> listFieldsUsed = new List<XElement>();

        //    XElement[] allfields = GetFieldNames();
        //    foreach (var field in allfields)
        //    {
        //        List<int> useLineNum = new List<int>();
        //        if (ifFieldUsedinMethod(method, field.Value, ref useLineNum))
        //            listFieldsUsed.Add(field);
        //    }

        //    return listFieldsUsed.ToArray();
        //}

        ///// <summary>
        ///// Get methods that uses a given field.
        ///// </summary>
        ///// <param name="fieldname">field NAME in String</param>
        ///// <returns>an array of method NAMES in XElement</returns>
        //public XElement[] GetMethodNamesUseField(String fieldname)
        //{
        //    List<XElement> listMethods = new List<XElement>();

        //    //XElement[] allmethods = GetFullMethods();
        //    XElement[] allmethods = FullMethods;
        //    foreach (var method in allmethods)
        //    {
        //        List<int> useLineNum = new List<int>();
        //        if (ifFieldUsedinMethod(method, fieldname, ref useLineNum))
        //        {
        //            var methodname = method.Element(SRC.Name);
        //            listMethods.Add(methodname);
        //        }
        //    }

        //    return listMethods.ToArray();
        //}

        //public XElement[] GetMethodNamesUseField(XElement fieldname)
        //{
        //    return GetMethodNamesUseField(fieldname.Value);
        //}

        //private int IfCalledByMe(XElement method, IEnumerable<XElement> allCalls)
        //{
        //    foreach (var call in allCalls)
        //    {
        //        if (Matches(call, method))
        //            return call.GetSrcLineNumber();
        //    }
        //    return -1;
        //}

        

        //public List<CodeSearchResult> GetRelatedMethods(CodeSearchResult codeSearchResult)
        //{
        //    return this.GetMethodsAsMethodElements();
        //}

        #endregion

      
    }
}
