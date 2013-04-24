using System;
using EnvDTE;
using EnvDTE80;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Core.Extensions.Logging;
using Sando.Core.Tools;
using System.Collections.Generic;

namespace Sando.UI.Actions
{
		public static class FileOpener
    	{
			private static DTE2 _dte;

            public static void OpenItem(CodeSearchResult result, string text)
    		{
    			if(result != null)
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
    				var selection = (TextSelection) _dte.ActiveDocument.Selection;                    
    				selection.GotoLine(lineNumber);

                    var literal = WordSplitter.IsLiteralSearchString(text);
                    if (literal!=null)
                        FocusOnLiteralString(literal);
                    else
                        HighlightLine(lineNumber);
    			}
    			catch (Exception e)
    			{
                    FileLogger.DefaultLogger.Error(e);
    				//ignore, we don't want this feature ever causing a crash
    			}
    		}

            private static void HighlightLine(int lineNumber)
            {
                var objSel = (TextSelection)(_dte.ActiveDocument.Selection);
                objSel.MoveToLineAndOffset(System.Convert.ToInt32
                    (lineNumber), System.Convert.ToInt32(1), true);
                objSel.SelectLine();
            }

            private static void HighlightTerms(string text)
            {
                var terms = text.Split(' ');
                foreach (var term in terms)
                {
                    var objSel = (TextSelection)(_dte.ActiveDocument.Selection);
                    TextRanges textRanges = null;
                    objSel.FindPattern(term, 0, ref textRanges);
                    {
                        long lStartLine = objSel.TopPoint.Line;
                        long lStartColumn = objSel.TopPoint.LineCharOffset;                        
                        objSel.SwapAnchor();
                        objSel.MoveToLineAndOffset(Convert.ToInt32(lStartLine), Convert.ToInt32(lStartColumn+term.Length), true);                                                
                    }

                }
            }



            private static void FocusOnLiteralString(string text) 
            {
                const char chars = '"';
                text = text.TrimStart(chars);
                text = text.TrimEnd(chars);
                var objSel = (TextSelection)(_dte.ActiveDocument.Selection);
                TextRanges textRanges = null;
                if(text.Contains("*"))                
                    objSel.FindPattern(text, 1024, ref textRanges);                                                                
                else
                    objSel.FindPattern(text, 0, ref textRanges);                
                objSel.SelectLine();             
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
