using System;
//using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sando.UI;
using System.Windows.Forms;
using Sando.Service;
using Microsoft.VisualStudio.Shell.Interop;

namespace Sando.IntegrationTests
{
    [TestClass]
    public class SandoServiceTest
    {
        private static IVsPackage package;
        private static ISandoGlobalService sandoService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // Create SandoServicePackage
            UIPackage packageObject = new UIPackage();
            var package = (IVsPackage)packageObject;
            Assert.IsNotNull(package, "Get a null UIPackage instance.");

            IServiceProvider serviceProvider = package as IServiceProvider;
            // Get Sando Service
            object o = serviceProvider.GetService(typeof(SSandoGlobalService));
            Assert.IsNotNull(o, "GetService returned null for the global service.");

            sandoService = o as ISandoGlobalService;
            Assert.IsNotNull(sandoService, "The service SSandoGlobalService does not implements ISandoGlobalService.");

        }

        [TestMethod]
        [HostType("VS IDE")]
        public void TestMethod1()
        {
            MessageBox.Show("hi");
           
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            sandoService = null;
            package.SetSite(null);
            package.Close();
            package = null;
        }
    }
}
