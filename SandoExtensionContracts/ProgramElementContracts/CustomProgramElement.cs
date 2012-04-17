using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
    public abstract class CustomProgramElement: ProgramElement
    {
        protected CustomProgramElement(string name, int definitionLineNumber, string fullFilePath, string snippet) : base(name, definitionLineNumber, fullFilePath, snippet)
        {
        }

        public override ProgramElementType ProgramElementType
        {
            get { return ProgramElementType.Custom; }
        }
    }
}
