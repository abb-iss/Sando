using System;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.View
{
		public static class FileOpener
    	{
			
			private static DTE2 dte = null;

    		public static void OpenItem(object sender,string text)
    		{
    			var result = sender as ListBoxItem;
    			if(result != null)
    			{
					var myResult = result.Content as CodeSearchResult;
    				OpenFile(myResult.Element.FullFilePath, myResult.Element.DefinitionLineNumber, text);
    			}
    		}

    		public static void OpenFile(string filePath, int lineNumber, string text)
    		{
    			InitDte2();
    			dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindTextView);
    			try
    			{
    				var selection = (TextSelection) dte.ActiveDocument.Selection;       
             
    				selection.GotoLine(lineNumber);
                    if(text.Contains("\"")){
                        var chars = '"';
                        text = text.TrimStart(chars);
                        text = text.TrimEnd(chars);
                        TextSelection objSel = (EnvDTE.TextSelection)(dte.ActiveDocument.Selection);

                        EnvDTE.TextRanges textRanges = null;

                        // Advance to the next Visual Basic function beginning or end by 
                        // searching for  "Sub" with white space before and after it.
                        objSel.FindPattern(text, 0, ref textRanges);
                        {
                            //  Select the entire line.
                            objSel.SelectLine();
                        }                    
                    }
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
