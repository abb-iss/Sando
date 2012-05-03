using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ParserExtensions
{
    public class TextFileParser: IParser 
    {
        public List<ProgramElement> Parse(string filename)
        {
            var list = new List<ProgramElement>(); 
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    var body = new StringBuilder();
                    var snippet = new StringBuilder();

                    String line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if(i<3)
                        {
                            snippet.Append(line + "\n");
                            body.Append(line + " ");
                        }
                        i++;
                    }
                    var element = new TextFileElement(Path.GetFileName(filename), 0, filename, snippet.ToString());
                    element.Body = body.ToString();                    
                    list.Add(element);                    
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
