using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ParserExtensions
{
    public class TextFileElement : CustomProgramElement
    {
        public TextFileElement(string name, int definitionLineNumber, string fullFilePath, string snippet) : base(name, definitionLineNumber, fullFilePath, snippet)
        {
        }

        public String Body { get; set; }
        public override string GetName()
        {
            return "Text File";
        }
    }
}
