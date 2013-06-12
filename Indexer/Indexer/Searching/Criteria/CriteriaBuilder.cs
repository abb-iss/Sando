﻿using Sando.Core.Tools;
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
            var terms = ReformQuery(WordSplitter.ExtractSearchTerms(searchString).
                SelectMany(ServiceLocator.Resolve<DictionaryBasedSplitter>().ExtractWords));
            _searchCriteria.SearchTerms = new SortedSet<string>(terms);
            return this;
        }

        private IEnumerable<string> ReformQuery(IEnumerable<string> words)
        {
            words = words.ToList();
            var reformer = ServiceLocator.Resolve<QueryReformer>();
            var reformedQueries = reformer.ReformTermsSynchronously(words).ToList();
            return reformedQueries.Any(q => q.QuryReformLevel == QuryReformLevel.REPLACING) ? 
                reformedQueries.First(q => q.QuryReformLevel == QuryReformLevel.REPLACING).GetReformedTerms : words;
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
