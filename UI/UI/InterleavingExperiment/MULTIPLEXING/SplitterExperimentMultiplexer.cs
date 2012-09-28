using System.Collections.Generic;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.UI.InterleavingExperiment.Multiplexing
{
	public class SplitterExperimentMultiplexer : IParser, IWordSplitter
	{
		public SplitterExperimentMultiplexer(ExtensionPointsRepository extensions, IWordSplitter experimentalSplitter)
		{
			_extensionsRepo = extensions;
			_splitter = experimentalSplitter;
		}

		public List<ProgramElement> Parse(string filename)
		{
			var allElements = new List<ProgramElement>();

			var regParser = _extensionsRepo.GetParserImplementation(System.IO.Path.GetExtension(filename));
			var regElements = regParser.Parse(filename);
			allElements.AddRange(regElements);

			foreach(var element in regElements)
			{
				//produce a Mux element for each type of element				
			}

			return allElements;
		}

		public string[] ExtractWords(string text)
		{
			//How do I know what type of program element this text came from?
			return null;
		}


		private IWordSplitter _splitter;
		private ExtensionPointsRepository _extensionsRepo;
	}
}
