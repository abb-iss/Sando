using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.IntegrationTests;
using Sando.IntegrationTests.Search;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.SearchEngine;
using Sando.LocalSearch;

using System.Collections.Generic;
using System.Linq;

namespace Sando.IntegrationTests.LocalSearch
{
    //target:
    /*
     * 1. MindMapMapModel.java
     * "Saving Failed" --> save (definition, 245) --> saveInternal(callby, 246)
     *                 --> saveInternal (definition, 250)
     *                 --> getXml (callby, 260) --> getXml (definition, 303)
     *                 --> getXml (callby, 304) --> getXml (definition, 286)
     *                 --> getXml (callby, 287) --> getXml (definition, 292)
     *                 
     * 2. ControllerAdaptor.java
     * "Saving Failed" --> (search result) --> mc.Save(control-dependent 969)
     *                 --> Save(definition, 373, same file, different class) 
     *                 --> Save(getModel().getFile) (callby, 378)
     *                 --> Save(definition, 560)
    */

    [TestFixture]
    public class HeuristicConfigurationFreeMind : AutomaticallyIndexingTestClass
    {
        [Test]
        public void FreeMindTest()
        {
            //SetTargetSet();
            
            string testfilePath = @"..\..\IntegrationTests\TestFiles\LocalSearchTestFiles\FreeMindTestFiles-orig\MindMapMapModel.java";
            int treeDepthThreshold = 20;
            int stopLine = 50;

            string keywords = "Saving Failed"; //"Saving Failed";
            var codeSearcher = new CodeSearcher(new IndexerSearcher());            
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);

            Context gbuilder = new Context(keywords);
            gbuilder.Intialize(testfilePath);

            //Console.WriteLine(codeSearchResults.Count); //debugging

            if (codeSearchResults.Count == 0)
                codeSearchResults = gbuilder.GetRecommendations().ToList();

            foreach (var initialSearchRes in codeSearchResults)
            {
                if ((initialSearchRes.Type == "Method") || (initialSearchRes.Type == "Field"))
                {
                    gbuilder.InitialSearchResults.Add(Tuple.Create(initialSearchRes, 1));
                    //Console.WriteLine(initialSearchRes.Name); //debugging
                }   
            }

     //       save (definition, 245) 
     //                 --> saveInternal(callby, 246) --> saveInternal (definition, 250)
     //                 --> getXml (callby, 260) --> getXml (definition, 303)
     //                 --> getXml (callby, 304) --> getXml (definition, 286)
     //                 --> getXml (callby, 287) --> getXml (definition, 292)
            List<targetProgramElement> targetSet = new List<targetProgramElement>();
            List<int> numberOfNavigation = new List<int>();
            List<bool> targetFound = new List<bool>();
            int[] linenumber = { 245,  250, 303, 286, 292, 653};
            String[] elements = { "save", "saveInternal",  "getXml", "getXml", "getXml", "run"};
            ProgramElementRelation[] relations = { ProgramElementRelation.Other, 
                                                   ProgramElementRelation.Other,
                                                   ProgramElementRelation.Other,
                                                   ProgramElementRelation.Other, 
                                                   ProgramElementRelation.Other,
                                                   ProgramElementRelation.Other
                                                 };

            for (int i = 0; i < linenumber.Length; i++)
            {
                targetProgramElement target =
                new targetProgramElement(linenumber[i], elements[i], relations[i]);

                targetSet.Add(target);                
            }

            for (double w0 = 0; w0 <= 0; w0++ )
                for (double w1 = 0; w1 <= 0; w1++)
                    for (double w2 = 1; w2 <= 1; w2++)
                        for (double w3 = 0; w3 <= 0; w3++)
                            for (double w4 = 0; w4 <= 0; w4++)
                        {
                            numberOfNavigation.Clear();
                            targetFound.Clear();
                            for (int i = 0; i < linenumber.Length; i++)
                            {
                                numberOfNavigation.Add(0);
                                targetFound.Add(false);
                            }

                            int lookahead = 0; 
                            int lookback = 1;  
                            int lookback2 = 1;
                            bool set = true;
                            bool decay = false;

                            if (set)
                            {
                                lookback = treeDepthThreshold; 
                                lookback2 = treeDepthThreshold; 
                            }

                            heuristicWeightComb configuration =
                                new heuristicWeightComb(set, decay, w0, lookahead, w1, w2, lookback, w3, lookback2, w4);

                            //recommendation trees building
                            foreach (var searchresultfull in gbuilder.InitialSearchResults)
                            {
                                if (targetSatisfied(targetFound) || stopCriteriaSatisfied(numberOfNavigation, stopLine))
                                    break;

                                var searchresult = searchresultfull.Item1;
                                
                                int number = Convert.ToInt32(searchresult.ProgramElement.DefinitionLineNumber);
                                CodeNavigationResult rootElement = new CodeNavigationResult(searchresult.ProgramElement, searchresult.Score, gbuilder.GetXElementFromLineNum(number));
                                NTree<CodeNavigationResult> rootTreeNode = new NTree<CodeNavigationResult>(rootElement);

                                TreeBuild(ref rootTreeNode, gbuilder, 0, configuration, ref targetFound,
                                    ref numberOfNavigation, ref targetSet, treeDepthThreshold, stopLine);
                            }

                            String outputStr = "Number of Navigation of ("
                                + set.ToString() + " "
                                + decay.ToString() + " "
                                + w0.ToString() + " "
                                + w1.ToString() + " "
                                + w2.ToString() + " "
                                + w3.ToString() + " "
                                + w4.ToString() + "): ";

                            //bool output = true;
                            int cnt = 0;
                            for (int i = 0; i < numberOfNavigation.Count; i++)
                            {                                
                                //if (numberOfNavigation[i] > stopLine)
                                //{
                                //    output = false;
                                //    break;
                                //}
                                if (numberOfNavigation[i] <= stopLine)
                                    cnt++;
                                outputStr += numberOfNavigation[i].ToString() + " ";
                            }

                            outputStr += "\t P = " +
                                (Convert.ToDouble(cnt) / Convert.ToDouble(numberOfNavigation.Count)).ToString();

                            //if(output)
                                Console.Write(outputStr);


                            gbuilder.CurrentPath.Clear();
                        }      

        }

        public struct targetProgramElement
        {
            public int relationLine;
            public string elementName;
            public ProgramElementRelation relationName;

            public targetProgramElement(int line, string name, ProgramElementRelation relation)
            {
                relationLine = line;
                elementName = name;
                relationName = relation;
            }

        }

        public struct heuristicWeightComb
        {
            public bool Decay;
            public bool Set;
            public double showBeforeW;
            public int searchResLookahead;
            public double AmongSearchResW;
            public double TopologyW;
            public int editDistanceLookback;
            public double EditDistanceW;
            public int dataFlowLookback;
            public double DataFlowW;

            public heuristicWeightComb(bool set, bool decay, double w0, int lookahead, double w1, double w2, 
                int lookback, double w3, int lookback2, double w4)
            {
                Decay = decay;
                Set = set;
                showBeforeW = w0;
                searchResLookahead = lookahead;
                AmongSearchResW = w1;
                TopologyW = w2;
                editDistanceLookback = lookback;
                EditDistanceW = w3;
                dataFlowLookback = lookback2;
                DataFlowW = w4;
            }

        }

        public bool targetSatisfied(List<bool> targetFound) //configurable
        {
            foreach (bool status in targetFound)
            {
                if (status == false)
                    return false;
            }

            return true;
        }

        public bool stopCriteriaSatisfied(List<int> numberOfNavigation, int stopLine)
        {
            foreach (int cumulativeRank in numberOfNavigation)
            {
                if (cumulativeRank > stopLine)
                    return true;
            }

            return false;
        }

        public void TreeBuild(ref NTree<CodeNavigationResult> rootNode, Context gbuilder,
            int depth, heuristicWeightComb config, ref List<bool> targetFound,
            ref List<int> numberOfNavigation, ref List<targetProgramElement> targetSet,
            int treeDepthThreshold, int stopLine)
        {
            //target found
            if (targetSatisfied(targetFound) || stopCriteriaSatisfied(numberOfNavigation, stopLine))
                return;

            for (int i = 0; i < numberOfNavigation.Count; i++)
            {
                if (targetFound[i] == false)
                    numberOfNavigation[i]++;
            }

            CodeNavigationResult rootElement = rootNode.getData();
            Console.WriteLine(rootElement.Name + ": " + rootElement.RelationLineNumberAsString
                + " " + rootElement.ProgramElementRelation.ToString() + " " + rootElement.Score.ToString()); //debugging
            for (int i = 0; i < targetSet.Count; i++)
            {
                targetProgramElement target = targetSet[i];
                if (targetFound[i] == true)
                    continue;

                if (rootElement.RelationLineNumber[0] == target.relationLine &&
                     rootElement.Name == target.elementName &&
                    // rootElement.ProgramElement.DefinitionLineNumber == target.relationLine
                     rootElement.ProgramElementRelation == target.relationName
                    )
                {
                    targetFound[i] = true;
                    break; //can't be another target
                }
            }

            gbuilder.CurrentPath.Add(rootElement as CodeSearchResult);
            depth++;

            if (depth >= treeDepthThreshold)
            {
                gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count - 1);
                depth--;
                return;
            }

            List<CodeNavigationResult> childrenElements
                = gbuilder.GetRecommendations(rootElement as CodeSearchResult, config.Set, config.Decay, 
                config.showBeforeW,
                config.searchResLookahead, config.AmongSearchResW, 
                config.TopologyW,
                config.editDistanceLookback, config.EditDistanceW,
                config.dataFlowLookback, config.DataFlowW);

            Console.WriteLine("   number of children: " + childrenElements.Count.ToString()); //debug
            if (childrenElements.Count == 0)
            {
                gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count - 1);
                depth--;
                return;
            }

            foreach (var child in childrenElements)
            {
                rootNode.addChild(child);
                NTree<CodeNavigationResult> newrootNode = rootNode.getChild(0);
                TreeBuild(ref newrootNode, gbuilder, depth, config, ref targetFound, ref numberOfNavigation,
                    ref targetSet, treeDepthThreshold, stopLine);
                rootNode.RemoveChild(newrootNode);
            }
                        
        }
        

        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationFreeMind";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\FreeMindTestFiles";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(5);
        }

    } //class end
     
} //namespace
