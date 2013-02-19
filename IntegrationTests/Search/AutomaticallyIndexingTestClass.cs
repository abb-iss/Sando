using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using NUnit.Framework;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.UI.Monitoring;
using UnitTestHelpers;
using Sando.Recommender;
using Sando.Indexer.IndexFiltering;
using Sando.UI.Options;
using Configuration.OptionsPages;
using ABB.SrcML.VisualStudio.SolutionMonitor;
using ABB.SrcML;
using System.Threading;
using Sando.Core.Tools;
using Sando.Indexer.Documents;
using Lucene.Net.Analysis.Standard;

namespace Sando.IntegrationTests.Search
{
    public class AutomaticallyIndexingTestClass
    {

        [TestFixtureSetUp]
        public void Setup()
        {
            IndexSpecifiedFiles(GetFilesDirectory(), GetIndexDirName());
        }

        public virtual string GetIndexDirName()
        {
            throw new NotImplementedException();
        }

        public virtual string GetFilesDirectory()
        {
            throw new System.NotImplementedException();
        }

        private void IndexSpecifiedFiles(string filesInThisDirectory, string indexDirName)
        {
            CreateSystemWideDefaults(indexDirName);
            CreateKey(filesInThisDirectory);
            CreateIndexer();
            SolutionMonitorFactory.CreateMonitor();
            CreateGenerator();
            CreateArchive(filesInThisDirectory);
            AddArchiveListeners();
            CreateSwum();

            //start the monitoring and wait for the initial indexing to finish before preceeding
            _srcMLArchive.StartWatching();
            while (ServiceLocator.Resolve<InitialIndexingWatcher>().IsInitialIndexingInProgress())
            {
                Thread.Sleep(500);
            }
            if (GetTimeToCommit() != null)
            {
                Thread.Sleep(Convert.ToInt32(GetTimeToCommit().GetValueOrDefault().TotalMilliseconds));
            }
        }

        private void CreateSwum()
        {
            SwumManager.Instance.Initialize(PathManager.Instance.GetIndexPath(ServiceLocator.Resolve<SolutionKey>()), false);
            SwumManager.Instance.Archive = _srcMLArchive;
        }

        private void AddArchiveListeners()
        {
            var srcMLArchiveEventsHandlers = ServiceLocator.Resolve<SrcMLArchiveEventsHandlers>();
            _srcMLArchive.SourceFileChanged += srcMLArchiveEventsHandlers.SourceFileChanged;
            _srcMLArchive.StartupCompleted += srcMLArchiveEventsHandlers.StartupCompleted;
            _srcMLArchive.MonitoringStopped += srcMLArchiveEventsHandlers.MonitoringStopped;
        }

        private void CreateArchive(string filesInThisDirectory)
        {
            var srcMlArchiveFolder = Path.Combine(_indexPath, "archive");
            Directory.CreateDirectory(srcMlArchiveFolder);
            var fakeFiles = new StaticFileList(new[] {"C:\\Temp\\Temp.txt"});
            var filesToWatch = new StaticFileList(Path.GetFullPath(filesInThisDirectory));

            //FILTERING LIST TEMPORARILY
            //DUE TO SRCML.NET BUG
            var list = filesToWatch.GetMonitoredFiles(null);
            var filteredList = new List<string>();
            var fake = new SrcMLArchive(fakeFiles, srcMlArchiveFolder, _generator);
            foreach (var file in list)
            {
                if (fake.IsValidFileExtension(file))
                    filteredList.Add(file);
            }

            _srcMLArchive = new SrcMLArchive(new StaticFileList(filteredList.ToArray()), srcMlArchiveFolder, _generator);
        }

        private void CreateGenerator()
        {
            string src2SrcmlDir = Path.Combine(".", "LIBS", "SrcML");
            _generator = new SrcMLGenerator(src2SrcmlDir);
        }

        private void CreateIndexer()
        {
            
            ServiceLocator.RegisterInstance(new IndexFilterManager());
            PerFieldAnalyzerWrapper analyzer =
                    new PerFieldAnalyzerWrapper(new SnowballAnalyzer("English"));
            analyzer.AddAnalyzer(SandoField.FullFilePath.ToString(), new StandardAnalyzer());
            ServiceLocator.RegisterInstance<Analyzer>(analyzer);
            var currentIndexer = new DocumentIndexer(TimeSpan.FromSeconds(10), GetTimeToCommit());
            ServiceLocator.RegisterInstance(currentIndexer);
            ServiceLocator.RegisterInstance(new IndexUpdateManager());
            currentIndexer.ClearIndex();
            ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingStarted();
        }

        public virtual TimeSpan? GetTimeToCommit()
        {
            return null;
        }

        private void CreateKey(string filesInThisDirectory)
        {
            Directory.CreateDirectory(_indexPath);
            var key = new SolutionKey(Guid.NewGuid(), filesInThisDirectory);
            ServiceLocator.RegisterInstance(key);
        }

        private void CreateSystemWideDefaults(string indexDirName)
        {
            _indexPath = Path.Combine(Path.GetTempPath(), indexDirName);
            TestUtils.InitializeDefaultExtensionPoints();
            ServiceLocator.RegisterInstance<ISandoOptionsProvider>(new SandoOptionsProvider());
            ServiceLocator.RegisterInstance(new SrcMLArchiveEventsHandlers());
            ServiceLocator.RegisterInstance(new InitialIndexingWatcher());
        }


        [TestFixtureTearDown]
        public void TearDown()
        {
            _srcMLArchive.Dispose();
            ServiceLocator.Resolve<IndexFilterManager>().Dispose();
            ServiceLocator.Resolve<DocumentIndexer>().Dispose();
            Directory.Delete(_indexPath, true);
        }

        private string _indexPath;
        private SrcMLArchive _srcMLArchive;
        private SrcMLGenerator _generator;

        protected List<CodeSearchResult> EnsureRankingPrettyGood(string keywords, Predicate<CodeSearchResult> predicate, int expectedLowestRank)
        {
            var codeSearcher = new CodeSearcher(new IndexerSearcher());
            List<CodeSearchResult> codeSearchResults = codeSearcher.Search(keywords);
            var methodSearchResult = codeSearchResults.Find(predicate);
            if (methodSearchResult == null)
            {
                Assert.Fail("Failed to find relevant search result for search: " + keywords);
            }

            var rank = codeSearchResults.IndexOf(methodSearchResult) + 1;
            Assert.IsTrue(rank <= expectedLowestRank,
                          "Searching for " + keywords + " doesn't return a result in the top " + expectedLowestRank + "; rank=" +
                          rank);

            return codeSearchResults;
        }
    }
}
