using System.IO;
using System.Collections.Generic;
using Sando.ExtensionContracts.ProgramElementContracts;
using System;

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

        public string Type
        {
            get
            {          
               return Element.GetName();                
            }
        }

		public static readonly int DefaultSnippetSize = 5;

		public string Snippet
		{
			get
			{
				var raw = Element.RawSource;
				return SourceToSnippet(raw, DefaultSnippetSize);               
			}           
		}

		public static string SourceToSnippet(string source, int numLines)
		{
			int toRemove = int.MaxValue;
			var lines = new List<string>(source.Split('\n'));
			if(numLines < lines.Count)
			{
				lines.RemoveRange(numLines, lines.Count - numLines);
			}

			//measure the shortest empty space prefix in all the lines in the snip
			for(int i = 0; i < lines.Count; i++)
			{
				string line = lines[i];
				if(!string.IsNullOrWhiteSpace(line))
				{
					int perLineToRemove;

					if(source.StartsWith("\t"))
					{
						perLineToRemove = source.Length - source.TrimStart('\t').Length;
					}
					else if(source.StartsWith(" "))
					{
						perLineToRemove = source.Length - source.TrimStart(' ').Length;
					}
					else
					{
						perLineToRemove = 0;
					}

					if(perLineToRemove < toRemove)
					{
						toRemove = perLineToRemove;
					}
				}
			}
		
			//remove the empty spaces in front
			if (toRemove > 0 && toRemove < int.MaxValue)
			{
				string newSnip = "";
				foreach (string line in lines)
				{
       				if (line.Length > toRemove + 1)
       					newSnip += line.Remove(0, toRemove) + "\n";
				}
				return newSnip;
			}

			return String.Join("\n", lines.ToArray());
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
