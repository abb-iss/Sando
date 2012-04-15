using System.Linq;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.TestExtensionPoints
{
	public class TestResultsReorderer : IResultsReorderer
	{
		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
			return searchResults.OrderByDescending(r => r.FileName);
		}
	}
}
