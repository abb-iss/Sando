using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.ComponentModel.Composition;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

using EnvDTE;
////using EnvDTE80;
////using Microsoft.VisualStudio.ArchitectureTools.Extensibility;   //just for test
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Uml.Classes;
using Microsoft.VisualStudio.Uml.Interactions;
using Microsoft.VSSDK.Tools.VsIdeTesting;

/******* from SolutionMonitorTest *******/
////using NUnit.Framework; // instead, using Microsoft.VisualStudio.TestTools.UnitTesting for VS IDE tests
using Sando.Core;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI;
using Sando.UI.Monitoring;
using UnitTestHelpers;
/******* from SolutionMonitorTest *******/

//using ABB.SrcML.VisualStudio.SolutionMonitor;

/* //// in SequenceDiagramTesting.IfElseBlocksBuilderTest but useless here
using ABB.DetailedGenerator;
using ABB.SolutionIndexer;
using ABB.SolutionIndexer.Core;
using ABB.SolutionIndexer.Engines;
using ABB.SrcML;
using Constants = BaseProgramModel.Constants;
using DetailedSequenceDiagramGenerator;
using ProgramModel;
using RuProgressBar;
using SequenceDiagramWidgets.Graph;
using StandaloneSequenceSearchDialog.Engines;
*/

namespace Sando.IntegrationTests
{
    [TestClass]
    public class SolutionMonitorIntegrationTest : IInvoker  // Why does it need IInvoker?
    {
        /// <summary>
        /// Location of a VS Solution that defines an initial state for your tests.
        /// </summary>
        private const string testSolutionFilePath = @"C:\Users\USJIZHE\Documents\Sando\Sando\Sando.sln";

        /// <summary>
        /// Make EnvDTE.Solution available to test methods.
        /// </summary>
        private static Solution ModelSolution;

        /// <summary>
        /// From Microsoft.VisualStudio.ComponentModelHost.IComponentModel
        /// </summary>
        private static IComponentModel context;

        /// <summary>
        /// SrcML.NET's solution monitor.
        /// </summary>
        private static ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitor sm;
        private static ABB.SrcML.VisualStudio.SolutionMonitor.FileSystemSrcMLFolder srcMLFolder;

        ////private static SearchRunner searchRunner;   // ABB.SolutionIndexer.Engines.SearchRunner
        ////private static UMLModelManager Manager;     // DetailedSequenceDiagramGenerator.UMLModelManager [No any reference]
        ////private static DetailedSDGenerator generator;   // ABB.DetailedGenerator.DetailedSDGenerator
        ////private static ObservableCollection<SequenceGraph> graphs;  // SequenceDiagramWidgets.Graph.SequenceGraph

        /******* from SolutionMonitorTest *******/
        ////private static SolutionMonitor monitor;     // Sando's SolutionMonitor
        ////private static SolutionKey key;             // Sando's SolutionKey
        ////private const string _luceneTempIndexesDirectory = "C:/Windows/Temp";
        /******* from SolutionMonitorTest *******/

        [ClassInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            /******* from SolutionMonitorTest *******/
            TestUtils.InitializeDefaultExtensionPoints();
            /******* from SolutionMonitorTest *******/

            OpenTestModelingProject(testContext);

            // Initialize SrcML.NET's Solution Monitor
            // TODO: Is there a GetTrackedProjectItems() API? Or do we need it?
            sm = ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitorFactory.CreateMonitor(true);
            // Start monitoring
            sm.StartMonitoring();

            WriteLog("D:\\Data\\log.txt", "@@@@@@@ To new FileSystemSrcMLFolder()");
            srcMLFolder = new ABB.SrcML.VisualStudio.SolutionMonitor.FileSystemSrcMLFolder("D:\\Data\\SandoSrcMLArchive");




            ////InitializeGenerator();
            ////graphs = new ObservableCollection<SequenceGraph>();
            ////for (int i = 0; i < 6; i++)
            ////{
            ////    graphs[i] = new SequenceGraph();
            ////}
        }

        /// <summary>
        /// Open test modeling project.
        /// </summary>
        /// <param name="testContext"></param>
        public static void OpenTestModelingProject(TestContext testContext)
        {
            // Get the components service
            context = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            // Open a solution that is the initial state for your tests
            ModelSolution = VsIdeTestHostContext.Dte.Solution;
            ModelSolution.Open(testSolutionFilePath);
            Assert.IsNotNull(ModelSolution, "VS solution not found");
            
            ////open up notepad and scintilla copy
            ////ModelSolution.Open(@"C:\TestingFiles\NotepadAndScintilla\NotepadAndScintilla\NotepadAndScintilla.sln");

            //Manager = new UMLModelManager();
            //context.DefaultCompositionService.SatisfyImportsOnce(Manager);            
            //Manager.SetDTE(VsIdeTestHostContext.Dte);

            //init indexer

            //init searcher
            ////searchRunner = new SearchRunner();
            ////searchRunner.Initialize("", "", Constants.SrcmlPath, ".", null);            //TODO - fix this
        }

        //// DetailedSequenceDiagramGenerator.UMLModelManager
        private static void InitializeGenerator()
        {
            ////if (generator == null)
            ////{
            ////    if (UMLModelManager.IsRunning())
            ////    {
            ////        generator = new DetailedSDGenerator();
            ////        generator.Initialize(UMLModelManager.GetInstance());
            ////    }
            ////    else
            ////    {
            ////        generator = new DetailedSDGenerator();
            ////        generator.Initialize(context, VsIdeTestHostContext.Dte);
            ////    }
            ////}
        }

        [TestInitialize]
        public void Init()
        {
            ////searchRunner.setInvoker(MockProgress.getProgress(this));

            /******* from SolutionMonitorTest *******/
            /*
            Directory.CreateDirectory(_luceneTempIndexesDirectory + "/basic/");
            TestUtils.ClearDirectory(_luceneTempIndexesDirectory + "/basic/");
            //key = new SolutionKey(Guid.NewGuid(), ".\\TestFiles\\FourCSFiles", _luceneTempIndexesDirectory + "/basic/");
            key = new SolutionKey(Guid.NewGuid(), "C:\\Users\\USJIZHE\\Documents\\Sando\\bin\\Debug\\TestFiles\\FourCSFiles", _luceneTempIndexesDirectory + "/basic/");
            var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
            monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer, false);
            //string[] files = Directory.GetFiles(".\\TestFiles\\FourCSFiles");
            string[] files = Directory.GetFiles("C:\\Users\\USJIZHE\\Documents\\Sando\\bin\\Debug\\TestFiles\\FourCSFiles");

            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                //monitor.ProcessFileForTesting(fullPath);
            }
            //monitor.UpdateAfterAdditions();
            */
            /******* from SolutionMonitorTest *******/
        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_EmptyTest()
        {
            // To test startup features of SrcML.NET
            System.Threading.Thread.Sleep(5000);
        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_AddProjectItemsTest()
        {
            AddProjectItems();
        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_SaveProjectItemsTest()
        {
            SaveProjectItems("C:\\Users\\USJIZHE\\Documents\\Sando\\IntegrationTests\\TestFiles\\Class222.c");
            sm.saveRDTFile("C:\\Users\\USJIZHE\\Documents\\Sando\\IntegrationTests\\TestFiles\\Class222.c");
        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_DeleteProjectItemsTest()
        {
            DeleteProjectItems("C:\\Users\\USJIZHE\\Documents\\Sando\\IntegrationTests\\TestFiles\\Class222.c");
        }

        /******* from SolutionMonitorTest *******/
        /*
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
        */

        /*
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
        */
        /******* from SolutionMonitorTest *******/


        /* from IfElseBlocksBuilderTest
        [HostType("VS IDE")]
        [TestMethod]
        public void SimpleIfBlockTest()
        {
            searchRunner.RunTest(graphs, "line");
            Assert.IsTrue(graphs[0].sEdges.Count > 0);

            var interaction = generator.GenerateTest("line", graphs[0], MockProgress.getProgress(this), "");
            Assert.IsTrue(interaction.Lifelines.Count() == 4);
            Assert.IsTrue(interaction.Lifelines.First().GetAllOutgoingMessages().Count() == 1);
            Assert.IsTrue(interaction.Lifelines.Last().GetAllOutgoingMessages().Count() == 1);
            //Assert.IsTrue(interaction.Lifelines.First().GetCombinedFragments().Count() >= 3);
            //var myIfBlock = interaction.Lifelines.Last().GetCombinedFragments().First();
            //Assert.IsTrue(myIfBlock.CoveredLifelines.First().Name == "CellBuffer");
            //Assert.IsTrue(myIfBlock.CoveredLifelines.Last().Name == "Generated");
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void NoIfBlocksTest()
        {
            searchRunner.RunTest(graphs, "file");
            Assert.IsTrue(graphs[0].sEdges.Count > 0);

            var interaction = generator.GenerateTest("file", graphs[1], MockProgress.getProgress(this), "");
            //Assert.IsTrue(interaction.Lifelines.Count() == 3);
            //Assert.IsTrue(interaction.Lifelines.First().GetAllOutgoingMessages().Count() == 8);
            //Assert.IsTrue(interaction.Lifelines.Last().GetAllOutgoingMessages().Count() == 3);
            //Assert.IsTrue(interaction.Lifelines.First().GetCombinedFragments().Count() == 0);            
            //Assert.IsTrue((interaction.Lifelines.First().Name == "NppParameters"));
            //Assert.IsTrue((interaction.Lifelines.Last().Name == "TiXmlElement"));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void FileOpenScenarioTest()
        {
            var searchTerm = "file";
            string[] methods = { "Notepad_plusfileNew", "Notepad_plusfileOpen", "FileManagerdeleteFile", "Notepad_plusfileSaveSession" };
            CreateGraphsForSpecificMethods(searchTerm, methods);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void StartScenarioTest()
        {
            var searchTerm = "start contracted";
            string[] methods = { "EditorToggleContraction", "EditorContractedFoldNext", "ContractionStateShowAll", "ContractionStateHiddenLines" };
            CreateGraphsForSpecificMethods(searchTerm, methods);
        }
        
        [HostType("VS IDE")]
        [TestMethod]
        public void StartRedoScenarioTest()
        {
            var searchTerm = "start redo";
            string[] methods = { "CellBufferCanRedo", "CellBufferGetRedoStep" };
            CreateGraphsForSpecificMethods(searchTerm, methods);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void LoadSessionTest()
        {
            var searchTerm = "load session";
            string[] methods = {  "NppParametersloadSession", "NppParameterswriteSession", 
                               "Notepad_plusfileLoadSession","Notepad_plusloadSession"
                               };
            CreateGraphsForSpecificMethods(searchTerm, methods);
        }

        //// ProgramModel.IMethod
        private void CreateGraphsForSpecificMethods(string searchTerm, string[] methods)
        {
            var myResults = searchRunner.RunTest(graphs, searchTerm);
            var theGraph = graphs[0];
            List<IMethod> resultsIWant = new List<IMethod>();
            foreach (var method in myResults)
            {
                var name = method.GetNamespace() + method.Name();
                foreach (var methName in methods)
                {
                    if ((method.GetNamespace() + method.Name()).ToLower().Equals(methName.ToLower()))
                    {
                        resultsIWant.Add(method);
                    }
                }
            }
            Assert.IsTrue(resultsIWant.Count >= methods.Length);
            var interaction = generator.GenerateTest(searchTerm, theGraph, MockProgress.getProgress(this), "");
            //System.Threading.Thread.Sleep(20000);
        }
        */


        /******* from SolutionMonitorTest *******/
        [TestCleanup]  // [TearDown] (TearDown for Unit Test)
        public void TearDown()
        {
            //monitor.StopMonitoring(true);
            //Directory.Delete(_luceneTempIndexesDirectory + "/basic/", true);  // not delete this folder in order to see inside the box
        }
        /******* from SolutionMonitorTest *******/
        
        [ClassCleanup]
        public static void TestCleanup()
        {
            // Stop monitoring
            sm.StopMonitoring();
            srcMLFolder.StopWatching();
            ////searchRunner.Dispose();
        }
        
        // FROM MSDN: http://msdn.microsoft.com/en-us/library/gg985355.aspx#UiThread
        // If your tests, or the methods under test, make changes to the model store, 
        // then you must execute them in the user interface thread. If you do not do this, 
        // you might see an AccessViolationException. Enclose the code of the test method in a call to Invoke:
        // System.Windows.Forms.MethodInvoker
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


        /// <summary>
        /// For debugging.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="str"></param>
        private static void WriteLog(string logFile, string str)
        {
            StreamWriter sw = new StreamWriter(logFile, true, System.Text.Encoding.ASCII);
            sw.WriteLine(str);
            sw.Close();
        }

        /// <summary>
        /// Add project items in VS environment for testing.
        /// </summary>
        public static void AddProjectItems()
        {
            var allProjects = ModelSolution.Projects;
            var enumerator = allProjects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var project = (Project)enumerator.Current;
                if (project != null && project.ProjectItems != null)
                {
                    //writeLog("D:\\Data\\log.txt", "Project: [" + project.FullName + "]");
                    if ("C:\\Users\\USJIZHE\\Documents\\Sando\\IntegrationTests\\IntegrationTests.csproj".Equals(project.FullName))
                    {
                        var items = project.ProjectItems.GetEnumerator();
                        while (items.MoveNext())
                        {
                            var item = (ProjectItem)items.Current;
                            if ("TestFiles".Equals(item.Name))
                            {
                                WriteLog("D:\\Data\\log.txt", "ProjectItem to be added under folder: [" + item.Name + "]");
                                item.ProjectItems.AddFromFileCopy("D:\\Data\\Class111.c");
                                //item.ProjectItems.AddFromFileCopy("D:\\Data\\Class111.cs");
                                //item.ProjectItems.AddFromFileCopy("D:\\Data\\Class222.cs");
                                //item.ProjectItems.AddFromFileCopy("D:\\Data\\Class333.cs");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save project items in VS environment for testing.
        /// </summary>
        /// <param name="fileName"></param>
        public static void SaveProjectItems(string fileName)
        {
            var projectItem = ModelSolution.FindProjectItem(fileName);
            if (projectItem != null)
            {
                WriteLog("D:\\Data\\log.txt", "ProjectItem to be saved: [" + projectItem.Name + "]");
                projectItem.Open();
                projectItem.Save();
            }
        }

        /// <summary>
        /// "Save As" project items in VS environment for testing.
        /// </summary>
        /// <param name="fileName"></param>
        public static void SaveAsProjectItems(string fileName)
        {
            var projectItem = ModelSolution.FindProjectItem(fileName);
            if (projectItem != null)
            {
                WriteLog("D:\\Data\\log.txt", "ProjectItem to be saveas-ed: [" + projectItem.Name + "]");
                projectItem.Open();
                projectItem.SaveAs("Class111111.c");   // Note: Class111.cs is Remove()-ed instead of Delete()-ed
            }
        }

        /// <summary>
        /// Delete project items in VS environment for testing.
        /// </summary>
        /// <param name="fileName"></param>
        public static void DeleteProjectItems(string fileName)
        {
            WriteLog("D:\\Data\\log.txt", "DeleteProjectItems(): [" + fileName + "]");
            var projectItem = ModelSolution.FindProjectItem(fileName);
            if (projectItem != null)
            {
                WriteLog("D:\\Data\\log.txt", "ProjectItem to be permanantly deleted: [" + projectItem.Name + "]");
                projectItem.Delete();   // File being deleted from the file system
                //projectItem.Remove();   // File not being deleted from the file system, just removed from VS Solution Explorer
            }
        }


    }
}
