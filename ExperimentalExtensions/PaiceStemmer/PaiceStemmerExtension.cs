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
		string defaultRuleDir = Environment.CurrentDirectory + "\\..\\..\\sando\\LIBS\\paice";

		private PaiceStemmer paiceStemmer;

		PaiceStemmerExtension()
		{
			paiceStemmer = new PaiceStemmer(defaultRuleDir,"");
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
					newElements.Add(new MethodElement(method.Name, method.DefinitionLineNumber, method.FullFilePath, method.Snippet, 
										method.AccessLevel, method.Arguments, method.ReturnType, StemSentence(method.Body), 
										method.ClassId, method.ClassName, method.Modifiers, method.IsConstructor));
				}
				else
				{
					newElements.Add(element);
				}
			}

			return newElements;
		}

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
