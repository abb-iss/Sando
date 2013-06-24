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
            var queries = GetReformedQuery(terms.Distinct()).ToList();
            if (queries.Count > 0)
            {
                var query = queries.First();
                terms.AddRange(query.ReformedTerms.ToList());
                _searchCriteria.Explanation = query.ReformExplanation;
                _searchCriteria.Reformed = true;
                _searchCriteria.RecommendedQueries =
                    queries.GetRange(1, queries.Count - 1).Select(n => n.QueryString).AsQueryable();
            }
            else
            {
                _searchCriteria.Reformed = false;
            }
        
            _searchCriteria.SearchTerms = new SortedSet<string>(terms);
            return this;
        }

        private IEnumerable<IReformedQuery> GetReformedQuery(IEnumerable<string> words)
        {
            words = words.ToList();
            var reformer = ServiceLocator.Resolve<QueryReformerManager>();
            return reformer.ReformTermsSynchronously(words).ToList();
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
