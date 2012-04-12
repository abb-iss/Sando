using System;
using System.Collections.Generic;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	public interface IIndexerSearcher
	{
		List<Tuple<ProgramElement,float>> Search(SearchCriteria searchCriteria);

    }
}
