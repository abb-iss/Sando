using System;
using System.Collections.Generic;
//using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using EnvDTE;
////using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;

/******* from SolutionMonitorTest *******/
using Sando.Core;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using UnitTestHelpers;
/******* from SolutionMonitorTest *******/




namespace Sando.UI.UnitTests
{
    [TestClass]
    ////public class SolutionMonitorIntegrationTest : IInvoker
    public class SolutionMonitorIntegrationTest
    {
        private static Solution ModelSolution;          // EnvDTE.Solution        
        private static Microsoft.VisualStudio.ComponentModelHost.IComponentModel context;        


        /******* from SolutionMonitorTest *******/
        private static SolutionMonitor monitor;
        private static SolutionKey key;
        private const string _luceneTempIndexesDirectory = "C:/Windows/Temp";
        /******* from SolutionMonitorTest *******/


        [ClassInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            OpenTestModelingProject(testContext);
    

            /******* from SolutionMonitorTest *******/
            TestUtils.InitializeDefaultExtensionPoints();
            /******* from SolutionMonitorTest *******/
        }

        //// BaseProgramModel.Constants.SrcmlPath
        public static void OpenTestModelingProject(TestContext testContext)
        {
            context = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;

            ModelSolution = VsIdeTestHostContext.Dte.Solution;
            ////open up notepad and scintilla copy
            ////ModelSolution.Open(@"C:\TestingFiles\NotepadAndScintilla\NotepadAndScintilla\NotepadAndScintilla.sln");
            ModelSolution.Open(@"C:\Users\USJIZHE\Documents\Sando\Sando\Sando.sln");
            


   
        }





    
        /******* from SolutionMonitorTest *******/
               
        //// SequenceDiagramTesting.MockProgress
        //// [No any reference]
        [TestInitialize]
        public void Init()
        {
            ////searchRunner.setInvoker(MockProgress.getProgress(this));

            /******* from SolutionMonitorTest *******/
            Directory.CreateDirectory(_luceneTempIndexesDirectory + "/basic/");
            TestUtils.ClearDirectory(_luceneTempIndexesDirectory + "/basic/");
            key = new SolutionKey(Guid.NewGuid(), ".\\TestFiles\\FourCSFiles", _luceneTempIndexesDirectory + "/basic/");
            var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
            monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer, false);
            string[] files = Directory.GetFiles(".\\TestFiles\\FourCSFiles");
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                monitor.ProcessFileForTesting(fullPath);
            }
            monitor.UpdateAfterAdditions();
            /******* from SolutionMonitorTest *******/
        }



        /******* from SolutionMonitorTest *******/
        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_BasicSetupTest()
        {

        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_SearchTwoWords()
        {
            var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
            string ensureLoaded = "extension file";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(ensureLoaded);
            foreach (var codeSearchResult in codeSearchResults)
            {
                var method = codeSearchResult.Element as MethodElement;
                if (method != null)
                {
                    if (method.Name.Equals("SetFileExtension"))
                        return;
                }
            }
            Assert.Fail("Failed to find relevant search result for search: " + ensureLoaded);
        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_SearchForExtension()
        {
            var codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(key));
            string ensureLoaded = "extension";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(ensureLoaded);
            foreach (var codeSearchResult in codeSearchResults)
            {
                var method = codeSearchResult.Element as MethodElement;
                if (method != null)
                {
                    if (method.Name.Equals("SetFileExtension"))
                        return;
                }
            }
            Assert.Fail("Failed to find relevant search result for search: " + ensureLoaded);
        }
  


        /******* from SolutionMonitorTest *******/
        [TestCleanup]  // [TearDown] (TearDown for Unit Test)
        public void TearDown()
        {
            monitor.StopMonitoring(true);
            //Directory.Delete(_luceneTempIndexesDirectory + "/basic/", true);  // not delete this folder in order to see inside the box
        }
        /******* from SolutionMonitorTest *******/

        
        
        [ClassCleanup]
        public static void TestCleanup()
        {
            ////searchRunner.Dispose();
        }









        //// FROM MSDN: http://msdn.microsoft.com/en-us/library/gg985355.aspx#UiThread
        //// If your tests, or the methods under test, make changes to the model store, 
        //// then you must execute them in the user interface thread. If you do not do this, 
        //// you might see an AccessViolationException. Enclose the code of the test method in a call to Invoke:
        //// System.Windows.Forms.MethodInvoker
        public void Invoke(MethodInvoker globalSystemWindowsFormsMethodInvoker)
        {
            UIThreadInvoker.Invoke(globalSystemWindowsFormsMethodInvoker);
        }

        //// ABB.SolutionIndexer.Core.IDirectoryProvider [No any reference]
        /*
        public static void ClearLuceneDirectory(IDirectoryProvider dirProvider)
        {
            var files = Directory.EnumerateFiles(dirProvider.GetLuceneDirectoryPath(), "*");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    //ignore for now
                }
            }
        }
        */
    }
}
