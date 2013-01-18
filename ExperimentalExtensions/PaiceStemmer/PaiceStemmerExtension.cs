using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ParserContracts;
using Sando.Parser;

namespace Sando.ExperimentalExtensions.PaiceStemmer
{
	public class PaiceStemmerExtension : IParser, IQueryRewriter
	{
		string defaultRuleDir = Environment.CurrentDirectory + "\\..\\..\\LIBS\\paice";

		private PaiceStemmer paiceStemmer;

		public PaiceStemmerExtension()
		{
			paiceStemmer = new PaiceStemmer(defaultRuleDir, "");
		}

		public string RewriteQuery(string query)
		{
			return StemSentence(query);
		}

		public List<ProgramElement> Parse(string filename)
		{
			List<ProgramElement> newElements = new List<ProgramElement>();
			SrcMLCSharpParser csParser = new SrcMLCSharpParser();
			List<ProgramElement> elements = csParser.Parse(filename);
	
			foreach(ProgramElement element in elements) 
			{
				if(element is MethodElement)
				{
					MethodElement method = (MethodElement)element;
					newElements.Add(new MethodElement(method.Name, method.DefinitionLineNumber, method.FullFilePath, 
											method.Snippet, method.AccessLevel, method.Arguments, method.ReturnType, 
											StemSentence(method.Body), method.ClassId, method.ClassName, 
											method.Modifiers, method.IsConstructor));
				}
				else
				{
					newElements.Add(element);
				}
			}

			return newElements;
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
            writeLog("D:\\Data\\log.txt", "PaiceStemmerExtension.Parse(): " + fileName);
            return Parse(fileName);
        }

        /// <summary>
        /// For debugging.
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="str"></param>
        private void writeLog(string logFile, string str)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.ASCII);
            sw.WriteLine(str);
            sw.Close();
        }
        // End of code changes

		private string StemSentence(string sentence) 
		{
			string newSentence = String.Empty;
			char[] delimiters = new char[] { ' ', ',', '.' };
			string[] splitSentence = sentence.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			foreach(var splice in splitSentence)
			{
				newSentence += paiceStemmer.stripAffixes(splice) + " ";
			}
			return newSentence.TrimEnd();
		}
	}
}
