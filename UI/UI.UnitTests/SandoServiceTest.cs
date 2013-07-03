using System;
//using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sando.UI;
using System.Windows.Forms;
using Sando.UI.Service;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VSSDK.Tools.VsIdeTesting;

using System.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using System.IO;

namespace Sando.IntegrationTests
{
    [TestClass]
    public class SandoServiceTest
    {
        private static IVsPackage package;
        private static ISandoGlobalService sandoService;

        private delegate void ThreadInvoker();

        [HostType("VS IDE")]
        [AssemblyInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // Create SandoServicePackage
            UIPackage packageObject = new UIPackage();
            var package = (IVsPackage)packageObject;
            Assert.IsNotNull(package, "Get a null UIPackage instance.");
            
            var context = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Assert.IsNotNull(context, "GetService returned null for the global service.");


            //Assert.IsTrue(false, Directory.GetCurrentDirectory());
            var solution = VsIdeTestHostContext.Dte.Solution;
            solution.Open(@"..\..\..\..\UI\UI.UnitTests\TestFiles\tictactoeproject\TicTacToe.sln");

            Thread.Sleep(8000);

            IServiceProvider serviceProvider = package as IServiceProvider;
            // Get Sando Service
  
            object o = serviceProvider.GetService(typeof(SSandoGlobalService));
            Assert.IsNotNull(o, "GetService returned null for the global service.");
            sandoService = o as ISandoGlobalService;
            Assert.IsNotNull(sandoService, "The service SSandoGlobalService does not implements ISandoGlobalService.");            
  

            Thread.Sleep(1000);
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void NullTest()
        {
            //pass me           
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            sandoService = null;
        }
    }
}
