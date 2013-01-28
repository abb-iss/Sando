using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace LocalSearch
{
    public class ProgramElementWithRelation : CodeSearchResult
    {
        public ProgramElementRelation ProgramElementRelation { get; set; }

        //public ProgramElement Element
        //{
        //    get;
        //    private set;
        //}

        public ProgramElementWithRelation(ProgramElement element, double score, ProgramElementRelation relation):
            base(element, score)
	   {           
		   this.ProgramElementRelation = relation;
	   }

        public ProgramElementWithRelation(ProgramElement element, double score) :
            base(element, score)
        {
            this.ProgramElementRelation = ProgramElementRelation.Other;
        }

               
    }
}
