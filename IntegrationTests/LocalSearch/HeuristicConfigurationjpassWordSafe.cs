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
using System.Collections.Generic;
using Sando.LocalSearch;

namespace Sando.IntegrationTests.LocalSearch
{
    [TestFixture]
    public class HeuristicConfigurationhpassWordSafe : AutomaticallyIndexingTestClass
    {
        [Test]
        public void jpassWordSafeTest()
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            string keywords = "fetch output stream";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
        }

        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationhpassWordSafe";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\LocaSearchTestFiles\\jpassWordSafeTestFiles";
        }
    }
}
