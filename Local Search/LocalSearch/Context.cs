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
    public class Context 
    {

        private GraphBuilder graph;
        private string filePath;

        public String query { set;  get; }

        //int -- line number on which the query or its extension is satisfied (only the highest score result?)
        public List<Tuple<CodeSearchResult, int>> InitialSearchResults { set; get; }

        public List<ProgramElementWithRelation> CurrentPath
        {
            set; //when the user makes a new selection in the UI, set it
            get;
        }

        public List<List<ProgramElementWithRelation>> droppedPaths
        {
            set; //when the user drop a path (by returning to an ealier state), set it
            get;
        }
        
        public Context()
        {
            CurrentPath = new List<ProgramElementWithRelation>();
            droppedPaths = new List<List<ProgramElementWithRelation>>();
            InitialSearchResults = new List<Tuple<CodeSearchResult,int>>();
        }

        public Context(String search)            
        {
            query = search;
            CurrentPath = new List<ProgramElementWithRelation>();
            droppedPaths = new List<List<ProgramElementWithRelation>>();
            InitialSearchResults = new List<Tuple<CodeSearchResult, int>>();
        }

        public void Intialize(string srcPath, string srcMLPath = null)
        {
            graph = new GraphBuilder(srcPath, srcMLPath);
            graph.Initialize();
            filePath = srcPath;
        }


        private void RankRelatedInfo(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo, UInt16 heuristic = 1)
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
                if (CurrentPath.Count() != 0)
                {
                    //what has shown before is set lower score
                    if (isExisting(CurrentPath, related))
                    {
                        related.Score = related.Score - 0.2;
                    }
                }

                if (InitialSearchResults.Count() != 0)
                {
                    //what is more closer related to search result is set higher score
                    double searchScore = isExisting(InitialSearchResults, related);
                    if (searchScore > 0)
                    {
                        related.Score = related.Score + 0.1;
                        if (searchScore > 1)
                            related.Score += searchScore - 1;
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

        private double isExisting(List<Tuple<CodeSearchResult,int>> source, ProgramElementWithRelation target)
        {
            double res = -1;

            foreach (var ele in source)
            {
                var ele1 = ele.Item1;
                if (ele1.Name.Equals(target.Name)
                  && ele1.ProgramElementType.Equals(target.ProgramElementType)
                  && ele1.ProgramElement.DefinitionLineNumber.Equals(target.ProgramElement.DefinitionLineNumber))
                    return ele1.Score;
            }


            return res;  
        }


        private void DistanceToQueryHeuristic(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo)
        {

        }

        private void EditDistanceHeuristic(CodeSearchResult target, ref List<ProgramElementWithRelation> listRelatedInfo)
        {
        }

        #endregion ranking heuristics


        public IEnumerable<CodeSearchResult> GetRecommendations()
        {
            var methods = graph.GetMethodsAsMethodElements();
            methods.AddRange(graph.GetFieldsAsFieldElements());
            return methods;
        }

        public List<ProgramElementWithRelation> GetRecommendations(CodeSearchResult codeSearchResult)
        {            
            ProgramElementType elementtype = codeSearchResult.ProgramElementType;
            List<ProgramElementWithRelation> recommendations = new List<ProgramElementWithRelation>();
            
            if (elementtype.Equals(ProgramElementType.Field))
                recommendations = GetFieldRelatedInfo(codeSearchResult);                
            else // if(elementtype.Equals(ProgramElementType.Method))
                recommendations = GetMethodRelatedInfo(codeSearchResult);

            RankRelatedInfo(codeSearchResult, ref recommendations);

            return recommendations;
        }

        private List<ProgramElementWithRelation> GetFieldRelatedInfo(CodeSearchResult codeSearchResult)
        {
            List<ProgramElementWithRelation> listFiledRelated
                = new List<ProgramElementWithRelation>();
            String fieldname = codeSearchResult.Name;

            //relation 0: get the decl of itself
            if ((codeSearchResult as ProgramElementWithRelation) == null //direct search result (first column)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Use)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Call)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.CallBy)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.UseBy))
            {
                //var fieldDeclaration = GetFieldDeclFromName(codeSearchResult.Element.Name);
                var fieldDeclaration = graph.GetField(codeSearchResult.ProgramElement as FieldElement);
                listFiledRelated.Add(XElementToProgramElementConverter.GetFieldElementWRelationFromDecl(fieldDeclaration, filePath));
            }

            //relation 1: get methods that use this field
            //listFiledRelated.AddRange(GetMethodElementsUseField(fieldname));
            listFiledRelated.AddRange(graph.GetFieldUsers(codeSearchResult));

            //there may be other relations that will be considered in the future
            // todo

            return listFiledRelated;

        }

        private List<ProgramElementWithRelation> GetMethodRelatedInfo(CodeSearchResult codeSearchResult)
        {
            List<ProgramElementWithRelation> listMethodRelated
                = new List<ProgramElementWithRelation>();

            //relation 0: get the decl of itself
            if ((codeSearchResult as ProgramElementWithRelation) == null
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Use)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.Call)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.CallBy)
                || (codeSearchResult as ProgramElementWithRelation).ProgramElementRelation.Equals(ProgramElementRelation.UseBy))
            {
                var methodDeclaration = graph.GetMethod(codeSearchResult.ProgramElement as MethodElement);
                listMethodRelated.Add(XElementToProgramElementConverter.GetMethodElementWRelationFromXElement(methodDeclaration,filePath));
            }

            String methodname = codeSearchResult.Name;
            int srcLineNumber = codeSearchResult.ProgramElement.DefinitionLineNumber;

            //var method = GetFullMethodFromName(methodname, srcLineNumber);
            //Contract.Requires((method != null), "Method "+ methodname + " does not belong to this local file.");

            //relation 1: get methods that are called by this method (callees)
            listMethodRelated.AddRange(graph.GetCallees(codeSearchResult));

            //relation 2: get fields that are used by this method
            //listMethodRelated.AddRange(GetFieldElementsUsedinMethod(method));
            listMethodRelated.AddRange(graph.GetFieldUses(codeSearchResult));

            //relation 3: get methods that call this method (callers)
            listMethodRelated.AddRange(graph.GetCallers(codeSearchResult));

            return listMethodRelated;
        }
        
        public XElement GetXElementFromLineNum(int number)
        {
            return graph.GetXElementFromLineNum(number);
        }

        public ProgramElementRelation GetRelation(CodeSearchResult searchRes1, CodeSearchResult searchRes2, ref List<int> UsedLineNumber)
        {
            return graph.GetRelation(searchRes1, searchRes2, ref UsedLineNumber);
        }
    }
}
