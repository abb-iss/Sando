using System.Collections.Generic;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.SplitterContracts;
using Sando.UI.InterleavingExperiment.Multiplexing.MuxProgramElements;

namespace Sando.UI.InterleavingExperiment.Multiplexing
{
	public class SplitterExperimentMultiplexer : IParser, IWordSplitter
	{
		public SplitterExperimentMultiplexer(IWordSplitter experimentalSplitter)
		{
			_splitter = experimentalSplitter;
		}

		public List<ProgramElement> Parse(string filename)
		{
			var allElements = new List<ProgramElement>();

            /*
			var regParser = _extensionsRepo.GetParserImplementation(System.IO.Path.GetExtension(filename));
			var regElements = regParser.Parse(filename);
			allElements.AddRange(regElements);

			//produce a Mux element for each type of element
			foreach(var element in regElements)
			{
                if (element is ClassElement)
                {
                    allElements.Add(new MuxClassElement(element as ClassElement));
                }
				else if(element is CommentElement)
				{
					allElements.Add(new MuxCommentElement(element as CommentElement));
				}
				else if(element is MethodElement)
				{
					allElements.Add(new MuxMethodElement(element as MethodElement));					
				}
			}
            */
			return allElements;
		}

		public string[] ExtractWords(string text)
		{
			//How do I know what type of program element this text came from?
			return null;
		}


		private IWordSplitter _splitter;
	}
}
