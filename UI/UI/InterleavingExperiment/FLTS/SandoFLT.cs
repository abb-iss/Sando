using System;
using System.Collections.Generic;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.View;

namespace Sando.UI.InterleavingExperiment.FLTs
{
	public class SandoFLT : FeatureLocationTechnique
	{
		public SandoFLT(): base("Sando") { }

		public override void IssueQuery(string query)
		{
			var searcher = SearchManager.GetCurrentSearcher();
			_results = searcher.Search(query);
		}

		public override List<CodeSearchResult> GetResults()
		{
			return _results;
		}

		private List<CodeSearchResult> _results;
	}
}
