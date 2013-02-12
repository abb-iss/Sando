using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.ComponentModel;

namespace LocalSearch
{
    public class ProgramElementWithRelation : CodeSearchResult
    {
        public ProgramElementRelation ProgramElementRelation { get; set; }

        public String ProgramElementRelationString
        {
            get
            {
                if(ProgramElementRelation.Equals(ProgramElementRelation.Other))
                    return "";
                else
                {
                    //WHAT THE HECK!!?!?!  
                    var Element = this;
                    var type = typeof(ProgramElementRelation);
                    if (Element as ProgramElementWithRelation != null)
                    {
                        var memInfo = type.GetMember(((Element as ProgramElementWithRelation)).ProgramElementRelation.ToString());
                        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
                            false);
                        var description = ((DescriptionAttribute)attributes[0]).Description;
                        return description;
                    }
                    else
                        return "";
                    
                }
            }
        }

        public List<int> RelationLineNumber {get; set;}

        public String RelationLineNumberAsString
        {
            get 
            { 
                String relationlinenumber = "";
                foreach (var linenumber in RelationLineNumber)
                {
                    relationlinenumber += linenumber.ToString() + " ";
                }

                return relationlinenumber;
            }
        }

        public String ScoreAsString
        {
            get
            {
                return Score.ToString();
            }
        }

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
           this.RelationLineNumber = new List<int>();
           this.RelationLineNumber.Add(Convert.ToInt32(this.DefinitionLineNumber));
	   }

        public ProgramElementWithRelation(ProgramElement element, double score) :
            base(element, score)
        {
            this.ProgramElementRelation = ProgramElementRelation.Other;
            this.RelationLineNumber = new List<int>();
            this.RelationLineNumber.Add(Convert.ToInt32(this.DefinitionLineNumber));
        }

               
    }
}
