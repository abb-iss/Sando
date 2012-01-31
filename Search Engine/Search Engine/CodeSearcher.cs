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
        /// <returns>List of Search Result</returns>
        public virtual List<CodeSearchResult> Search(string searchString)
        {
		 return this.searcher.Search(this.GetCriteria(searchString)).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
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
            criteria.SearchTerms = searchString.Split(' ').ToList();
            return criteria;
        }
        #endregion
    }
}
