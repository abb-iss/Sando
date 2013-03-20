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

namespace Sando.IntegrationTests.LocalSearch
{
    //target:
    /*
     * 1. MindMapModel.java
     * "Saving Failed" --> save (definition, 245) --> SaveInternal (callby, 250)
     *                 --> getXml (callby, 260) --> getXml (callby, 303)
     *                 --> getXml (callby, 286) --> getXml (callby, 298)
     *                 
     * 2. ControllerAdaptor.java
     * "Saving Failed" --> (search result) --> mc.Save(control-dependent 969)
     *                 --> Save(definition, 373) --> Save(getModel().getFile) (callby, 378)
     *                 --> Save(definition, 560)
    */

    [TestFixture]
    public class HeuristicConfigurationFreeMind : AutomaticallyIndexingTestClass
    {
        [Test]
        public void FreeMindTest()
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            string keywords = "Saving Failed";
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
            

        }

        public override string GetIndexDirName()
        {
            return "HeuristicConfigurationFreeMind";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\IntegrationTests\\TestFiles\\LocalSearchTestFiles\\FreeMindTestFiles";
        }
    }
}
