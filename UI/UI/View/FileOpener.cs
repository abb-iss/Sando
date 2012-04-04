using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.SearchEngine;
using Sando.UI.Model;

namespace Sando.UI.View
{
		public static class FileOpener
    	{
			
			private static DTE2 dte = null;

    		public static void OpenItem(object sender)
    		{
    			var result = sender as ListBox;
    			if(result != null)
    			{
					var myResult = result.SelectedItem as CodeSearchResult;
    				OpenFile(myResult.Element.FullFilePath, myResult.Element.DefinitionLineNumber);
    			}
    		}

    		public static void OpenFile(string filePath, int lineNumber)
    		{
    			InitDte2();
    			dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindCode);
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
