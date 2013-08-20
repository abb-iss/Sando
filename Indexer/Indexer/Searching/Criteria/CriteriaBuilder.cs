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
            // SearchCriteriaReformer.ReformSearchCriteria(_searchCriteria);
            _searchCriteria.SearchTerms = new SortedSet<string>(terms);
            return this;
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
            _searchCriteria.SearchByAccessLevel = _searchCriteria.AccessLevels.Any();
            _searchCriteria.SearchByLocation = _searchCriteria.Locations.Any();
            _searchCriteria.SearchByProgramElementType = _searchCriteria.ProgramElementTypes.Any();
            _searchCriteria.SearchByUsageType = _searchCriteria.UsageTypes.Any();
            _searchCriteria.SearchByFileExtension = _searchCriteria.FileExtensions.Any();
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

        public CriteriaBuilder AddFromDescription(SandoQueryDescription description, SimpleSearchCriteria searchCriteria = null)
        {
            Initialze(searchCriteria);
            _searchCriteria.AddAccessLevels(description.AccessLevels);
            _searchCriteria.FileExtensions.UnionWith(description.FileExtensions);
            _searchCriteria.SearchTerms.UnionWith(description.LiteralSearchTerms);
            _searchCriteria.Locations.UnionWith(description.Locations);
            _searchCriteria.AddProgramElementTypes(description.ProgramElementTypes);
            _searchCriteria.SearchTerms.UnionWith(description.SearchTerms);         
            return this;
        }
    }
}
