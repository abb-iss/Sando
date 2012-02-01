using System.Diagnostics.Contracts;

namespace Sando.Indexer.Searching.Criteria
{
	public class ParenthesisSearchCriteria : SearchCriteria
	{
		public ParenthesisSearchCriteria(SearchCriteria searchCriteria)
		{
			Contract.Requires(searchCriteria != null, "ParenthesisSearchCriteria:Constructor - searchCriteria cannot be null!");

			SearchCriteria = searchCriteria;
		}

		public override string ToQueryString()
		{
			return "(" + SearchCriteria.ToQueryString() + ")";
		}

		public SearchCriteria SearchCriteria { get; set; }
	}
}
