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

                var literal = WordSplitter.IsLiteralSearchString(text);
                if (literal != null)
                    FocusOnLiteralString(literal);
                else
                    HighlightLine(lineNumber);
            }
            catch (Exception e)
            {
                    LogEvents.UIOpenFileError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, e);
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
                    objSel.MoveToLineAndOffset(Convert.ToInt32(lStartLine), Convert.ToInt32(lStartColumn + term.Length), true);
                }

            }
        }



        private static void FocusOnLiteralString(string text)
        {
            const char chars = '"';
            bool isQuoted = text.StartsWith("\"") && text.EndsWith("\"");
            text = text.TrimStart(chars);
            text = text.TrimEnd(chars);
            var objSel = (TextSelection)(_dte.ActiveDocument.Selection);
            TextRanges textRanges = null;
            if (text.Contains("*") && !isQuoted)
            {
                text = LuceneQueryStringBuilder.EscapeChars(text);
                if (Is2012())
                {
                    text = text.Replace("*", ".*");
                    objSel.FindPattern(text, 8, ref textRanges);
                }
                else
                    objSel.FindPattern(text, 1024, ref textRanges);
            }
            else
                objSel.FindPattern(text, 0, ref textRanges);
            objSel.SelectLine();
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
