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
using System.IO;

using System.Collections.Generic;

namespace Sando.IntegrationTests.LocalSearch
{
    [TestFixture]
    public class HeuristicConfigurationAddMonster : AutomaticallyIndexingTestClass
    {
        [Test]
        public void AddMonsterTest()
        {
            targetProgramElement target = 
                new targetProgramElement(98, "AddMonster", ProgramElementRelation.CallBy);

            int numberOfNavigation = 0;

            bool targetFound = false;
            
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
                                    new heuristicWeightComb(false, 1, lookahead, w1, w2, lookback, w3);

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

                                    TreeBuild(ref rootTreeNode, gbuilder, 0, configuration,
                                        ref targetFound, ref numberOfNavigation, ref target);
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
        
        private void TreeBuild(ref NTree<CodeNavigationResult> rootNode, Context gbuilder,
            int depth, heuristicWeightComb config, ref bool targetFound, ref int numberOfNavigation,
            ref targetProgramElement target)
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
                = gbuilder.GetRecommendations(rootElement as CodeSearchResult, config.showBeforeDecay, config.showBeforeW,
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
                TreeBuild(ref newrootNode, gbuilder, depth, config, ref targetFound, 
                    ref numberOfNavigation, ref target);
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
            return "..\\..\\IntegrationTests\\TestFiles\\AddMonsterTestFiles"; //AddMonsterTestFiles
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(5);
        }
    }
        
}
