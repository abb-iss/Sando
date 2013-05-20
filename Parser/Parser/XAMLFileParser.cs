using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
    public class XAMLFileParser : IParser
    {
        public List<ProgramElement> Parse(string filename)
        {
            var allText = File.ReadAllText(filename);
            XDocument doc = XDocument.Parse(allText, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
            var allXElements = doc.Elements().SelectMany(e => e.DescendantsAndSelf());
            return allXElements.Select(e => new ProgramElement(getName(e), getLineNumber(e), filename, 
                getSnippet(e))).ToList();
        }

        String getName(XElement element)
        {
            return element.Name.LocalName;
        }

        int getLineNumber(XElement element)
        {
            var lineInfo = element as IXmlLineInfo;
            if (lineInfo != null)
            {
                return lineInfo.LineNumber;
            }
            throw new Exception("Cannot get line number.");
        }

        String getSnippet(XElement element)
        {
            var sb = new StringBuilder();
            sb.AppendLine(element.Name.LocalName);
            foreach(var att in element.Attributes())
            {
                sb.AppendLine(att.Name.LocalName + " = " + att.Value);
            }
            return sb.ToString();
        }

        public List<ProgramElement> Parse(string filename, System.Xml.Linq.XElement sourceElements)
        {
            return Parse(filename);
        }
    }
}
