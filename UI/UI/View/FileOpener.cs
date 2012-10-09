using System;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Core.Extensions;
using Sando.UI.InterleavingExperiment;

namespace Sando.UI.View
{
		public static class FileOpener
    	{
			
			private static DTE2 dte = null;

    		public static void OpenItem(object sender)
    		{
    			var result = sender as ListBoxItem;
    			if(result != null)
    			{
					var myResult = result.Content as CodeSearchResult;
    				OpenFile(myResult.Element.FullFilePath, myResult.Element.DefinitionLineNumber);

                    if (ExtensionPointsRepository.IsInterleavingExperimentOn)
                    {
                        InterleavingExperimentManager.Instance.NotifyClicked(myResult);
                    }
    			}
    		}

    		public static void OpenFile(string filePath, int lineNumber)
    		{
    			InitDte2();
    			dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindTextView);
    			try
    			{
    				var selection = (TextSelection) dte.ActiveDocument.Selection;
    				selection.GotoLine(lineNumber);
    				selection.SelectLine();
    			}
    			catch (Exception)
    			{
    				//ignore, we don't want this feature ever causing a crash
    			}
    		}    	

    		private static void InitDte2()
    		{
    			if (dte == null)
    			{
    				dte = Package.GetGlobalService(typeof (DTE)) as DTE2;
    			}
    		}
    	}

}
