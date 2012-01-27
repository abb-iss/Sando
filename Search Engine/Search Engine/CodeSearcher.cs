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
			//DAVE: Consider removing reference to Lucene and analyzer by using the DocumentIndexerFactory
            this.analyzer = analyzer;
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
            Directory luceneDirectory = null;            
            IndexSearcher searcher = null;
            IndexReader indexReader = null;
            try
            {
			 //DAVE: Consider putting code to initialize indexer and searcher in a CodeSearcher.Initialize method.
				// My concern with initializing the indexer and searcher for each search is that it may be to expensive in terms of time.
             luceneDirectory = FSDirectory.Open(new System.IO.DirectoryInfo(this.directory));
             indexReader = IndexReader.Open(luceneDirectory, true);             
             searcher = new IndexSearcher(indexReader);             
             Query searchQuery = MultiFieldQueryParser.Parse(Lucene.Net.Util.Version.LUCENE_29, new string[] { searchString }, this.GetSearchableFields(), this.analyzer);
             TopDocs resultDocs = searcher.Search(searchQuery, indexReader.MaxDoc());
             List<CodeSearchResult> results = new List<CodeSearchResult>();
             foreach (ScoreDoc scoreDoc in resultDocs.ScoreDocs)
             {
                results.Add(new CodeSearchResult() { Score = scoreDoc.score, Document = searcher.Doc(scoreDoc.doc) });
             }             

             return results;                        
            }   
            finally
            {
				//DAVE: Same idea as above, consider moving this to a CodeSearcher.Dispose instead of opening and closing for each search
                if (luceneDirectory != null)
                {
                    luceneDirectory.Close();
                }

                indexReader.Dispose();
            }
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
    }
}
