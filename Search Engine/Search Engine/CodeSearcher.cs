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
    public class CodeSearcher
    {
        #region Private Members
        /// <summary>
        /// private member for lucene directory
        /// </summary>
        private string directory;

        /// <summary>
        /// private member for analyzer
        /// </summary>
        private Analyzer analyzer;        
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearcher"/> class.
        /// </summary>
        /// <param name="directory">Sando code indexer directory.</param>
        /// <param name="analyzer">Analyzer used during index creation, Same has to be used during search.</param>
        public CodeSearcher(string directory, SimpleAnalyzer analyzer)
        {
            this.directory = directory;
            this.analyzer = analyzer;            
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Searches the specified search string.
        /// </summary>
        /// <param name="searchString">The search string.</param>
        public virtual Hits Search(string searchString)
        {
            Directory luceneDirectory = null;            
            IndexSearcher searcher = null;
            try
            {
             luceneDirectory = FSDirectory.Open(new System.IO.DirectoryInfo(this.directory));             
             searcher = new IndexSearcher(luceneDirectory, true);             
             Query searchQuery = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_29, new string[] { searchString }, this.GetSearchableFields(), this.analyzer);
             return searcher.Search(searchQuery);             
            }
            finally
            {
                if (luceneDirectory != null)
                {
                    luceneDirectory.Close();
                }
            }
        }

        /// <summary>
        /// Gets the searchable fields.
        /// </summary>
        /// <returns>An array of searchable fields</returns>
        public virtual string[] GetSearchableFields()
        {
            //TODO : To determine logic to get searchable fields for different elements
            return new string[] {"Name"};
        }
        #endregion
    }
}
