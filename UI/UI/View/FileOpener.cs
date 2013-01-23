using System;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Core.Extensions.Logging;

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

                    if (IsLiteralSearchString(text))
                        FocusOnLiteralString(text);
                    else
                        HighlightTerms(text);
    			}
    			catch (Exception e)
    			{
                    FileLogger.DefaultLogger.Error(e);
    				//ignore, we don't want this feature ever causing a crash
    			}
    		}

            private static void HighlightTerms(string text)
            {
                var terms = text.Split(' ');
                foreach (var term in terms)
                {
                    TextSelection objSel = (EnvDTE.TextSelection)(dte.ActiveDocument.Selection);
                    EnvDTE.TextRanges textRanges = null;
                    objSel.FindPattern(term, 0, ref textRanges);
                    {
                        long lStartLine = objSel.TopPoint.Line;
                        long lStartColumn = objSel.TopPoint.LineCharOffset;                        
                        objSel.SwapAnchor();
                        objSel.MoveToLineAndOffset(System.Convert.ToInt32
                                (lStartLine), System.Convert.ToInt32(lStartColumn+term.Length), true);                                                
                    }

                }
            }

            private static bool IsLiteralSearchString(string text)
            {
                return text.Contains("\"");
            }

            private static void FocusOnLiteralString(string text) 
            {
                var chars = '"';
                text = text.TrimStart(chars);
                text = text.TrimEnd(chars);
                TextSelection objSel = (EnvDTE.TextSelection)(dte.ActiveDocument.Selection);
                EnvDTE.TextRanges textRanges = null;
                objSel.FindPattern(text, 0, ref textRanges);
                {
                    objSel.SelectLine();
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
