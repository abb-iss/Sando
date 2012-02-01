using System.Diagnostics.Contracts;

namespace Sando.Indexer.Searching.Criteria
{
	//NOT operator in Lucene cannot be used alone:
	//The NOT operator excludes documents that contain the term after NOT. This is equivalent to a difference using sets. The symbol ! can be used in place of the word NOT.
	//To search for documents that contain "jakarta apache" but not "Apache Lucene" use the query: 
	//"jakarta apache" NOT "Apache Lucene"
	//Note: The NOT operator cannot be used with just one term. For example, the following search will return no results:
	//NOT "jakarta apache"
	public class NotSearchCriteria : SearchCriteria
	{
		public NotSearchCriteria(SearchCriteria searchCriteria1, SearchCriteria searchCriteria2)
		{
			Contract.Requires(searchCriteria1 != null, "NotSearchCriteria:Constructor - searchCriteria1 cannot be null!");
			Contract.Requires(searchCriteria2 != null, "NotSearchCriteria:Constructor - searchCriteria2 cannot be null!");

			SearchCriteria1 = searchCriteria1;
			SearchCriteria2 = searchCriteria2;
		}

		public override string ToQueryString()
		{
			return SearchCriteria1.ToQueryString() + " NOT " + SearchCriteria2.ToQueryString();
		}

		public SearchCriteria SearchCriteria1 { get; set; }
		public SearchCriteria SearchCriteria2 { get; set; }
	}
}
