using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;

namespace Sando.SearchEngine
{
    public class CodeSearcher
    {
        private readonly IIndexerSearcher _searcher;

        public CodeSearcher(IIndexerSearcher searcher)
        {
            _searcher = searcher;
        }

        public virtual List<CodeSearchResult> Search(string searchString, bool rerunWithWildcardIfNoResults = false)
		{
			Contract.Requires(String.IsNullOrWhiteSpace(searchString), "CodeSearcher:Search - searchString cannot be null or an empty string!");

            var searchCriteria = CriteriaBuilder.GetBuilder().AddSearchString(searchString).GetCriteria();
            return Search(searchCriteria, rerunWithWildcardIfNoResults);
		}

        public virtual List<CodeSearchResult> Search(SearchCriteria searchCriteria, bool rerunWithWildcardIfNoResults = false)
		{
            Contract.Requires(searchCriteria != null, "CodeSearcher:Search - searchCriteria cannot be null!");

            var searchResults = _searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
		    if (!searchResults.Any() && rerunWithWildcardIfNoResults)
		        searchResults = RerunQueryWithWildcardAtTheEnd(searchCriteria, searchResults);
			return searchResults;
		}

        private List<CodeSearchResult> RerunQueryWithWildcardAtTheEnd(SearchCriteria searchCriteria, List<CodeSearchResult> searchResults)
        {
            var simple = searchCriteria as SimpleSearchCriteria;
            if (simple != null)
            {
                var terms = simple.SearchTerms;
                if (terms.Count == 1)
                {
                    var term = simple.SearchTerms.First();
                    simple.SearchTerms.Clear();
                    simple.SearchTerms.Add(term + "*");
                    searchResults = _searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
                }
            }
            return searchResults;
        }
    }
}
