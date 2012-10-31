using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.View;

namespace Sando.IntegrationTests.Search
{
	public class InterleavingSearchTest_ResultListener : ISearchResultListener
	{
		public void Update(IQueryable<CodeSearchResult> results)
		{
			Results = results;
		}

		public void UpdateMessage(string message)
		{
		}

		public IQueryable<CodeSearchResult> Results { get; private set; }

	}
}
