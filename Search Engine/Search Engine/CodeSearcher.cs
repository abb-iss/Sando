using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;

namespace Sando.SearchEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
   
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
        /// <returns>List of Search Result</returns>
        public virtual List<CodeSearchResult> Search(string searchString)
        {			
			SearchCriteria searchCrit = this.GetCriteria(searchString);
			//test cache hits
			bool indexingChanged = false;//TODO: need API to get the status of the indexing
			List<CodeSearchResult> res = lruCache.Get(searchCrit);
			if(res!=null && !indexingChanged)
			{
				//cache hits and index not changed
				return res;
			}
			else
			{
				//no cache hits, new search and update the cache
				res = this.searcher.Search(searchCrit).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
				//add into cache even the res contains no contents
				lruCache.Put(searchCrit, res);
				return res;
			}
        }

		/// <summary>
		/// Searches using the specified search criteria.
		/// </summary>
		/// <param name="searchCriteria">The search criteria.</param>
		/// <returns>List of Search Result</returns>
		public virtual List<CodeSearchResult> Search(SearchCriteria searchCriteria)
		{
			//test cache hits
			bool indexingChanged = false;//TODO: need API to get the status of the indexing
			List<CodeSearchResult> res = lruCache.Get(searchCriteria);
			if(res != null && !indexingChanged)
			{
				//cache hits and index not changed
				return res;
			}
			else
			{
				//no cache hits, new search and update the cache
				res = this.searcher.Search(searchCriteria).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
				//add into cache even the res contains no contents
				lruCache.Put(searchCriteria, res);
				return res;
			}
		}

        #endregion	
        #region Private Mthods
        /// <summary>
        /// Gets the criteria.
        /// </summary>
        /// <param name="searchString">Search string.</param>
        /// <returns>search criteria</returns>
        private SearchCriteria GetCriteria(string searchString)
        {
            var criteria = new SimpleSearchCriteria();
            criteria.SearchTerms = new SortedSet<string>(searchString.Trim().Split(' ').ToList());
            return criteria;
        }
        #endregion
    }
}
