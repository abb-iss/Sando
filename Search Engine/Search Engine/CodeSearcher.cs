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
		private LRUCache<SearchCriteria, List<CodeSearchResult>> lruCache
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
			int capacity = 50; //TODO: max capacity of the cache, need to determine its value
			                   //from configuration or default value (50)
			lruCache = new LRUCache<SearchCriteria, List<CodeSearchResult>>(capacity);
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

			SearchCriteria searchCrit = this.GetCriteria(searchString);
			//test cache hits
			bool indexingChanged = false;//TODO: need API to get the status of the indexing
            return ExecuteSearch(searchCrit,indexingChanged);
		}

        private List<CodeSearchResult> ExecuteSearch(SearchCriteria searchCrit,bool indexingChanged, bool ignoreAnalyzer =false)
        {
            List<CodeSearchResult> res = lruCache.Get(searchCrit);
            if (res != null && !indexingChanged)
            {
                //cache hits and index not changed
                return GetResultOrEmpty(res,  "");
            }
            else
            {
                //no cache hits, new search and update the cache
                if (ignoreAnalyzer)
                {
                    res = UnanalyzedQuerySearch(searchCrit);
                }
                else
                {                    
                    res = NormalSearch(searchCrit);
                }
                //add into cache even the res contains no contents
                lruCache.Put(searchCrit, res);
                return GetResultOrEmpty(res,  "");
            }
        }

        private List<CodeSearchResult> NormalSearch(SearchCriteria searchCrit)
        {
            List<CodeSearchResult> res;
            res =
                this.searcher.Search(searchCrit).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2))
                    .ToList();
            return res;
        }

        private List<CodeSearchResult> UnanalyzedQuerySearch(SearchCriteria searchCrit)
        {
            List<CodeSearchResult> res;
            //don't mess with the query terms when creating query
            //the analyzer will stem stuff and remove '\', etc.  Sometimes you don't want that,
            //like when matching a filepath
            res =
                this.searcher.SearchNoAnalyzer(searchCrit).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2))
                    .ToList();
            return res;
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
			List<CodeSearchResult> res = lruCache.Get(searchCriteria);
			if(res != null && !indexingChanged)
			{
				//cache hits and index not changed
                return GetResultOrEmpty(res,  solutionName);
			}
			else
			{
				//no cache hits, new search and update the cache
				res = this.searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
				//add into cache even the res contains no contents
				lruCache.Put(searchCriteria, res);
                return GetResultOrEmpty(res,solutionName);
			}
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

        private List<CodeSearchResult> GetResultOrEmpty(List<CodeSearchResult> res,  string solutionName)
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

		#region Private Mthods
		/// <summary>
		/// Gets the criteria.
		/// </summary>
		/// <param name="searchString">Search string.</param>
		/// <returns>search criteria</returns>
		private SearchCriteria GetCriteria(string searchString, SimpleSearchCriteria searchCriteria = null)
		{
			if (searchCriteria == null)
				searchCriteria = new SimpleSearchCriteria();
			var criteria = searchCriteria;
			criteria.SearchTerms = new SortedSet<string>(WordSplitter.ExtractSearchTerms(searchString));
			return criteria;
		}
		#endregion

        public List<CodeSearchResult> UnalteredSearch(SearchCriteria whatToGet)
        {            
            return ExecuteSearch(whatToGet, false, true);
        }

    }
}
