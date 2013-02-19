using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;
using ABB.SrcML;

namespace Sando.Parser.UnitTests
{
    public class ParserTestingUtils
    {
        public static List<ProgramElement> ParseCsharpFile(string filePath) {
            SrcMLCSharpParser parser = new SrcMLCSharpParser(new ABB.SrcML.SrcMLGenerator(@"LIBS\SrcML"));
            var elements = parser.Parse(filePath);
            return elements;
        }

        public static MethodElement GetMethod(string name, List<ProgramElement> elements )
        {
            foreach (var programElement in elements)
            {
                if(programElement as MethodElement !=null)
                if ((programElement as MethodElement).Name.Equals(name))
                    return programElement as MethodElement;
            }
            return null;
        }

        public static MethodElement GetMethod(string filepath, string name)
        {
            return GetMethod(name,ParseCsharpFile(filepath));
        }
    }
}
