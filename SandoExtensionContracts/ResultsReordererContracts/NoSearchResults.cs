using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExtensionContracts.ResultsReordererContracts
{
    public class NoSearchResults:CodeSearchResult
    {
        

        public NoSearchResults(ProgramElement programElement, double score) : base(programElement, score)
        {
        }

        public static CodeSearchResult Instance(string searchTerm, string openSolution)
        { 
            return new NoSearchResults(new TextLineElement("\""+searchTerm+"\" not found ",0,"Solution "+openSolution," Tips for improving your query:" +
                                                                                                          "\n\t* Consider adding * to the end of your query" +
                                                                                                          "\n\t* Search for a complete, not partial word"+
                                                                                                          "\n\t* Reduce the number of search terms"
            ,"-"),1.0 );
        }
    }
}
