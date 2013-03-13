using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reflection;
using System.IO;
using Sando.Core.Tools;

namespace Sando.Core.Tools
{
    public class PathManager
    {
        private static PathManager _instance;       
        private string _pathToExtensionRoot;

        public static void Create(string path)
        {
            if(Path.HasExtension(path))
                new PathManager(Path.GetDirectoryName(path));
            else
                new PathManager(path);
        }

        private PathManager(string pathToExtensionRoot)
        {
            this._pathToExtensionRoot = pathToExtensionRoot;
            _instance = this;
        }

        public static PathManager Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (_instance==null)                    
                {
                    throw new NotImplementedException();
                }
                return _instance;
            }
        }


        public string GetExtensionRoot()
        {
            return _pathToExtensionRoot;
        }

        public string GetIndexPath(ABB.SrcML.VisualStudio.SolutionMonitor.SolutionKey solutionKey)
        {            
            return LuceneDirectoryHelper.GetOrCreateLuceneDirectoryForSolution(solutionKey.GetSolutionPath(), PathManager.Instance.GetExtensionRoot());                
        }

        public bool IndexPathExists(ABB.SrcML.VisualStudio.SolutionMonitor.SolutionKey key)
        {
            return LuceneDirectoryHelper.
                DoesLuceneDirectoryForSolutionExist(key.GetSolutionPath(), PathManager.Instance.GetExtensionRoot());                
        }
    }
}
