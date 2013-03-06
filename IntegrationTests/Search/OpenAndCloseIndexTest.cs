using NUnit.Framework;
using Sando.DependencyInjection;
using Sando.Indexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.IntegrationTests.Search
{
    [TestFixture]
    public class OpenAndCloseIndexTest: AutomaticallyIndexingTestClass
    {

        [Test]
        public void CloseAndReopen()
        {
            ServiceLocator.Resolve<DocumentIndexer>().Dispose();
            var currentIndexer = new DocumentIndexer(TimeSpan.FromSeconds(10), GetTimeToCommit());
            ServiceLocator.RegisterInstance(currentIndexer);
            Assert.IsTrue(currentIndexer.GetNumberOfIndexedDocuments() > 5, "The index is being destroyed when it is closed and reopened");
        }

        public override string GetIndexDirName()
        {
            return "OpenAndCloseTest";
        }

        public override string GetFilesDirectory()
        {
            return "..\\..\\Parser";
        }

        public override TimeSpan? GetTimeToCommit()
        {
            return TimeSpan.FromSeconds(10);
        }

    }
}
