using Sando.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Indexer.Searching.Criteria
{
    public class CriteriaBuilder
    {
        SimpleSearchCriteria _searchCriteria = null;

        public static CriteriaBuilder GetBuilder()
        {
            return new CriteriaBuilder();
        }

        public CriteriaBuilder AddSearchString(string searchString, SimpleSearchCriteria searchCriteria = null)
        {
            Initialze(searchCriteria);
            _searchCriteria.SearchTerms = new SortedSet<string>(WordSplitter.ExtractSearchTerms(searchString));
            return this;
        }

        private void Initialze(SimpleSearchCriteria searchCriteria)
        {
            if (_searchCriteria == null)
            {
                if (searchCriteria == null)
                    _searchCriteria = new SimpleSearchCriteria();
                else
                    _searchCriteria = searchCriteria;
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
            _searchCriteria.SearchByFileExtension = _searchCriteria.FileExtensions.Count() > 0;
            return this;
        }
    }
}
