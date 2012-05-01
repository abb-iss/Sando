using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ParserExtensions
{
    public class TextFileElement : ProgramElement
    {
        public TextFileElement(string name, int definitionLineNumber, string fullFilePath, string snippet) : base(name, definitionLineNumber, fullFilePath, snippet)
        {
        }

        public String Body { get; set; }

        public override ProgramElementType ProgramElementType
        {
            get { return ProgramElementType.Custom; }
        }

        public override string GetName()
        {
            return "Text File";
        }
    }
}
