using Sando.Core;

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

	   public CodeSearchResult(ProgramElement programElement, double p)
	   {
		   this.Element = programElement;
		   this.Score = p;
	   }
        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The search score.
        /// </value>
       public double Score { get; private set; }

       /// <summary>
       /// Gets or sets the element.
       /// </summary>
       /// <value>
       /// The Lucene document.
       /// </value>
	   public ProgramElement Element
	   {
		   get;
		   private set;
	   }
    }
}
