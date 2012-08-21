using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.Diagnostics;
using System.Threading;


namespace Sando.Core.Extensions.PairedInterleaving
{
    public static class LexSearch
    {
        private static DTE2 _dte = null;
        private static AutoResetEvent _auto = new AutoResetEvent(false);
        private static string _selectionText = String.Empty;

        //has to be at class level, http://support.microsoft.com/kb/555430
        private static FindEvents _findEvents = null;

        public static List<CodeSearchResult> GetResults(string query)
        {
            InitDte2();

            _findEvents = _dte.Events.FindEvents;
            _findEvents.FindDone += LexSearch.OnFindDone;
            Find objFind = _dte.Find;
            objFind.FindReplace(vsFindAction.vsFindActionFindAll, query, 0, "",
                                vsFindTarget.vsFindTargetSolution, "", "",
                                vsFindResultsLocation.vsFindResults1);

            _auto.WaitOne();
            _findEvents.FindDone -= OnFindDone;
      
            return ParseFindInFilesText(_selectionText);
        }

        private static List<CodeSearchResult> ParseFindInFilesText(string text)
        {
            _selectionText = String.Empty;
            return null;
        }
     
        private static void OnFindDone(vsFindResult result, bool cancelled) 
        {
            if (result == vsFindResult.vsFindResultFound)
            {
                string vsWindowKindFindResults1 = "{0F887920-C2B6-11D2-9375-0080C747D9A0}";
                Window resultsWin = _dte.Windows.Item(vsWindowKindFindResults1);
                TextSelection selection = resultsWin.Selection;
                selection.SelectAll();
                _selectionText = selection.Text;
                resultsWin.Visible = false;
            }
            _auto.Set();
        }
        
        private static void InitDte2()
        {
            if (_dte == null)
            {
                _dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            }
        }
    }
}
