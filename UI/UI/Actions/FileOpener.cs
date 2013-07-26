using System;
using EnvDTE;
using EnvDTE80;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Core.Logging.Events;
using Sando.Core.Tools;
using System.Collections.Generic;
using Sando.Indexer.Searching;
using Microsoft.VisualStudio.Shell;

namespace Sando.UI.Actions
{
    public static class FileOpener
    {
        private static DTE2 _dte;

        public static void OpenItem(CodeSearchResult result, string text)
        {
            if (result != null)
            {
                OpenFile(result.ProgramElement.FullFilePath, result.ProgramElement.DefinitionLineNumber, text);
            }
        }

        public static void OpenFile(string filePath, int lineNumber, string text)
        {
            InitDte2();
            _dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindTextView);
            try
            {
                var selection = (TextSelection)_dte.ActiveDocument.Selection;
                selection.GotoLine(lineNumber);                              
            }
            catch (Exception e)
            {
                    LogEvents.UIOpenFileError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, e);
                //ignore, we don't want this feature ever causing a crash
            }
        }


        private static bool Is2012()
        {
            EnvDTE.DTE dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            if (dte.Version.Contains("11.0"))
                return true;
            return false;
        }

        private static void InitDte2()
        {
            if (_dte == null)
            {
                _dte = ServiceLocator.Resolve<DTE2>();
            }
        }
    }

}
