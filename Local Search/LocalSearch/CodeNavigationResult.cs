using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.ComponentModel;
using System.Xml.Linq;
using ABB.SrcML;

namespace Sando.LocalSearch
{
    public class CodeNavigationResult : CodeSearchResult
    {
        public ProgramElementRelation ProgramElementRelation { get; set; }         

        public String ProgramElementRelationString
        {
            get
            {
                if(ProgramElementRelation.Equals(ProgramElementRelation.Other))
                    return "definition";
                else
                {
                    var Element = this;
                    var type = typeof(ProgramElementRelation);
                    if (Element as CodeNavigationResult != null)
                    {
                        var memInfo = type.GetMember(((Element as CodeNavigationResult)).ProgramElementRelation.ToString());
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

        public XElement RelationCode { get; set; }

        public List<double> scorehistory;

        public String RelationCodeAsString
        {
            get { return RelationCode.ToSource(); }
        }

        public CodeNavigationResult(ProgramElement element, double score, XElement code):
            base(element, score)
	   {           
           this.ProgramElementRelation = ProgramElementRelation.Other;

           this.RelationLineNumber = new List<int>();
           this.RelationLineNumber.Add(Convert.ToInt32(this.ProgramElement.DefinitionLineNumber));

           this.scorehistory = new List<double>();
           this.scorehistory.Add(score);

           this.RelationCode = new XElement(code);
           if (element.ProgramElementType == ProgramElementType.Method) 
           {
               XElement body = this.RelationCode.Element(SRC.Block);
               try // [todo -- uncomment when release, now leave for detecting bug]
               {
                   body.Remove();
               }
               catch (NullReferenceException e)
               {
                   //do nothing
               }
           }
           
	   }
               
    }
}
