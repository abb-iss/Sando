using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using ABB.SrcML;
using System.Xml;
using System.Xml.Linq;

namespace LocalSearch
{
    public class Context : GraphBuilder
    {
        public String query { set;  get; }

        //int -- line number on which the query or its extension is satisfied (only the highest score result?)
        public List<Tuple<CodeSearchResult, int>> searchres { set; get; }

        public List<ProgramElementWithRelation> path
        {
            set; //when the user makes a new selection in the UI, set it
            get;
        }

        public List<List<ProgramElementWithRelation>> droppedPaths
        {
            set; //when the user drop a path (by returning to an ealier state), set it
            get;
        }

        public int curPos
        {
            get
            {
                return path.Count - 1;
            }

        }
        
        public Context(String srcPath, string SrcMLForCSharp = null):base(srcPath, SrcMLForCSharp) 
        {
            path = new List<ProgramElementWithRelation>();
            droppedPaths = new List<List<ProgramElementWithRelation>>();
            searchres = new List<Tuple<CodeSearchResult,int>>();
        }

        public Context(String srcPath, String search, string SrcMLForCSharp = null)
            : base(srcPath, SrcMLForCSharp) 
        {
            query = search;
            path = new List<ProgramElementWithRelation>();
            droppedPaths = new List<List<ProgramElementWithRelation>>();
            searchres = new List<Tuple<CodeSearchResult, int>>();
        }

        public ProgramElementRelation GetRelation(CodeSearchResult element1, CodeSearchResult element2, ref List<int> UsedLineNumber)
        {
            ProgramElementRelation relation = ProgramElementRelation.No;
            ProgramElementType eletype1 = element1.ProgramElementType;
            ProgramElementType eletype2 = element2.ProgramElementType;

            if(eletype1.Equals(ProgramElementType.Field))
            {
                if(eletype2.Equals(ProgramElementType.Method))
                {
                    var methodDeclaration = GetMethod(element2.ProgramElement as MethodElement);
                    if(ifFieldUsedinMethod(methodDeclaration, element1.Name, ref UsedLineNumber))
                    {
                        relation = ProgramElementRelation.UseBy;
                        return relation;
                    }
                }
            }            
            else // eletype1.Equals(ProgramElementType.Method)
            {
                   if(eletype2.Equals(ProgramElementType.Method))
                   {
                       var methodDec1 = GetMethod(element1.ProgramElement as MethodElement);
                       var methodDec2 = GetMethod(element2.ProgramElement as MethodElement);

                       List<Tuple<XElement, int>> myCallers = null;
                       List<Tuple<XElement, int>> myCallees = null;
                       Callers.TryGetValue(methodDec1.GetSrcLineNumber(), out myCallers);
                       Calls.TryGetValue(methodDec1.GetSrcLineNumber(), out myCallees);

                       if (myCallers != null)
                       {   
                           foreach (var caller in myCallers)
                           {
                               if ((caller.Item1.GetSrcLineNumber() == methodDec2.GetSrcLineNumber())
                               && (caller.Item1.Element(SRC.Name).Value == methodDec2.Element(SRC.Name).Value))
                               {
                                   relation = ProgramElementRelation.CallBy;
                                   UsedLineNumber.Add(caller.Item2);
                               }                               
                           }
                           return relation;
                       }

                       if (myCallees != null)
                       {
                           foreach (var callee in myCallees)
                           {
                               if ((callee.Item1.GetSrcLineNumber() == methodDec2.GetSrcLineNumber())
                               && (callee.Item1.Element(SRC.Name).Value == methodDec2.Element(SRC.Name).Value))
                               {
                                   relation = ProgramElementRelation.Call;
                                   UsedLineNumber.Add(callee.Item2);
                               }
                           }

                           return relation;                           
                       }

                   }
                   else //eletype2.Equals(ProgramElementType.Field)
                   {
                       var methodDeclaration = GetMethod(element1.ProgramElement as MethodElement);
                       if(ifFieldUsedinMethod(methodDeclaration, element2.Name, ref UsedLineNumber))
                       {
                            relation = ProgramElementRelation.Use;
                            return relation;
                       }
                   }
            }

            return relation;
                
        }

        public void RankRelatedInfo(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo, UInt16 heuristic = 1)
        {
            //score setting
            switch (heuristic)
            {
                case 1:
                    {
                        BasicHeuristic(target, ref listRelatedInfo);
                        break;
                    }
                case 2:
                    {
                        DistanceToQueryHeuristic(target, ref listRelatedInfo);
                        break;
                    }
                case 3:
                    {
                        EditDistanceHeuristic(target, ref listRelatedInfo);
                        break;
                    }
                default:
                    break;
            }

            //bubble ranking
            for (int i = 0; i < listRelatedInfo.Count()-1; i++)
                for(int j=i+1; j< listRelatedInfo.Count(); j++)
            {
                if (listRelatedInfo[j].Score > listRelatedInfo[i].Score)
                {
                    ProgramElementWithRelation temp = listRelatedInfo[j];
                    listRelatedInfo[j] = listRelatedInfo[i];
                    listRelatedInfo[i] = temp;
                }
            }
        }

        #region ranking heuristics 
        private void BasicHeuristic(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo)
        {            
            foreach (var related in listRelatedInfo)
            {
                if (path.Count() != 0)
                {
                    //what has shown before is set lower score
                    if (isExisting(path, related))
                    {
                        related.Score = related.Score - 0.2;
                    }
                }

                if (searchres.Count() != 0)
                {
                    //what is more closer related to search result is set higher score
                    if (isExisting(searchres, related))
                    {
                        related.Score = related.Score + 0.1;
                    }
                }

            }            
        }

        private bool isExisting(List<ProgramElementWithRelation> source, ProgramElementWithRelation target)
        {   
            foreach (var ele in source)
            {
                if (ele.Name.Equals(target.Name)
                && ele.ProgramElementType.Equals(target.ProgramElementType)
                && ele.ProgramElement.DefinitionLineNumber.Equals(target.ProgramElement.DefinitionLineNumber))
                    return true;
            }

            return false;
        }

        private bool isExisting(List<Tuple<CodeSearchResult,int>> source, ProgramElementWithRelation target)
        {
            foreach (var ele in source)
            {
                var ele1 = ele.Item1;
                if (ele1.Name.Equals(target.Name)
                  && ele1.ProgramElementType.Equals(target.ProgramElementType)
                  && ele1.ProgramElement.DefinitionLineNumber.Equals(target.ProgramElement.DefinitionLineNumber))
                    return true;
            }

            return false;  
        }


        private void DistanceToQueryHeuristic(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo)
        {

        }

        private void EditDistanceHeuristic(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo)
        {
        }

        #endregion ranking heuristics

    }
}
