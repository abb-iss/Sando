using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Sando.Core.Extensions.PairedInterleaving
{
    public class LexSearch
    {
        private static DTE2 dte = null;

        public void GetResults(string query)
        {
            InitDte2();
            
            FindEvents _findEvents = dte.Events.FindEvents;
            _findEvents.FindDone += new _dispFindEvents_FindDoneEventHandler(OnFindDone);

            TextDocument _objDoc = dte.ActiveSolutionProjects.Object();
            Find _objFind = _objDoc.DTE.Find;
            _objFind.Action = vsFindAction.vsFindActionFindAll;
            _objFind.FindWhat = query;
            _objFind.Execute();
        }

        void OnFindDone(vsFindResult result, bool cancelled) 
        {
            Console.Write("FindDone event fired with vsFindResult = ");
            Console.WriteLine(result.ToString());
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
