using System.Collections.Generic;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	public interface IIndexerSearcher
	{
        IEnumerable<CodeSearchResult> Search(SearchCriteria searchCriteria);
    }
}
