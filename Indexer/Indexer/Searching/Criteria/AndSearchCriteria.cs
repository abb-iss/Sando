using System.Diagnostics.Contracts;

namespace Sando.Indexer.Searching.Criteria
{
	public class AndSearchCriteria : SearchCriteria
	{
		public AndSearchCriteria(SearchCriteria searchCriteria1, SearchCriteria searchCriteria2)
		{
			Contract.Requires(searchCriteria1 != null, "AndSearchCriteria:Constructor - searchCriteria1 cannot be null!");
			Contract.Requires(searchCriteria2 != null, "AndSearchCriteria:Constructor - searchCriteria2 cannot be null!");

			SearchCriteria1 = searchCriteria1;
			SearchCriteria2 = searchCriteria2;
		}

		public override string ToQueryString()
		{
			return SearchCriteria1.ToQueryString() + " AND " + SearchCriteria2.ToQueryString();
		}

		public SearchCriteria SearchCriteria1 { get; protected set; }
		public SearchCriteria SearchCriteria2 { get; protected set; }
	}
}
