using System;
using System.Linq;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.ExtensionContracts.SearchContracts
{
	public interface ISearchResultListener
	{
		void Update(string searchString, IQueryable<CodeSearchResult> results);
	    void UpdateMessage(string message);
	    void UpdateRecommendedQueries(IQueryable<String> queries);
	}
}
