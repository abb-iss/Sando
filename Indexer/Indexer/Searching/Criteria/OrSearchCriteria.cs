using System.Diagnostics.Contracts;

namespace Sando.Indexer.Searching.Criteria
{
	public class OrSearchCriteria : SearchCriteria
	{
		public OrSearchCriteria(SearchCriteria searchCriteria1, SearchCriteria searchCriteria2)
		{
			Contract.Requires(searchCriteria1 != null, "OrSearchCriteria:Constructor - searchCriteria1 cannot be null!");
			Contract.Requires(searchCriteria2 != null, "OrSearchCriteria:Constructor - searchCriteria2 cannot be null!");

			SearchCriteria1 = searchCriteria1;
			SearchCriteria2 = searchCriteria2;
		}

		public override string ToQueryString()
		{
			return SearchCriteria1.ToQueryString() + " OR " + SearchCriteria2.ToQueryString();
		}

		public SearchCriteria SearchCriteria1 { get; set; }
		public SearchCriteria SearchCriteria2 { get; set; }
	}
}
