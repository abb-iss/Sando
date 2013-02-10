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

namespace Sando.UI
{
    public class PathManager
    {
        private static PathManager _instance;
        private static bool _initialized = false;
        private string _pathToExtensionRoot;

        public PathManager(string pathToExtensionRoot)
        {
            this._pathToExtensionRoot = pathToExtensionRoot;
        }

        public static PathManager Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (!_initialized)                    
                {
                    _instance = Setup();
                    _initialized = true;
                }
                return _instance;
            }
        }

        private static PathManager Setup()
        {
            try
            {

                 IVsExtensionManager extensionManager = ServiceProvider.GlobalProvider.GetService(typeof(SVsExtensionManager)) as IVsExtensionManager;
                 var pathToExtensionRoot = extensionManager.GetInstalledExtension("7e03caf3-06ed-4ff5-962a-effa1fb2f383").InstallPath;
                return new PathManager(pathToExtensionRoot);
            }catch( Exception e){
                //for when just running unit tests
                return new PathManager(Assembly.GetExecutingAssembly().Location);
            }
        }

        public string GetExtensionRoot()
        {
            return _pathToExtensionRoot;
        }

    }
}
