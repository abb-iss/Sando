using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class HeuristicConfiguration
    {
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

        public static bool targetSatisfied(List<bool> targetFound) //configurable
        {
            foreach (bool status in targetFound)
            {
                if (status == false)
                    return false;
            }

            return true;
        }

        public static bool stopCriteriaSatisfied(List<int> numberOfNavigation, int stopLine)
        {
            foreach (int cumulativeRank in numberOfNavigation)
            {
                if (cumulativeRank > stopLine)
                    return true;
            }

            return false;
        }

        public static void TreeBuild(ref NTree<CodeNavigationResult> rootNode, Context gbuilder,
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
                + " " + rootElement.ProgramElementRelation.ToString()); //debugging
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

            gbuilder.CurrentPath.Add(rootElement as CodeSearchResult);
            depth++;

            if (depth >= treeDepthThreshold)
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
                TreeBuild(ref newrootNode, gbuilder, depth, config, ref targetFound, ref numberOfNavigation,
                    ref targetSet, treeDepthThreshold, stopLine);
            }

            //gbuilder.CurrentPath.RemoveAt(gbuilder.CurrentPath.Count-1);
            //depth--;
        }
    }
}
