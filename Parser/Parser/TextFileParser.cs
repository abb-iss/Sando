using System;
using System.Collections.Generic;
using System.IO;
using Sando.Core.Logging.Events;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Parser
{
    public class TextFileParser : IParser
    {               
        public List<ProgramElement> Parse(string filename)
        {
            if (File.Exists(filename) && GetSizeInMb(filename) > 15)
            {
                return new List<ProgramElement>();
            }
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
                        //var name = Regex.Replace(line, @"(\w+)\W+", "$1 ");
                        var name = line.TrimStart(' ', '\n', '\r', '\t');
                        name = name.TrimEnd(' ');
                        var snippet = SrcMLParsingUtils.RetrieveSource(name);
                        var element = new TextLineElement(name, linenum, filename, snippet, line);
                        list.Add(element);
                    }

                }
            }
            catch (Exception e)
            {
                LogEvents.ParsingFileGenericError(this, filename);
            }
            return list;
        }

        private float GetSizeInMb(string filename)
        {
            float sizeInMb = (new FileInfo(filename).Length / 1024f) / 1024f;
            return sizeInMb;
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
