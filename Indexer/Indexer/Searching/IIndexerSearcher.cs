using System;
using System.Collections.Generic;
using Sando.Core;
using Sando.Indexer.Searching.Criteria;

namespace Sando.Indexer.Searching
{
	public interface IIndexerSearcher
	{
		List<Tuple<ProgramElement,Double>> Search(SearchCriteria searchCriteria);
	}
}
