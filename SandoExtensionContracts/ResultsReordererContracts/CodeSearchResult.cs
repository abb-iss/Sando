using System.IO;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExtensionContracts.ResultsReordererContracts
{
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
       public double Score { get; set; }

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

        public string ParentOrFile
        {
            get
            {
                if(string.IsNullOrEmpty(Parent))
                {
                    return Path.GetFileName(this.FileName);
                }else
                {
                    string fileName = Path.GetFileName(this.FileName);
                    if (fileName.StartsWith(Parent))
                    {
                        return fileName;
                    }
                    return Parent +" ("+fileName+")";
                }
            }
        }

       public ProgramElementType ProgramElementType
       {
           get { return Element.ProgramElementType; }
       }

       public string DefinitionLineNumber
       {
           get { return Element.DefinitionLineNumber.ToString(); }
       }

        public string Type
        {
            get
            {          
               return Element.GetName();                
            }
        }

		public string Snippet
		{
			get
			{
				var snip = Element.Snippet;
				return FixSnip(snip);               
			}           
		}

		public static string FixSnip(string snip)
		{
			int toRemove = int.MaxValue;
			string[] split = snip.Split('\n');

			//measure the shortest empty space prefix in all the lines in the snip
			foreach (string line in split)
			{
				if (! string.IsNullOrWhiteSpace(line))
				{
					int perLineToRemove;

					if (snip.StartsWith("\t"))
					{
						perLineToRemove = snip.Length - snip.TrimStart('\t').Length;
					}
					else if (snip.StartsWith(" "))
					{
						perLineToRemove = snip.Length - snip.TrimStart(' ').Length;
					}
					else
					{
						perLineToRemove = 0;
					}

					if (perLineToRemove < toRemove)
					{
						toRemove = perLineToRemove;
					}
				}
			}
		
			//remove the empty spaces in front
			if (toRemove > 0 && toRemove < int.MaxValue)
			{
				string newSnip = "";
				foreach (string line in split)
				{
       				if (line.Length > toRemove + 1)
       					newSnip += line.Remove(0, toRemove) + "\n";
				}
				return newSnip;
			}

			return snip;
		}

    	public string FileName
    	{
    		get
    		{
    			var fileName = Path.GetFileName(Element.FullFilePath);
    			return fileName;
    		}    		
    	}
	   /*
        private static string Shorten(string fileName)
        {
            if (fileName.Length > 17)
            {
                fileName = fileName.Substring(0, 17) + "..";
            }
            return fileName;
        }*/

        public string Parent
    	{
    		get
    		{
    			var method = Element as MethodElement;
				if(method !=null)
				{
					return method.ClassName;
				}else
				{
					return "";
				}

    		}
    	}
       

        public string Name
        {
            get
            {
				return Element.Name;
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
