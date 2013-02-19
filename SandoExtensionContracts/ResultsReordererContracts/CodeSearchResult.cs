using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sando.ExtensionContracts.ProgramElementContracts;
using System;

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

        public double Score { get; set; }

        public ProgramElement ProgramElement { get; private set; }

        public string ParentOrFile
        {
            get
            {
                if (string.IsNullOrEmpty(Parent))
                    return Path.GetFileName(FileName);

                var fileName = Path.GetFileName(FileName);
                if (fileName.StartsWith(Parent))
                {
                    return fileName;
                }
                return Parent + " (" + fileName + ")";
            }
        }

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

        public static string SourceToSnippet(string source, int numLines)
        {
            var toRemove = int.MaxValue;
            var lines = new List<string>(source.Split('\n'));
            if (numLines < lines.Count)
            {
                lines.RemoveRange(numLines, lines.Count - numLines);
            }

            int perLineToRemove;

            if (source.StartsWith("\t"))
            {
                perLineToRemove = source.Length - source.TrimStart('\t').Length;
            }
            else if (source.StartsWith(" "))
            {
                perLineToRemove = source.Length - source.TrimStart(' ').Length;
            }
            else
            {
                perLineToRemove = 0;
            }

            if (perLineToRemove < toRemove)
            {
                toRemove = perLineToRemove;
            }

            //remove the empty spaces in front
            if (toRemove > 0 && toRemove < int.MaxValue)
            {
                string newSnip = "";
                foreach (string line in lines)
                {
                    if (line.Length > toRemove + 1)
                        newSnip += line.Remove(0, toRemove) + "\n";
                }
                return newSnip;
            }

            return String.Join("\n", lines.ToArray());
        }

        public string FileName
        {
            get { return Path.GetFileName(ProgramElement.FullFilePath); }
        }

        public int DefinitionLineNumber
        {
            get;
            set;
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
    }
}
