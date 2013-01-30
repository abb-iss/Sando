using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
    public class XMLFileParser : IParser
    {
        private static readonly int SnippetLinesAbove = 0;
        public List<ProgramElement> Parse(string filename)
        {
            var programElements = new List<ProgramElement>();

            XmlTextReader reader = new XmlTextReader(filename);

            while (reader.Read())
            {
                string text = String.Empty;

                if (reader.NodeType == XmlNodeType.Text)
                {
                    text = reader.Value;
                }
                else if (reader.NodeType == XmlNodeType.Element)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        text += reader.Value + " ";
                    }
                }

                if (!String.IsNullOrWhiteSpace(text))
                {
                    var cleanedText = text.TrimStart(' ', '\n', '\r', '\t');
                    cleanedText = cleanedText.TrimEnd(' ', '\n', '\r', '\t');
                    var linenum = reader.LineNumber;
                    var snippet = SrcMLParsingUtils.RetrieveSource(cleanedText);
                    var pe = new TextLineElement(cleanedText, linenum, filename, snippet, cleanedText);
                    programElements.Add(pe);
                }
            }


            return programElements;
        }

        // Code changed by JZ: solution monitor integration
        /// <summary>
        /// New Parse method that takes two arguments, due to modification of IParser
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sourceElements"></param>
        /// <returns></returns>
        public List<ProgramElement> Parse(string fileName, System.Xml.Linq.XElement sourceElements)
        {
            return Parse(fileName);
        }
        // End of code changes
    }
}
