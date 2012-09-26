using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.InterleavingExperiment
{
	public abstract class FeatureLocationTechnique
	{
		public FeatureLocationTechnique(string name)
		{
			Name = name;
		}

		public abstract void IssueQuery(string query);

		public abstract List<CodeSearchResult> GetResults();

		public string Name { private set; get; }
	}
}
