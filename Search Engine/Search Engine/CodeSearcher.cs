using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;

namespace Sando.SearchEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Lucene.Net;
    using Lucene.Net.Analysis;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    /// <summary>
    /// Class defined to search the index using code searcher
    /// </summary>
    public class CodeSearcher
    {
        #region Private Members    	

		private IIndexerSearcher Searcher
		{
			get; set; 
		}

    	#endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearcher"/> class.
        /// </summary>
        /// <param name="indexer">Sando code indexer</param>        
		public CodeSearcher(IIndexerSearcher searcher)
        {
			this.Searcher = searcher;

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
			return Searcher.Search(GetCriteria(searchString)).Select(tuple => new CodeSearchResult(tuple.Item1, tuple.Item2)).ToList();
        }

    	/// <summary>
        /// Gets the searchable fields.
        /// </summary>
        /// <returns>An array of searchable fields</returns>
        public virtual string[] GetSearchableFields()
        {
            //TODO : To determine logic to get searchable fields for different elements
            return new string[] { "Name" };
        }
        #endregion

		private SearchCriteria GetCriteria(string searchString)
		{
			var criteria = new SimpleSearchCriteria();
			criteria.SearchTerms = searchString.Split(' ').ToList();
			return criteria;
		}
    }
}
