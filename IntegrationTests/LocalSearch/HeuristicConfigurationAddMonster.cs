﻿using System;
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
using System.IO;

using System.Collections.Generic;

namespace Sando.IntegrationTests.LocalSearch
{
    [TestFixture]
    public class HeuristicConfigurationAddMonster : AutomaticallyIndexingTestClass
    {
        static targetProgramElement target = 
            new targetProgramElement(98, "AddMonster", ProgramElementRelation.CallBy);

        static int numberOfNavigation = 0;

        static bool targetFound = false;

        [Test]
        public void AddMonsterTest()
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            string keywords = "Add";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
            
            Context gbuilder = new Context(keywords);
            gbuilder.Intialize(@"..\..\IntegrationTests\TestFiles\LocalSearchTestFiles\AddMonsterTestFiles\CreatureManager.cs");
            foreach (var initialSearchRes in codeSearchResults)
            {
                if((initialSearchRes.Type == "Method") || (initialSearchRes.Type == "Field"))
                    gbuilder.InitialSearchResults.Add(Tuple.Create(initialSearchRes, 1));
            }

            //List<NTree<CodeNavigationResult>> recommendTrees = new List<NTree<CodeNavigationResult>>();

            for(int lookahead = 1; lookahead <= 2; lookahead++)
                for(double w1 = 1; w1 <=1; w1++)
                    for(double w2 = 1; w2 <=1; w2++)
                        for(int lookback = 1; lookback <=2; lookback++)
                            for (double w3 = 1; w3 <= 1; w3++)
                            {
                                heuristicWeightComb configuration =
                                    new heuristicWeightComb(lookahead, w1, w2, lookback, w3);

                                //recommendation trees building
                                foreach (var searchresult in codeSearchResults)
                                {
                                    if (targetFound == true)
                                        break;

                                    if ((searchresult.Type != "Method") && (searchresult.Type != "Field"))
                                        continue;

                                    int number = Convert.ToInt32(searchresult.ProgramElement.DefinitionLineNumber);
                                    CodeNavigationResult rootElement = new CodeNavigationResult(searchresult.ProgramElement, searchresult.Score, gbuilder.GetXElementFromLineNum(number));
                                    NTree<CodeNavigationResult> rootTreeNode = new NTree<CodeNavigationResult>(rootElement);

                                    //recommendTrees.Add(rootTreeNode);

                                    TreeBuild(ref rootTreeNode, gbuilder, 0, configuration);
                                }

                                Console.WriteLine("Number of Navigation of (" 
                                    + lookahead.ToString() + " "
                                    + w1.ToString() + " "
                                    + w2.ToString() + " "
                                    + lookback.ToString() + " "
                                    + w3.ToString() + "):"
                                    + numberOfNavigation.ToString());
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

            //public static bool operator ==(targetProgramElement t1, targetProgramElement t2)
            //{
            //    return t1.relationLine == t2.relationLine 
            //        && t1.elementName == t2.elementName 
            //        && t1.relationName == t2.relationName;
            //}
        }

        public struct heuristicWeightComb
        {
            public int searchResLookahead;
            public double AmongSearchResW;
            public double TopologyW;
            public int editDistanceLookback;
            public double EditDistanceW;

            public heuristicWeightComb(int lookahead, double w1, double w2, int lookback, double w3)
            {
                searchResLookahead = lookahead;
                AmongSearchResW = w1;
                TopologyW = w2;
                editDistanceLookback = lookback;
                EditDistanceW = w3;
            }

        }
        
        public void TreeBuild(ref NTree<CodeNavigationResult> rootNode, Context gbuilder, 
            int depth, heuristicWeightComb config)
        {
            //target found
            if (targetFound == true)
                return;

            numberOfNavigation++;

            CodeNavigationResult rootElement = rootNode.getData();
            if (rootElement.RelationLineNumber[0] == target.relationLine
                && rootElement.Name == target.elementName
                && rootElement.ProgramElementRelation == target.relationName)
            {
                targetFound = true;
                return;
            }

            gbuilder.CurrentPath.Add(rootElement as CodeSearchResult);
            depth++;            

            if (depth >= 3)
            {
                gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count - 1);
                depth--;
                return;
            }

            List<CodeNavigationResult> childrenElements 
                = gbuilder.GetRecommendations(rootElement as CodeSearchResult,
                config.searchResLookahead, config.AmongSearchResW, config.TopologyW,
                config.editDistanceLookback, config.EditDistanceW);

            if (childrenElements.Count == 0)
            {
                gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count - 1);
                depth--;
                return;
            }

            foreach (var child in childrenElements)
            {
                rootNode.addChild(child);
            }
            
            for (int i = 0; i < rootNode.getChildNumber(); i++)
            {
                NTree<CodeNavigationResult> newrootNode = rootNode.getChild(i);
                TreeBuild(ref newrootNode, gbuilder, depth, config);
            }

            //gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count-1);
            //depth--;
        }
        
        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationAddMonster";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\LocalSearchTestFiles\\AddMonsterTestFiles";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(5);
        }
    }


    public class NTree<T>
    {
        T data;
        LinkedList<NTree<T>> children;

        public NTree(T data)
        {
            this.data = data;
            children = new LinkedList<NTree<T>>();
        }

        public T getData()
        {
            return this.data;
        }

        public void addChild(T data)
        {
            children.AddLast(new NTree<T>(data));
        }

        public NTree<T> getChild(int i)
        {
            foreach (NTree<T> n in children)
                if (i-- == 0) return n;
            return null;            
        }

        public int getChildNumber()
        {
            return children.Count;
        }
        
    }
}
