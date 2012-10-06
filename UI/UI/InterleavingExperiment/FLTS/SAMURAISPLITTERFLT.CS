using System;
using System.Collections.Generic;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.InterleavingExperiment.FLTs
{
	public class SamuraiSplitterFLT : FeatureLocationTechnique
	{
		public SamuraiSplitterFLT() : base("Samurai")
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
	}
}
