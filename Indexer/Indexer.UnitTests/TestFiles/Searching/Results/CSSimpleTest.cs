using System.Diagnostics.Contracts;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.Indexer.Searching;
using Sando.Indexer.UnitTests.TestFiles.Searching.Results;

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
            ExtensionPointsRepository extensionPointsRepository = ExtensionPointsRepository.Instance;
            extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter());
            extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());
            extensionPointsRepository.RegisterQueryRewriterImplementation(new DefaultQueryRewriter());
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
