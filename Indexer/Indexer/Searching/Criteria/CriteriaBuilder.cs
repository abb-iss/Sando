using System;
using Sando.Core.QueryRefomers;
using Sando.Core.Tools;
using System.Collections.Generic;
using System.Linq;
using Sando.DependencyInjection;

namespace Sando.Indexer.Searching.Criteria
{
    public class CriteriaBuilder
    {
        SimpleSearchCriteria _searchCriteria;
     
        public static CriteriaBuilder GetBuilder()
        {
            return new CriteriaBuilder();
        }

        public CriteriaBuilder AddSearchString(string searchString, SimpleSearchCriteria searchCriteria = null)
        {
            Initialze(searchCriteria);
            var terms = WordSplitter.ExtractSearchTerms(searchString).ToList();
            var dictionarySplittedTerms = terms.SelectMany(ServiceLocator.Resolve<DictionaryBasedSplitter>
                ().ExtractWords).ToList(); 
            terms.AddRange(dictionarySplittedTerms);
            var query = TryGetReformedQuery(terms.Distinct());
            if (query != null)
            {
                terms.AddRange(query.GetReformedTerms.ToList());
                _searchCriteria.Explanation = query.ReformExplanation;
                _searchCriteria.Reformed = true;
            }
            else
            {
                _searchCriteria.Reformed = false;
            }
        
            _searchCriteria.SearchTerms = new SortedSet<string>(terms.Distinct());
            return this;
        }

        private IReformedQuery TryGetReformedQuery(IEnumerable<string> words)
        {
            words = words.ToList();
            var reformer = ServiceLocator.Resolve<QueryReformerManager>();
            var reformedQueries = reformer.ReformTermsSynchronously(words).ToList();
            _searchCriteria.RecommendedQueries = TryGetRecommendedQueries(reformedQueries);
            return reformedQueries.FirstOrDefault();
        }

        private IEnumerable<string> TryGetRecommendedQueries(List<IReformedQuery> reformedQueries)
        {
            return reformedQueries.Count > 1 ? reformedQueries.Skip(1).Select(q => q.QueryString) 
                : Enumerable.Empty<string>();
        }


        private void Initialze(SimpleSearchCriteria searchCriteria)
        {
            if (_searchCriteria == null)
            {
                _searchCriteria = searchCriteria ?? new SimpleSearchCriteria();
            }   
        }

        public SearchCriteria GetCriteria()
        {
            return _searchCriteria;
        }

        public CriteriaBuilder AddCriteria(SimpleSearchCriteria searchCriteria)
        {
            Initialze(searchCriteria);
            return this;
        }

        public CriteriaBuilder NumResults(int numResults, SimpleSearchCriteria searchCriteria = null)
        {
            Initialze(searchCriteria);
            _searchCriteria.NumberOfSearchResultsReturned = numResults;
            return this;
        }

        public CriteriaBuilder Ext(string searchString, SimpleSearchCriteria searchCriteria = null)
        {
            Initialze(searchCriteria);
            _searchCriteria.FileExtensions = WordSplitter.GetFileExtensions(searchString);
            _searchCriteria.SearchByFileExtension = _searchCriteria.FileExtensions.Any();
            return this;
        }
    }
}
