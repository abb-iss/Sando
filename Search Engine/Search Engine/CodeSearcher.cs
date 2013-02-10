using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sando.Core.Tools;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;

namespace Sando.SearchEngine
{
   
    /// <summary>
    /// Class defined to search the index using code searcher
    /// </summary>
    public class CodeSearcher
    {
        #region Private Members    	

        /// <summary>
        /// Gets or sets the searcher.
        /// </summary>
        /// <value>
        /// Instance of searcher.
        /// </value>
		private IIndexerSearcher searcher
		{
			get; set; 
		}
    	#endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearcher"/> class.
        /// </summary>
        /// <param name="searcher">instance of index searcher.</param>
		public CodeSearcher(IIndexerSearcher searcher)
        {
            this.searcher = searcher;
        }
        #endregion       
        #region Public Methods

        /// <summary>
        /// Searches the specified search string.
        /// </summary>
        /// <param name="searchString">The search string.</param>
		/// <param name="searchCriteria">The other search criteria</param>
        /// <returns>List of Search Result</returns>
		public virtual List<CodeSearchResult> Search(string searchString)
		{
			Contract.Requires(String.IsNullOrWhiteSpace(searchString), "CodeSearcher:Search - searchString cannot be null or an empty string!");

            SearchCriteria searchCrit = CriteriaBuilder.GetBuilder().AddSearchString(searchString).GetCriteria();
			//test cache hits
			bool indexingChanged = false;//TODO: need API to get the status of the indexing
            List<CodeSearchResult> res = this.searcher.Search(searchCrit).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
            return GetResultOrEmpty(res,searchString,"");
		}

		/// <summary>
		/// Searches using the specified search criteria.
		/// </summary>
		/// <param name="searchCriteria">The search criteria.</param>
		/// <returns>List of Search Result</returns>
		public virtual List<CodeSearchResult> Search(SearchCriteria searchCriteria, String solutionName="")
		{
			//test cache hits
			bool indexingChanged = false;//TODO: need API to get the status of the indexing
			List<CodeSearchResult> res = this.searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
            if (NoResults(res))
                res = AddStartToQueryAndRerun(searchCriteria, res);
			return GetResultOrEmpty(res,GetSearchTerms(searchCriteria),solutionName);
		}

        private List<CodeSearchResult> AddStartToQueryAndRerun(SearchCriteria searchCriteria, List<CodeSearchResult> res)
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
                    res = this.searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
                }
                res = this.searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
            }
            return res;
        }

        private static bool NoResults(List<CodeSearchResult> res)
        {
            return res == null || res.Count == 0;
        }

        private string GetSearchTerms(SearchCriteria searchCriteria)
        {
            var simple = searchCriteria as SimpleSearchCriteria;
            if(simple!=null)
            {
                return String.Join(" ",simple.SearchTerms.Reverse());
            }else
            {
                return "";
            }
        }

        private List<CodeSearchResult> GetResultOrEmpty(List<CodeSearchResult> res, string searchTerm, string solutionName)
        {
            if(res!=null && res.Count>0)
            {
                return res;
            }else
            {
                var empty = new List<CodeSearchResult>();                
                return empty;
            }
        }

        #endregion


		
	}
}
