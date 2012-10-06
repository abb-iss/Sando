using System.Collections.Generic;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Core.Extensions
{
	public class ExtensionPointsSet
	{
		public ExtensionPointsSet()
		{
			parsers = new Dictionary<string, IParser>();
		}

		public void ClearSet()
		{
			parsers.Clear();
			wordSplitter = null;
			resultsReorderer = null;
			queryWeightsSupplier = null;
			queryRewriter = null;
		}
		
		public ExtensionPointsSet Clone()
		{
			var clonedSet = new ExtensionPointsSet();
			clonedSet.parsers = this.parsers;
			clonedSet.wordSplitter = this.wordSplitter;
			clonedSet.resultsReorderer = this.resultsReorderer;
			clonedSet.queryWeightsSupplier = this.queryWeightsSupplier;
			clonedSet.queryRewriter = this.queryRewriter;
			return clonedSet;
		}

		public Dictionary<string, IParser> parsers { get; set; }
		public IWordSplitter wordSplitter { get; set; }
		public IResultsReorderer resultsReorderer { get; set; }
		public IQueryWeightsSupplier queryWeightsSupplier { get; set; }
		public IQueryRewriter queryRewriter { get; set; }
	}
}
