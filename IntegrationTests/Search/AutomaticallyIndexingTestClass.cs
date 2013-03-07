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
using ABB.SrcML.VisualStudio.SrcMLService;

namespace Sando.IntegrationTests.Search
{
    public class AutomaticallyIndexingTestClass : ISrcMLGlobalService
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
            filesInThisDirectory = Path.GetFullPath(filesInThisDirectory);
            CreateSystemWideDefaults(indexDirName);
            CreateKey(filesInThisDirectory);
            CreateIndexer();
            CreateArchive(filesInThisDirectory);            
            CreateSwum();
            AddFilesToIndex(filesInThisDirectory);
            WaitForCommit(filesInThisDirectory);
        }

        private void WaitForCommit(string filesInThisDirectory)
        {            
            int numFiles = 0;
            int updatedFiles = -1;
            while (updatedFiles != numFiles)
            {
                Thread.Sleep(int.Parse(GetTimeToCommit().Value.TotalMilliseconds*1.5+""));
                numFiles = updatedFiles;
                updatedFiles = ServiceLocator.Resolve<DocumentIndexer>().GetNumberOfIndexedDocuments();
            }
        }

        private void AddFilesToIndex(string filesInThisDirectory)
        {
            _handler = new SrcMLArchiveEventsHandlers();
            var files = GetFileList(filesInThisDirectory);
            foreach (var file in files)
            {
                if (Path.GetExtension(Path.GetFullPath(file)).Equals(".cs") ||
                    Path.GetExtension(Path.GetFullPath(file)).Equals(".cpp") ||
                    Path.GetExtension(Path.GetFullPath(file)).Equals(".c") ||
                    Path.GetExtension(Path.GetFullPath(file)).Equals(".h") ||
                    Path.GetExtension(Path.GetFullPath(file)).Equals(".cxx")
                    )
                    _handler.SourceFileChanged(this, new FileEventRaisedArgs(FileEventType.FileAdded, file));
            }
        }

        private List<string> GetFileList(string filesInThisDirectory, List<string> incoming = null)
        {
            if (incoming == null)
                incoming = new List<string>();
            incoming.AddRange(Directory.EnumerateFiles(filesInThisDirectory));
            var dirs = new List<string>();
            dirs.AddRange(Directory.EnumerateDirectories(filesInThisDirectory));
            foreach (var dir in dirs)
                GetFileList(dir, incoming);
            return incoming;
        }

        private void CreateSwum()
        {
            SwumManager.Instance.Initialize(PathManager.Instance.GetIndexPath(ServiceLocator.Resolve<SolutionKey>()), false);
            SwumManager.Instance.Archive = _srcMLArchive;
        }


        private void CreateArchive(string filesInThisDirectory)
        {
            var srcMlArchiveFolder = Path.Combine(_indexPath, "archive");
            var srcMLFolder = Path.Combine(".", "SrcML", "CSharp");
            Directory.CreateDirectory(srcMlArchiveFolder);
            var generator = new SrcMLGenerator(Path.GetFullPath(srcMLFolder));
            _srcMLArchive = new SrcMLArchive(_indexPath,  false, generator);
        }



        private void CreateIndexer()
        {
            ServiceLocator.RegisterInstance(new IndexFilterManager());
            ServiceLocator.RegisterInstance<Analyzer>(new SnowballAnalyzer("English"));
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
        private SrcMLArchiveEventsHandlers _handler;

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

        public System.Xml.Linq.XElement GetXElementForSourceFile(string sourceFilePath)
        {
            return _srcMLArchive.GenerateXmlAndXElementForSource(sourceFilePath);
        }

////////////////////////////////////////////////////////////////////////////////////////////////

        public SrcMLArchive GetSrcMLArchive()
        {
            throw new NotImplementedException();
        }


        public event EventHandler<EventArgs> MonitoringStopped;

        public event EventHandler<FileEventRaisedArgs> SourceFileChanged;

        public void StartMonitoring(string srcMlArchiveDirectory, bool useExistingSrcML, string srcMLBinaryDirectory)
        {
            throw new NotImplementedException();
        }

        public void StartMonitoring()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> StartupCompleted;


        public void StopMonitoring()
        {
            throw new NotImplementedException();
        }
    }
}
