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
        private static String srcPath = @"..\..\Local Search\LocalSearch.UnitTests\TestFiles\DatabaseMenuCommands.cs";
        private static String callsSrcPath = @"..\..\Local Search\LocalSearch.UnitTests\TestFiles\TestingFile.cs";
        private static String xmlPath = @"..\..\Local Search\LocalSearch.UnitTests\TestFiles\DatabaseMenuCommands.XML";

        private static String singlemethodPath = @"..\..\Local Search\LocalSearch.UnitTests\TestFiles\TestMethod.cs";

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
            var methods = gbuilder.GetFullMethods();
            
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
            var methods = gbuilder.GetFullMethods();
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

        
        //[Test]
        //public void GetMethodsUseFieldTest1()
        //{
        //    List<String> listMethodsUse = new List<String>() 
        //    { "ConfigManip", "genConfigImp", "findChangeImpConfig" };

        //    GraphBuilder gbuilder = new GraphBuilder(xmlPath);
        //    var methodsuse = gbuilder.GetMethodNamesUseField("configWorkspaceRoot");
        //    foreach (var method in methodsuse)
        //    {
        //        String methodname = method.Value;
        //        Assert.IsTrue(listMethodsUse.Contains(methodname));
        //        listMethodsUse.Remove(methodname);
        //    }

        //    Assert.IsEmpty(listMethodsUse);
        //}

        //[Test]
        //public void GetMethodsUseFieldTest2()
        //{
        //    List<String> listMethodsUse = new List<String>() { "ConfigManip", "displayConfig"};

        //    GraphBuilder gbuilder = new GraphBuilder(xmlPath);
        //    var methodsuse = gbuilder.GetMethodNamesUseField("configList");
        //    foreach (var method in methodsuse)
        //    {
        //        String methodname = method.Value;
        //        Assert.IsTrue(listMethodsUse.Contains(methodname));
        //        listMethodsUse.Remove(methodname);
        //    }

        //    Assert.IsEmpty(listMethodsUse);
        //}

        //[Test]
        //public void GetMethodsUseFieldTest3()
        //{
        //    List<String> listMethodsUse = new List<String>() { "getConfigSelect", "getChangeSets" };

        //    GraphBuilder gbuilder = new GraphBuilder(xmlPath);
        //    var methodsuse = gbuilder.GetMethodNamesUseField("configSelect");
        //    foreach (var method in methodsuse)
        //    {
        //        String methodname = method.Value;
        //        Assert.IsTrue(listMethodsUse.Contains(methodname));
        //        listMethodsUse.Remove(methodname);
        //    }

        //    Assert.IsEmpty(listMethodsUse);
        //}

        [Test]
        public void GetXElementFromLineNumTest()
        {
            GraphBuilder gbuilder = new GraphBuilder(xmlPath);
            XElement line = gbuilder.GetXElementFromLineNum(9);
            String linestr = "DatabaseCommand = new RoutedUICommand(\"Command from table context menu\", \"DatabaseCommand\", typeof(DatabaseMenuCommands));";
            Assert.IsTrue(line.ToSource().Equals(linestr));
        }

        //[Test]
        //public void GetFieldDeclFromNameTest() 
        //{
        //    GraphBuilder gbuilder = new GraphBuilder(xmlPath);
        //    String fieldname = "fileImpacted";
        //    XElement fielddecl = gbuilder.GetFieldDeclFromName(fieldname);

        //    Assert.IsTrue(fieldname.Equals(fielddecl.Element(SRC.Name).Value));
        //    String strDecl = "private List<string> fileImpacted = new List<string>()";
        //    Assert.IsTrue(fielddecl.ToSource().Equals(strDecl));
        //}

        //[Test]
        //public void GetMethodCallsMethodTest()
        //{

        //    GraphBuilder gbuilder = new GraphBuilder(callsSrcPath);
        //    var methods = gbuilder.GetFullMethods();
        //    var updateMethod = methods.Where(x => x.Element(SRC.Name).Value.Equals("UpdateFile"));
        //    var callees = gbuilder.GetCallees(updateMethod.First());
        //    Assert.IsTrue(callees.Count() > 0);
        //}
    }
    
}
