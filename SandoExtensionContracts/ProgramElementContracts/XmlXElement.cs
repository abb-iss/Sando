using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.ExtensionContracts.ProgramElementContracts
{
    public class XmlXElement : ProgramElement
    {
        public XmlXElement(string name, string body, int definitionLineNumber, int definitionColumnNumber, 
            string fullFilePath, string snippet) : 
            base(name, definitionLineNumber, definitionColumnNumber, fullFilePath, snippet)
        {
            this.Body = body;
        }

        public override ProgramElementType ProgramElementType
        {
            get
            {
                return ProgramElementType.XmlElement;
            }
        }

        public String Body { 
            get;
            private set;
        }
    }
}
