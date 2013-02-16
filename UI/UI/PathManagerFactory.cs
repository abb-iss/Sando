using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reflection;
using System.IO;
using Sando.Core.Tools;

namespace Sando.UI
{
    public class PathManagerFactory
    {
       
        public static void Setup()
        {
            try
            {
                 IVsExtensionManager extensionManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsExtensionManager)) as IVsExtensionManager;
                 var pathToExtensionRoot = extensionManager.GetInstalledExtension("7e03caf3-06ed-4ff5-962a-effa1fb2f383").InstallPath;
                 PathManager.Create(pathToExtensionRoot);
            }catch( Exception e){
                //for when just running unit tests
                PathManager.Create(Assembly.GetExecutingAssembly().Location);
            }
        }

    }
}
