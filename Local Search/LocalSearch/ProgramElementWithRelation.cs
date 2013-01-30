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

        public int RelationLineNumber { get; set; }

        public String ProgramElementRelationSimple 
        {
            get
            {
                if (ProgramElementRelation == ProgramElementRelation.Other)
                    return "d";
                else
                    return "u";
            } 
        }

        public String ProgramElementTypeSimple
        {
            get { return ProgramElementType.ToString().Substring(0, 1).ToLower(); }
        }

        public ProgramElementWithRelation(ProgramElement element, double score, ProgramElementRelation relation):
            base(element, score)
	   {           
		   this.ProgramElementRelation = relation;
           this.RelationLineNumber = Convert.ToInt32(this.DefinitionLineNumber);
	   }

        public ProgramElementWithRelation(ProgramElement element, double score) :
            base(element, score)
        {
            this.ProgramElementRelation = ProgramElementRelation.Other;
            this.RelationLineNumber = Convert.ToInt32(this.DefinitionLineNumber);
        }

               
    }
}
