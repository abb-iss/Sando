namespace Sando.SearchEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Lucene.Net.Documents;

    /// <summary>
    /// Class defined to create return result from Lucene indexer
    /// </summary>
   public class CodeSearchResult
    {
        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The search score.
        /// </value>
       public float Score { get; set; }

       /// <summary>
       /// Gets or sets the document.
       /// </summary>
       /// <value>
       /// The Lucene document.
       /// </value>
       public Document Document { get; set; }
    }
}
