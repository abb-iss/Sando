using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
    public class TextFileParser: IParser 
    {
        private static readonly int SnippetSize = 5;

        public List<ProgramElement> Parse(string filename)
        {
            var list = new List<ProgramElement>(); 
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (var sr = new StreamReader(filename))
                {
                    String line;
                	int linenum = 0;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                    	linenum++;
						if (String.IsNullOrWhiteSpace(line)) continue;
						var name = Regex.Replace(line, @"(\w+)\W+", "$1 ");
            			name = name.TrimStart('<', ' ', '\n', '\r', '\t', '/');
						name = name.TrimEnd(' ');
                    	var snippet = SrcMLParsingUtils.RetrieveSnippet(filename, linenum, SnippetSize); 
                    	var element = new TextLineElement(name, linenum, filename, snippet, line);
                    	list.Add(element);
                    }
    
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return list;
        }
    }
}
