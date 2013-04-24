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
    public class HeuristicConfigurationFreeMind2 : AutomaticallyIndexingTestClass
    {
        [Test]
        public void FreeMindTest2()
        {
            //SetTargetSet();
            
            string testfilePath = @"..\..\IntegrationTests\TestFiles\LocalSearchTestFiles\FreeMindTestFiles-orig\ControllerAdapter.java";
            int treeDepthThreshold = 5;
            int stopLine = 30;

            string keywords = "Saving failed"; //"Saving Failed";
            var codeSearcher = new CodeSearcher(new IndexerSearcher());            
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);

            int numberofNode = 0; //for debugging

            Context gbuilder = new Context(keywords);
            gbuilder.Intialize(testfilePath);

            Console.WriteLine(codeSearchResults.Count); //debugging

            if (codeSearchResults.Count == 0)
                codeSearchResults = gbuilder.GetRecommendations().ToList();

            foreach (var initialSearchRes in codeSearchResults)
            {
                if ((initialSearchRes.Type == "Method") || (initialSearchRes.Type == "Field"))
                {
                    gbuilder.InitialSearchResults.Add(Tuple.Create(initialSearchRes, 1));
                    Console.WriteLine(initialSearchRes.Name); //debugging
                }   
            }

            //       (search result) --> actionPerformed (definition 968)
     //                              --> mc.save(callby, 969)
     //                 --> save(definition, 373, same file, different class) 
     //                 --> save(getModel().getFile) (callby, 378)
     //                 --> save(definition, 560)
            List<targetProgramElement> targetSet = new List<targetProgramElement>();
            List<int> numberOfNavigation = new List<int>();
            List<bool> targetFound = new List<bool>();
            int[] linenumber = { 968, 373, 560};
            String[] elements = { "actionPerformed", "save", "save" };
            ProgramElementRelation[] relations = { ProgramElementRelation.Other, 
                                                   ProgramElementRelation.Other,
                                                   ProgramElementRelation.Other
                                                 };

            for (int i = 0; i < linenumber.Length; i++)
            {
                targetProgramElement target =
                new targetProgramElement(linenumber[i], elements[i], relations[i]);

                targetSet.Add(target);
                numberOfNavigation.Add(0);
                targetFound.Add(false);
            }

            for (double w0 = 1; w0 <= 1; w0++ )
                for (double w1 = 1; w1 <= 1; w1++)
                    for (double w2 = 1; w2 <= 1; w2++)
                        for (double w3 = 1; w3 <= 1; w3++)
                        {
                            int lookahead = 1;
                            int lookback = 3;
                            bool decay = false;

                            heuristicWeightComb configuration =
                                new heuristicWeightComb(decay, w0, lookahead, w1, w2, lookback, w3);

                            //recommendation trees building
                            foreach (var searchresultfull in gbuilder.InitialSearchResults)
                            {
                                if (targetSatisfied(targetFound) || stopCriteriaSatisfied(numberOfNavigation, stopLine))
                                    break;

                                var searchresult = searchresultfull.Item1;
                                
                                int number = Convert.ToInt32(searchresult.ProgramElement.DefinitionLineNumber);
                                CodeNavigationResult rootElement = new CodeNavigationResult(searchresult.ProgramElement, searchresult.Score, gbuilder.GetXElementFromLineNum(number));
                                NTree<CodeNavigationResult> rootTreeNode = new NTree<CodeNavigationResult>(rootElement);

                                //recommendTrees.Add(rootTreeNode);

                                TreeBuild(ref rootTreeNode, gbuilder, 0, configuration, ref targetFound,
                                    ref numberOfNavigation, ref targetSet, treeDepthThreshold, stopLine);
                            }

                            String outputStr = "Number of Navigation of ("
                                + decay.ToString() + " "
                                + w0.ToString() + " "
                                + w1.ToString() + " "
                                + w2.ToString() + " "
                                + w3.ToString() + "): ";

                            for (int i = 0; i < numberOfNavigation.Count; i++)
                            {
                                outputStr += numberOfNavigation[i].ToString() + " ";
                            }

                            Console.Write(outputStr);
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
            public bool showBeforeDecay;
            public double showBeforeW;
            public int searchResLookahead;
            public double AmongSearchResW;
            public double TopologyW;
            public int editDistanceLookback;
            public double EditDistanceW;

            public heuristicWeightComb(bool decay, double w0, int lookahead, double w1, double w2, int lookback, double w3)
            {
                showBeforeDecay = decay;
                showBeforeW = w0;
                searchResLookahead = lookahead;
                AmongSearchResW = w1;
                TopologyW = w2;
                editDistanceLookback = lookback;
                EditDistanceW = w3;
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
            if (targetSatisfied(targetFound) || stopCriteriaSatisfied(numberOfNavigation, stopLine))
                return;

            for (int i = 0; i < numberOfNavigation.Count; i++)
            {
                if (targetFound[i] == false)
                    numberOfNavigation[i]++;
            }

            CodeNavigationResult rootElement = rootNode.getData();
            Console.WriteLine("+[" + (depth + 1).ToString() + "] "
                + rootElement.Name + ": " + rootElement.RelationLineNumberAsString
                + " " + rootElement.ProgramElementRelation.ToString()); //debugging
            gbuilder.CurrentPath.Add(rootElement as CodeSearchResult);
            //Console.WriteLine("Last element of current path: " + gbuilder.CurrentPath.ElementAt(gbuilder.CurrentPath.Count - 1).Name); //debug
            depth++;
            
            for (int i = 0; i < targetSet.Count; i++)
            {
                targetProgramElement target = targetSet[i];
                if (targetFound[i] == true)
                    continue;

                if (rootElement.RelationLineNumber[0] == target.relationLine &&
                     rootElement.Name == target.elementName
                    && rootElement.ProgramElementRelation == target.relationName)
                {
                    targetFound[i] = true;
                    break; //can't be another target
                }
            }

            if (targetSatisfied(targetFound) || stopCriteriaSatisfied(numberOfNavigation, stopLine))
                return;

            if (depth >= treeDepthThreshold)
            {
                Console.WriteLine("-[" +depth.ToString() + "] "
                    + rootElement.Name + ": " + rootElement.RelationLineNumberAsString
                    + " " + rootElement.ProgramElementRelation.ToString()); //debugging
                gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count - 1);
                depth--;
                //Console.WriteLine("Last element of current path: " + gbuilder.CurrentPath.ElementAt(gbuilder.CurrentPath.Count - 1).Name); //debug
                return;
            }

            List<CodeNavigationResult> childrenElements
                = gbuilder.GetRecommendations(rootElement as CodeSearchResult, config.showBeforeDecay, config.showBeforeW,
                config.searchResLookahead, config.AmongSearchResW, config.TopologyW,
                config.editDistanceLookback, config.EditDistanceW);

            if (childrenElements.Count == 0)
            {
                Console.WriteLine("-[" +depth.ToString() + "] "
                    + rootElement.Name + ": " + rootElement.RelationLineNumberAsString
                    + " " + rootElement.ProgramElementRelation.ToString()); //debugging
                gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count - 1);
                depth--;
                //Console.WriteLine("Last element : " 
                //    + gbuilder.CurrentPath.ElementAt(gbuilder.CurrentPath.Count - 1).Name); //debug
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

            //for (int i = 0; i < rootNode.getChildNumber(); i++)
            //while (rootNode.getChildNumber() > 0)
            //{
            //    NTree<CodeNavigationResult> newrootNode = rootNode.getChild(0);
            //    TreeBuild(ref newrootNode, gbuilder, depth, config, ref targetFound, ref numberOfNavigation,
            //        ref targetSet, treeDepthThreshold, stopLine);
            //    rootNode.RemoveChild(newrootNode);
            //}
            
        }
        

        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationFreeMind";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\FreeMindTestFiles2";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(5);
        }

    } //class end
     
} //namespace
