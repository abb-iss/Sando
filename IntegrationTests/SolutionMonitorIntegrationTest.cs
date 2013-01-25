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
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;

namespace Sando.IntegrationTests
{
    [TestClass]
    public class SolutionMonitorIntegrationTest : IInvoker  // Why does it need IInvoker?
    {
        /// <summary>
        /// Location of a VS Solution that defines an initial state for your tests.
        /// </summary>
        private const string testSolutionFilePath = @"D:\Data\SrcML.NETDemo\MySolution\MySolution.sln";

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

        [ClassInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            OpenTestModelingProject(testContext);

            // Initialize SrcML.NET's Solution Monitor
            sm = ABB.SrcML.VisualStudio.SolutionMonitor.SolutionMonitorFactory.CreateMonitor();
            // Start monitoring
            sm.StartMonitoring();
            sm.FileEventRaised += RespondToSolutionMonitorEvent;
        }

        public static void RespondToSolutionMonitorEvent(object sender, ABB.SrcML.FileEventRaisedArgs eventArgs)
        {
            writeLog("D:\\Data\\log.txt", "!! RespondToSolutionMonitorEvent(), eventArgs.EventType = " + eventArgs.EventType);
            switch (eventArgs.EventType)
            {
                case ABB.SrcML.FileEventType.FileAdded:
                    writeLog("D:\\Data\\log.txt", "!! TO ADD index: " + eventArgs.SourceFilePath);
                    break;
                case ABB.SrcML.FileEventType.FileChanged:
                    writeLog("D:\\Data\\log.txt", "!! TO CHANGE index: " + eventArgs.SourceFilePath);
                    break;
                case ABB.SrcML.FileEventType.FileDeleted:
                    writeLog("D:\\Data\\log.txt", "!! TO DELETE index: " + eventArgs.SourceFilePath);
                    break;
            }
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
        }

        [TestInitialize]
        public void Init()
        {
        }

        [HostType("VS IDE")]
        [TestMethod]  // [Test]
        public void SolutionMonitor_StartupTest()
        {
            // To test startup features of SrcML.NET
            System.Threading.Thread.Sleep(10000);
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

        [TestCleanup]  // [TearDown] (TearDown for Unit Test)
        public void TearDown()
        {
        }
        
        [ClassCleanup]
        public static void TestCleanup()
        {
            // Stop monitoring
            sm.StopMonitoring();
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

        /// <summary>
        /// For debugging.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="str"></param>
        private static void writeLog(string logFile, string str)
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
                                writeLog("D:\\Data\\log.txt", "ProjectItem to be added under folder: [" + item.Name + "]");
                                item.ProjectItems.AddFromFileCopy("D:\\Data\\Class111.c");
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
                writeLog("D:\\Data\\log.txt", "ProjectItem to be saved: [" + projectItem.Name + "]");
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
                writeLog("D:\\Data\\log.txt", "ProjectItem to be saveas-ed: [" + projectItem.Name + "]");
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
            writeLog("D:\\Data\\log.txt", "DeleteProjectItems(): [" + fileName + "]");
            var projectItem = ModelSolution.FindProjectItem(fileName);
            if (projectItem != null)
            {
                writeLog("D:\\Data\\log.txt", "ProjectItem to be permanantly deleted: [" + projectItem.Name + "]");
                projectItem.Delete();   // File being deleted from the file system
                //projectItem.Remove();   // File not being deleted from the file system, just removed from VS Solution Explorer
            }
        }
    }
}
