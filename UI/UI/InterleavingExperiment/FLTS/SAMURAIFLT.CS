using System;
using System.Collections.Generic;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;
using Sando.Parser;

namespace Sando.UI.InterleavingExperiment.FLTs
{
	public class SamuraiFLT : FeatureLocationTechnique, IParser, IWordSplitter
	{
		public SamuraiFLT() : base("Samurai")
		{
		}

		public override void IssueQuery(string query)
		{
			throw new NotImplementedException();
		}

		public override List<CodeSearchResult> GetResults()
		{
			throw new NotImplementedException();
		}

		public List<ProgramElement> Parse(string filename)
		{
			var parser = new SrcMLCSharpParser();
			var parsedElements = parser.Parse(filename);
			foreach (var programElement in parsedElements)
			{
			
			}
		}

		public string[] ExtractWords(string text)
		{
			//need to know the program element, to find the umph
			throw new NotImplementedException();
		}
	}
}
