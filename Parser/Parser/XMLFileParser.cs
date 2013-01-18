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
    public class XMLFileParser: IParser 
    {
        public List<ProgramElement> Parse(string filename)
        {
            var programElements = new List<ProgramElement>();

        	XmlTextReader reader = new XmlTextReader(filename);

			while(reader.Read())
			{
				string text = String.Empty;

				if(reader.NodeType == XmlNodeType.Text)
				{
					text = reader.Value;
				}
				else if(reader.NodeType == XmlNodeType.Element)
				{
					while(reader.MoveToNextAttribute())
					{
						text += reader.Value + " ";
					}
				}

				if(! String.IsNullOrWhiteSpace(text))
				{
					var cleanedText = text.TrimStart(' ', '\n', '\r', '\t');
					cleanedText = cleanedText.TrimEnd(' ', '\n', '\r', '\t');
					var linenum = reader.LineNumber;
					var source = SrcMLParsingUtils.RetrieveSource(cleanedText);
					var pe = new TextLineElement(cleanedText, linenum, filename, source, cleanedText);
					programElements.Add(pe);
				}
			}


			return programElements;
        }
    }
}
