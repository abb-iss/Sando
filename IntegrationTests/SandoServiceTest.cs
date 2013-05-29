using System;
//using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sando.UI;

namespace Sando.IntegrationTests
{
    [TestClass]
    public class SandoServiceTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // Create SrcMLServicePackage
            //UIPackage packageObject = new UIPackage();
            //var package = (IVsPackage)packageObject;
            //Assert.IsNotNull(package, "Get a null UIPackage instance.");

            //IServiceProvider serviceProvider = package as IServiceProvider;
            // Get SrcML Service
            //object o = serviceProvider.GetService(typeof(SSrcMLGlobalService));
            //Assert.IsNotNull(o, "GetService returned null for the global service.");
            //srcMLService = o as ISrcMLGlobalService;
            //Assert.IsNotNull(srcMLService, "The service SSrcMLGlobalService does not implements ISrcMLGlobalService.");

        }

        [TestMethod]
        [HostType("VS IDE")]
        public void TestMethod1()
        {
            //Console.WriteLine("");
        }
    }
}
