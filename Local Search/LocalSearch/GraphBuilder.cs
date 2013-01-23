using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ABB.SrcML;
using System.Runtime.CompilerServices;

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
        /// Get all field declarations in the source file.
        /// </summary>
        /// <returns>An array (of "decl_stmt" XElement) that contains all the field declarations.</returns>
        public XElement[] GetFieldDecs()
        {
            List<XElement> AllFields = new List<XElement>();
            
            foreach (XElement file in srcmlFile.FileUnits)
            {
                var fields = from field in file.Descendants()
                                where field.Name.Equals(SRC.DeclarationStatement)
                                && !field.Ancestors(SRC.Function).Any()
                                && !field.Ancestors(SRC.Constructor).Any()
                                && !field.Ancestors(SRC.Destructor).Any()
                                select field;
                
                if (fields != null)
                    AllFields.AddRange(fields);
            }

            //FieldDecs = AllFields.ToArray();
            return AllFields.ToArray();
        }

        /// <summary>
        /// Get all field names declared in the source file.
        /// </summary>
        /// <returns>An array (of "name" XElement) that contains all the field names.</returns>
        public XElement[] GetFieldNames()
        {   
            List<XElement> AllFieldNames = new List<XElement>();

            var fields = GetFieldDecs();

            foreach (XElement field in fields)
            {
                var fieldname = field.Element(SRC.Declaration).Element(SRC.Name);                
                if (fieldname != null)
                    AllFieldNames.Add(fieldname);
            }

            return AllFieldNames.ToArray();
        }

        /// <summary>
        /// Get all functions (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array (of XElement) that contains all the methods/constructors/destructors.</returns>
        public XElement[] GetMethods()
        {
            List<XElement> Allmethods = new List<XElement>();

            foreach (XElement file in srcmlFile.FileUnits)
            {
                var methods = from method in file.Descendants()
                              where method.Name.Equals(SRC.Function)
                             || method.Name.Equals(SRC.Constructor)
                             || method.Name.Equals(SRC.Destructor)
                             select method;

                if (methods != null)
                    Allmethods.AddRange(methods);
            }

            FieldDecs = Allmethods.ToArray();
            return FieldDecs;
        }

        /// <summary>
        /// Get names of all functions (including constructor and destructor) in the source file.
        /// </summary>
        /// <returns>An array (of "name" XElement) that contains names of all the functions.</returns>
        public XElement[] GetMethodNames()
        {
            List<XElement> AllMethodNames = new List<XElement>();

            var methods = GetMethods();

            foreach (XElement method in methods)
            {
                var methodname = method.Element(SRC.Name);
                if (methodname != null)
                    AllMethodNames.Add(methodname);
            }

            return AllMethodNames.ToArray();
        }

        /// <summary>
        /// Get all local variables declared in a given method XElement.
        /// </summary>
        /// <param name="srcPath">A given method in XElement format.</param>
        public XElement[] GetAllLocalVarsinMethod(XElement method)
        {
            XElement methodbody = method.Element(SRC.Block);
            var localdeclares =
                methodbody.Descendants(SRC.DeclarationStatement);
               
            List<XElement> localvars = new List<XElement>();
            
            foreach (var localdeclare in localdeclares)
            {
                XElement localvar = localdeclare.Element(SRC.Declaration).Element(SRC.Name);
                localvars.Add(localvar);
            }
            
            return localvars.ToArray();
        }

        /// <summary>
        /// Get all parameters for a given method XElement.
        /// </summary>
        /// <param name="srcPath">A given method in XElement format.</param>
        public XElement[] GetAllParametersinMethod(XElement method)
        {
            XElement paralist = method.Element(SRC.ParameterList);
            var parameters = paralist.Elements(SRC.Parameter);

            List<XElement> paras = new List<XElement>();

            foreach (var parameter in parameters)
            {
                var paradec = parameter.Element(SRC.Declaration);
                var para = paradec.Element(SRC.Name);
                paras.Add(para);
            }

            return paras.ToArray();
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
        /// <param name="srcPath">field and method</param>
        /// <returns>true if it is used.</returns>
        //public bool ifFieldUsedinMethod(XElement method, XElement field)
        //{
        //    var nonlocalvars = GetAllNonLocalVarsinMethod(method);
        //    List<String> list_nonlocalvars = new List<string>();
        //    foreach (var nonlocalvar in nonlocalvars)
        //        list_nonlocalvars.Add(nonlocalvar.Value);

        //    return (list_nonlocalvars.Contains(field.Value));
        //}

    }
}
