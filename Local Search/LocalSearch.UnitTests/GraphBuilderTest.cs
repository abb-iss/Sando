using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.SrcML;
using NUnit.Framework;
using System.Xml.Linq;

namespace LocalSearch.UnitTests
{
    [TestFixture]
    public class GraphBuilderTest
    {
        private static String srcPath = @"..\..\TestFiles\ConfigManip.cs";
        private static String xmlPath = @"..\..\TestFiles\ConfigManip.XML";

        private static String singlemethodPath = @"..\..\TestFiles\TestMethod.cs";

        //[Test]
        //public void ConvertSrcToXMLTest()
        //{
        //    Src2SrcMLRunner srcmlConverter;
        //    String fileExt = Path.GetExtension(srcPath);
        //    if (fileExt.Equals(".cs"))
        //        srcmlConverter = new Src2SrcMLRunner(@"C:\WORK-XIAO\sando\LIBS\srcML-Win-cSharp");
        //    else
        //        srcmlConverter = new Src2SrcMLRunner(@"C:\WORK-XIAO\sando\LIBS\srcML-Win");

        //    var tempSrcMLFile = srcmlConverter.GenerateSrcMLFromFile(srcPath, xmlPath);
        //}

        [Test]
        public void GetFieldNamesTest()
        {
            List<String> listFieldNames = new List<String>() { "configWorkspaceRoot", 
                "configList", "configSelect", "files_in_build", "allSelections", 
                "configCnt", "fileImpacted", "lineCntImpactedPerFile", "funcImpacted",
                "lineCntImpactedPerFunc", "code_change_imp_file", "configIndexImpacted", "configImpacted"};
                        
            GraphBuilder gbuilder = new GraphBuilder(srcPath);
            var fields = gbuilder.GetFieldNames();

            foreach (var field in fields)
            {                
                String fieldname = field.Value;
                //Console.WriteLine(fieldname);
                Assert.IsTrue(listFieldNames.Contains(fieldname));
                listFieldNames.Remove(fieldname);
            }

            Assert.IsEmpty(listFieldNames);
        }

        [Test]
        public void GetMethodNamesTest()
        {
            List<String> listMethodNames = new List<string>()
            { "toUpper", "convertInt", "replaceCharInStr", "removeCharInStr", 
              "ConfigManip", "ConfigManip", "displayConfig", "getConfigSelect", 
              "getChangeImpact","genConfigImp","findChangeImpConfig","genConfigImp",
              "findChangeImpConfig","getConfigImpacted","genSingleConfigImp","genMultiSelectImp",
              "getChangeSets","getFileInBuild","genFileImp","genFuncImp",
              "genImpUnionFile","genImpUnionFunc"
            };

            GraphBuilder gbuilder = new GraphBuilder(xmlPath);
            var methods = gbuilder.GetMethodNames();

            foreach (var method in methods)
            {
                String methodname = method.Value;
                Assert.IsTrue(listMethodNames.Contains(methodname));
                listMethodNames.Remove(methodname);
            }

            Assert.IsEmpty(listMethodNames);
        }

        [Test]
        public void GetAllLocalVarsinMethodTest()
        {
            List<String> listLocalVars = new List<String>() 
            { "pos", "configListFile", "line", "configuration" }; 
                
            GraphBuilder gbuilder = new GraphBuilder(singlemethodPath);
            var methods = gbuilder.GetMethods();
            
            var method = methods.First(); //should be only one method
            var localvars = gbuilder.GetAllLocalVarsinMethod(method);
            foreach (var localvar in localvars)
            {
                String varname = localvar.Value;
                Assert.IsTrue(listLocalVars.Contains(varname));
                listLocalVars.Remove(varname);
            }

            Assert.IsEmpty(listLocalVars);
        }

        [Test]
        public void GetAllParametersinMethodTest()
        {
            List<String> listParameters = new List<String>() { "configListFileName"};

            GraphBuilder gbuilder = new GraphBuilder(singlemethodPath);
            var methods = gbuilder.GetMethods();
            var method = methods.First(); //should be only one method

            var parameters = gbuilder.GetAllParametersinMethod(method);
            foreach (var parameter in parameters)
            {
                String paraname = parameter.Value;
                Assert.IsTrue(listParameters.Contains(paraname));
                listParameters.Remove(paraname);
            }

            Assert.IsEmpty(listParameters);
        }

        [Test]
        public void GetFieldsUsedinMethodTest()
        {
            List<String> listFieldsUsed = new List<String>() 
            { "configCnt", "configWorkspaceRoot","configList"};

            GraphBuilder gbuilder = new GraphBuilder(singlemethodPath);
            var methods = gbuilder.GetMethods();
            var method = methods.First(); //should be only one method

            var fieldsused = gbuilder.GetFieldsUsedinMethod(method);
            foreach (var fieldused in fieldsused)
            {
                String fieldname = fieldused.Value;
                Assert.IsTrue(listFieldsUsed.Contains(fieldname));
                listFieldsUsed.Remove(fieldname);
            }

            Assert.IsEmpty(listFieldsUsed);
        }

        [Test]
        public void GetMethodsUseFieldTest1()
        {
            List<String> listMethodsUse = new List<String>() 
            { "ConfigManip", "genConfigImp", "findChangeImpConfig" };

            GraphBuilder gbuilder = new GraphBuilder(xmlPath);
            var methodsuse = gbuilder.GetMethodsUseField("configWorkspaceRoot");
            foreach (var method in methodsuse)
            {
                String methodname = method.Value;
                Assert.IsTrue(listMethodsUse.Contains(methodname));
                listMethodsUse.Remove(methodname);
            }

            Assert.IsEmpty(listMethodsUse);
        }

        [Test]
        public void GetMethodsUseFieldTest2()
        {
            List<String> listMethodsUse = new List<String>() { "ConfigManip", "displayConfig"};

            GraphBuilder gbuilder = new GraphBuilder(xmlPath);
            var methodsuse = gbuilder.GetMethodsUseField("configList");
            foreach (var method in methodsuse)
            {
                String methodname = method.Value;
                Assert.IsTrue(listMethodsUse.Contains(methodname));
                listMethodsUse.Remove(methodname);
            }

            Assert.IsEmpty(listMethodsUse);
        }

        [Test]
        public void GetMethodsUseFieldTest3()
        {
            List<String> listMethodsUse = new List<String>() { "getConfigSelect", "getChangeSets" };

            GraphBuilder gbuilder = new GraphBuilder(xmlPath);
            var methodsuse = gbuilder.GetMethodsUseField("configSelect");
            foreach (var method in methodsuse)
            {
                String methodname = method.Value;
                Assert.IsTrue(listMethodsUse.Contains(methodname));
                listMethodsUse.Remove(methodname);
            }

            Assert.IsEmpty(listMethodsUse);
        }

        [Test]
        public void GetFieldDeclFromNametest() 
        {
            GraphBuilder gbuilder = new GraphBuilder(xmlPath);
            String fieldname = "fileImpacted";
            XElement fielddecl = gbuilder.GetFieldDeclFromName(fieldname);

            Assert.IsTrue(fieldname.Equals(fielddecl.Element(SRC.Name).Value));
            String strDecl = "private List<string> fileImpacted = new List<string>()";
            Assert.IsTrue(fielddecl.ToSource().Equals(strDecl));
        }


    }
    
}
