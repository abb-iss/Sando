using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Core;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.Parser;
using Sando.UnitTestHelpers;
using UnitTestHelpers;

namespace Sando.Indexer.UnitTests.Searching.Results
{
    [TestFixture]
    public class CsSimpleTest
    {
        
        [SetUp]
        public void ResetContract()
        {            
            contractFailed = false;
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetHandled();
                e.SetUnwind();
                contractFailed = true;
            };
        }

        [TearDown]
        public void CloseDocumentIndexer()
        {

        }

        private bool contractFailed;        

        [Test]
        public void CSSimple_OneFile_Passing()
        {            
            SearchTester.Create().CheckFolderForExpectedResults("plugin",  "EnsureOutputsLoaded",".\\TestFiles\\CS_1");
            SearchTester.Create().CheckFolderForExpectedResults("capture", "Capture", ".\\TestFiles\\CS_1");
        }

        //TODO - make this test pass
        public void CSSimple_OneFile_Failing()
        {
            SearchTester.Create().CheckFolderForExpectedResults("dispose", "Dispose", ".\\TestFiles\\CS_1");
        }
   
    }
}
