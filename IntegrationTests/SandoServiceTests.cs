using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sando.DependencyInjection;
using Sando.Recommender;
using Sando.UI.View;

namespace Sando.IntegrationTests
{
    [TestClass]
    public class SandoServiceTests : IInvoker
    {
        private const string TestSolutionName = "tictactoe";
        private static Solution TestSolution;
        private static object TestLock;

        private static string TestSolutionPath = Path.Combine(TestSolutionName, TestSolutionName + ".sln");

        [ClassInitialize]
        public static void ClassSetup(TestContext testContext)
        {
            // Create a local copy of the solution
            TestHelpers.CopyDirectory(Path.Combine(TestConstants.InputFolderPath, TestSolutionName), TestSolutionName);
            TestHelpers.CopyDirectory(TestHelpers.GetSolutionDirectory() + @"/LIBS/SrcML", "SrcML");
            TestHelpers.CopyDirectory(TestHelpers.GetSolutionDirectory() + @"/Core/Core/Dictionaries", "Dictionaries");
            TestHelpers.CopyDirectory(TestHelpers.GetSolutionDirectory() + @"/Recommender/Recommender/swum-data", "swum-data");
            File.Copy(TestHelpers.GetSolutionDirectory() + @"\LIBS\Swum.NET\ABB.Swum.dll.config", "ABB.Swum.dll.config");
            TestLock = new object();
        }

        [TestInitialize]
        public void Setup()
        {
            Invoke(StartupCompleted);            
            TestSolution = VsIdeTestHostContext.Dte.Solution;
            Assert.IsNotNull(TestSolution, "Could not get the solution");
            TestSolution.Open(Path.GetFullPath(TestSolutionPath));
            TestHelpers.SrcMLTestScaffold.Service.StartMonitoring();
        }

        private void StartupCompleted()
        {
            TestHelpers.StartupCompleted();
        }

        [TestCleanup]
        public void Cleanup()
        {            
            TestSolution.Close();
            TestHelpers.SrcMLTestScaffold.Service.StopMonitoring();
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void SearchTest()
        {
            Assert.IsTrue(TestHelpers.WaitForServiceToFinish(TestHelpers.SrcMLTestScaffold.Service, 5000));
            try
            {
                var results = TestHelpers.TestScaffold.Service.GetSearchResults("game pad");
                Assert.IsTrue(results.Count > 0, "Did not find any results when I should have");     
                Assert.IsTrue(results.First().Name.Equals("GamePadNode"),"Didn't find fthe correct first result, found: "+results.First().Name);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, "Search failed with an exception: " + e.Message);
            }            
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void RecommendationsTest()
        {
            Assert.IsTrue(TestHelpers.WaitForServiceToFinish(TestHelpers.SrcMLTestScaffold.Service, 5000));
            try
            {
                var recommender = ServiceLocator.Resolve<QueryRecommender>();
                Assert.IsTrue(recommender.GenerateRecommendations("play").Length > 0, "Did not find any recommendations when I should have");
                Assert.IsTrue(recommender.GenerateRecommendations("play")[0].Query.Equals("Player"), "Didn't find the correct first result, found: " + recommender.GenerateRecommendations("game")[0].Query);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, "Search failed with an exception: " + e.Message);
            }
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void SummarizationTest()
        {
            Assert.IsTrue(TestHelpers.WaitForServiceToFinish(TestHelpers.SrcMLTestScaffold.Service, 10000));
            var firstResult = TestHelpers.TestScaffold.Service.GetSearchResults("pc player game").First();            
            var control = ServiceLocator.Resolve<SearchViewControl>();
            string highlight;
            string highlightRaw;
            control.GenerateHighlight(firstResult.Raw, "play", out highlight, out highlightRaw);
            //        public PCPlayer PCPlayer
            //Assert.IsTrue(false, highlight);
            Assert.IsTrue(highlight.Contains("public PC|~S~|Play|~E~|er PC|~S~|Play|~E~|er"));
            Assert.IsFalse(highlight.Contains("get"));
            //                return pcPlayer;
            Assert.IsTrue(highlight.Contains("return pc|~S~|Play|~E~|er;")); 
        }

        public void Invoke(MethodInvoker globalSystemWindowsFormsMethodInvoker)
        {
            UIThreadInvoker.Invoke(globalSystemWindowsFormsMethodInvoker);
        }
    }
}