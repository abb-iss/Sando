using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sando.ExtensionContracts.ProgramElementContracts;
using System;
using System.Text;

namespace Sando.ExtensionContracts.ResultsReordererContracts
{
    /// <summary>
    /// Class defined to create return result from Lucene indexer
    /// </summary>
    public class CodeSearchResult
    {
        public CodeSearchResult(ProgramElement programElement, double score)
        {
            ProgramElement = programElement;
            Score = score;
        }

        public double Score { get; private set; }

        public ProgramElement ProgramElement { get; private set; }

        public string ParentOrFile
        {
            get 
            {
                //NOTE: shortening is happening in this UI class instead of in the xaml because of xaml's limitations around controling column width inside of a listviewitem
                var parentOrFile = "";
                if (string.IsNullOrEmpty(Parent))
                    parentOrFile = Path.GetFileName(FileName);
                else
                {
                    var fileName = Path.GetFileName(FileName);
                    if (fileName.StartsWith(Parent))
                    {
                        parentOrFile = fileName;
                    }
                    else
                    {
                        parentOrFile = Parent + " (" + fileName + ")";
                    }
                }
                if (parentOrFile.Length > MAX_PARENT_LENGTH)
                {
                    int lengthAfterTrim = MAX_PARENT_LENGTH + RoomLeftFromName();
                    if (parentOrFile.Length > lengthAfterTrim)
                        return parentOrFile.Substring(0, lengthAfterTrim) + "...";   
                }
                return parentOrFile;
            }
        }

        private int RoomLeftFromName()
        {
            if (Name.Length > MAX_PARENT_LENGTH - 10)
                return 0;
            else
                return MAX_PARENT_LENGTH - 10 - Name.Length;
        }

        private const int MAX_PARENT_LENGTH = 33;

        public ProgramElementType ProgramElementType
        {
            get { return ProgramElement.ProgramElementType; }
        }

        public string Type
        {
            get { return ProgramElement.GetName(); }
        }

        public static readonly int DefaultSnippetSize = 5;

        public string Snippet
        {
            get
            {
                var raw = ProgramElement.RawSource;
                return SourceToSnippet(raw, DefaultSnippetSize);
            }
        }

        //Zhao Li, return all the RawSource to the Popup window
        //We need to highlight the search string?
        public string Raw {
            get {
                return ProgramElement.RawSource.Replace('\t', ' ');
            }
        }

        //Highlight raw
        private string highlightRaw;
        public string HighlightRaw {
            get {
                return this.highlightRaw;
            }
            set {
                this.highlightRaw = value;
            }
        }
        //Zhao Li, return all the seach results
        private string highlight;
        public string Highlight {
            get {
                return this.highlight;
            }
            set {
                this.highlight = value;
            }
        }

        private static int TAB = 4;
        private static int MAX_SNIPPET_LENGTH = 85;        

        public string SourceToSnippet(string source, int numLines)
        {            
            if (IsXml())
            {         
                source = PrettyPrintXElement(source, numLines);
            }
            //NOTE: shortening is happening in this UI class instead of in the xaml because of xaml's limitations around controling column width inside of a listviewitem            
            var lines = GetLines(source);
            int leadingSpaces = 0;
            var shortenedLineList = ShortenSnippet(numLines, lines);
            
            if (!IsXml())
            {
                leadingSpaces = GetLeadingSpaces(lines);
                shortenedLineList = StandardizeLeadingWhitespace(shortenedLineList, numLines);
            }
            StringBuilder snippet = AddTruncatedLinesToSnippet(shortenedLineList, leadingSpaces);
            return snippet.ToString();
        }

        private static List<string> GetLines(string source)
        {
            var lines = new List<string>(source.Split('\n'));
            return lines;
        }

        private bool IsXml()
        {
            if(this.FileName!=null && (FileName.EndsWith(".xaml")||FileName.EndsWith(".xml")))
                return true;
            else
                return false;
        }

        private static StringBuilder AddTruncatedLinesToSnippet(List<string> lines, int leadingSpaces)
        {
            StringBuilder snippet = new StringBuilder();
            foreach (var aLine in lines)
            {
                try
                {
                    if (aLine.Substring(0, leadingSpaces).Trim().Equals(""))
                        Append(snippet, aLine.Substring(leadingSpaces));
                    else
                        Append(snippet, aLine);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Append(snippet, aLine);
                }
            }
            return snippet;
        }

        private static List<string> StandardizeLeadingWhitespace(List<string> lines, int numLines)
        {
            var newLines = new List<string>();
            int count = 0;
            var line = "";
            foreach (var aLine in lines)
            {
                line = aLine;
                if (!line.Trim().Equals(""))
                {
                    while (line.StartsWith(" ") || line.StartsWith("\t"))
                    {
                        if (line.StartsWith(" "))
                            count++;
                        else
                            count = count + TAB;
                        line = line.Substring(1);
                    }
                    string newLine = "";
                    for (int i = 0; i < count; i++)
                        newLine += " ";
                    newLine += line + "\n";
                    newLines.Add(newLine);
                }
                count = 0;
            }
            return newLines;
        }

        private static List<string> ShortenSnippet(int numLines, List<string> lines)
        {
            if (numLines < lines.Count)
            {
                return lines.GetRange(0, numLines);
            }
            else
            {
                return lines.GetRange(0,lines.Count);
            }
        }

        private static int GetLeadingSpaces(List<string> lines)
        {
            if (lines.Count > 0)
            {
                var lastLine = lines.Last();
                if(lastLine.Trim().Equals(String.Empty))
                {
                    if (lines.Count > 2)
                    {
                        lastLine = lines.ElementAt(lines.Count - 2);
                    }
                }
                int count = 0;
                while (lastLine.StartsWith(" ") || lastLine.StartsWith("\t"))
                {
                    if (lastLine.StartsWith(" "))
                        count++;
                    else
                        count = count + TAB;
                    lastLine = lastLine.Substring(1);
                }
                return count;
            }
            return 0;
        }
           
        private static void Append(StringBuilder snippet, string p)
        {
            if (p.Length < MAX_SNIPPET_LENGTH)
                snippet.Append(p);
            else
                snippet.Append(p.Substring(0,MAX_SNIPPET_LENGTH)+"..."+"\n");
        }

        public string FileName
        {
            get { return Path.GetFileName(ProgramElement.FullFilePath); }
        }

        public string Parent
        {
            get
            {
                var method = ProgramElement as MethodElement;
                return method != null ? method.ClassName : String.Empty;
            }
        }

        public string Name
        {
            get { return ProgramElement.Name; }
        }


        private static string PrettyPrintXElement(String source, int numLines)
        {
            try
            {
                var prettyPrint = String.Empty;
                var doc = XDocument.Parse(source);
                prettyPrint = doc.ToString();               
                return prettyPrint;
            }
            catch (Exception e)
            {                
                return source;
            }
        }
    }
}
