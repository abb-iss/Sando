using System.IO;

namespace Sando.SearchEngine
{
    using Sando.Core;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;    

    /// <summary>
    /// Class defined to create return result from Lucene indexer
    /// </summary>
   public class CodeSearchResult
   {
       #region Public Properties
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
       /// Sando Program Element.
       /// </value>
       public ProgramElement Element
       {
           get;
           private set;
       }

       public String Snippet
       {
           get
           {
               var snip = Element.Snippet;
               if(snip.StartsWith("\r\n"))
               {
                   snip = snip.Substring(2);
               }
               return snip.Replace("\t", "     ");
           }           
       }
    	public string FileName
    	{
    		get
    		{
    			var fileName = Path.GetFileName(Element.FullFilePath);
                fileName = Shorten(fileName);
    			return fileName;
    		}    		
    	}

        private static string Shorten(string fileName)
        {
            if (fileName.Length > 20)
            {
                fileName = fileName.Substring(0, 20) + "...";
            }
            return fileName;
        }

        public string Parent
    	{
    		get
    		{
    			var method = Element as MethodElement;
				if(method !=null)
				{
					return Shorten(method.ClassName);
				}else
				{
					return "";
				}

    		}
    	}
       #endregion
       #region Constructor
       /// <summary>
        /// Initializes a new instance of the <see cref="CodeSearchResult"/> class.
        /// </summary>
        /// <param name="programElement">program element.</param>
        /// <param name="score">search score.</param>
	   public CodeSearchResult(ProgramElement programElement, double score)
	   {
		   this.Element = programElement;
		   this.Score = score;
	   }
       #endregion
       
    }
}
