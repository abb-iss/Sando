using System.Linq;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.SearchEngine
{
	public class SortByScoreResultsReorderer : IResultsReorderer
	{
		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
			return searchResults.OrderByDescending(r => r.Score);
		}
	}
}
