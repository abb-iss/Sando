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
            List<String> FieldNames = new List<String>() { "configWorkspaceRoot", 
                "configList", "configSelect", "files_in_build", "allSelections", 
                "configCnt", "fileImpacted", "lineCntImpactedPerFile", "funcImpacted",
                "lineCntImpactedPerFunc", "code_change_imp_file", "configIndexImpacted", "configImpacted"};
                        
            GraphBuilder gbuilder = new GraphBuilder(srcPath);
            var fields = gbuilder.GetFieldNames();

            foreach (var field in fields)
            {                
                String fieldname = field.Value;
                //Console.WriteLine(fieldname);
                Assert.IsTrue(FieldNames.Contains(fieldname));
                FieldNames.Remove(fieldname);
            }

            Assert.IsEmpty(FieldNames);
        }

        [Test]
        public void GetMethodNamesTest()
        {
            List<String> MethodNames = new List<string>()
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
                Assert.IsTrue(MethodNames.Contains(methodname));
                MethodNames.Remove(methodname);
            }

            Assert.IsEmpty(MethodNames);
        }

        [Test]
        public void GetAllLocalVarsinMethodTest()
        {
            List<String> LocalVars = new List<String>() 
            { "pos", "configListFile", "line", "configuration" }; 
                
            GraphBuilder gbuilder = new GraphBuilder(singlemethodPath);
            var methods = gbuilder.GetMethods();
            
            var method = methods.First(); //should be only one method
            var localvars = gbuilder.GetAllLocalVarsinMethod(method);
            foreach (var localvar in localvars)
            {
                String varname = localvar.Value;
                Assert.IsTrue(LocalVars.Contains(varname));
                LocalVars.Remove(varname);
            }

            Assert.IsEmpty(LocalVars);
        }

        [Test]
        public void GetAllParametersinMethodTest()
        {
            List<String> Parameters = new List<String>() { "configListFileName"};

            GraphBuilder gbuilder = new GraphBuilder(singlemethodPath);
            var methods = gbuilder.GetMethods();

            var method = methods.First(); //should be only one method
            var parameters = gbuilder.GetAllParametersinMethod(method);
            foreach (var parameter in parameters)
            {
                String paraname = parameter.Value;
                Assert.IsTrue(Parameters.Contains(paraname));
                Parameters.Remove(paraname);
            }

            Assert.IsEmpty(Parameters);
        }
    }
    
}
