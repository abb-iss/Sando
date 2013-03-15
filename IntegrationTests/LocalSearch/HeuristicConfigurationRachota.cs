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
using System.Linq;

namespace Sando.IntegrationTests.LocalSearch
{
    [TestFixture]
    public class HeuristicConfigurationRachota : AutomaticallyIndexingTestClass
    {        
        [Test]
        public void rachotaTest()
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            string keywords = "fetch output stream";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);

            Context gbuilder = new Context();
            gbuilder.Intialize(@"..\..\Local Search\LocalSearch.UnitTests\TestFiles\SrcMLCSharpParser.cs");
            
            if(codeSearchResults.Count == 0)
                codeSearchResults = gbuilder.GetRecommendations().ToList();

            foreach (var searchRes in codeSearchResults)
                gbuilder.InitialSearchResults.Add(Tuple.Create(searchRes, 0));

        }

        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationRachota";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\LocaSearchTestFiles\\RachotaTestFiles";
        }
    }
}
