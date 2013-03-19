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
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            string keywords = "Add";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
            int a = codeSearchResults.Count;



        }

        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationAddMonster";
        }

        public override string GetFilesDirectory()
        {
            return Path.GetFullPath("..\\..\\IntegrationTests\\TestFiles\\LocalSearchTestFiles\\AddMonsterTestFiles");
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(1);
        }
    }
}
