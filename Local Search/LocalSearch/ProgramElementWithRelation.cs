using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace LocalSearch
{
    public class ProgramElementWithRelation : ProgramElement
    {
        public ProgramElementWithRelation(string name, int definitionLineNumber, string fullFilePath,
            string snippet, ProgramElementRelation elementrelation)
            : base(name, definitionLineNumber, fullFilePath, snippet)
        {
            ProgramElementRelation = elementrelation;
        }

        public virtual ProgramElementRelation ProgramElementRelation { get; set; }
    }
}
