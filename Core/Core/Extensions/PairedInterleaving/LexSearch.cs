using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.Diagnostics;

namespace Sando.Core.Extensions.PairedInterleaving
{
    public static class LexSearch
    {
        private static DTE2 dte = null;

        public static List<CodeSearchResult> GetResults(string query)
        {
            InitDte2();
                       
            dte.Events.FindEvents.FindDone += new _dispFindEvents_FindDoneEventHandler(OnFindDone);
            Find _objFind = dte.Find;
            vsFindResult _findResult = _objFind.FindReplace(vsFindAction.vsFindActionFindAll, query, 0, "", 
                                                            vsFindTarget.vsFindTargetSolution, "", "", 
                                                            vsFindResultsLocation.vsFindResults1);

            string _selectionText = "";
            return ParseFindInFilesText(_selectionText);
        }

        private static List<CodeSearchResult> ParseFindInFilesText(string text)
        {
            return null;
        }

        private static void OnFindDone(vsFindResult result, bool cancelled) 
        {
            if (result == vsFindResult.vsFindResultFound)
            {
                string vsWindowKindFindResults1 = "{0F887920-C2B6-11D2-9375-0080C747D9A0}";
                Window _resultsWin = dte.Windows.Item(vsWindowKindFindResults1);
                TextSelection _selection = _resultsWin.Selection;
                _selection.SelectAll();
                string _selectionText = _selection.Text;
                _resultsWin.Visible = false;
            }
            //dte.Events.FindEvents.FindDone -= OnFindDone;
        }

        private static void InitDte2()
        {
            if (dte == null)
            {
                dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            }
        }
    }
}
