using System.Linq;

namespace Sando.ExtensionContracts.ResultsReordererContracts
{
	public interface IResultsReorderer
	{
		IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults);
	}
}
