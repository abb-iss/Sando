using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using NUnit.Framework;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SearchContracts;
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
using Sando.UI.View;
using System.Diagnostics;
using System.Text;
using Sando.Indexer.Documents;
using Lucene.Net.Analysis.Standard;
using Sando.Core.QueryRefomers;
using Sando.UI;

namespace Sando.IntegrationTests.Search
{
    public class AutomaticallyIndexingTestClass : ISrcMLGlobalService, ISearchResultListener
    { 
        public event EventHandler<IsReadyChangedEventArgs> IsReadyChanged;

        [TestFixtureSetUp]
        public void Setup()
        {        
            IndexSpecifiedFiles(GetFilesDirectory(), GetIndexDirName());
        }

        public virtual TimeSpan? GetTimeToCommit()
        {
            return null;
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
            _handler.WaitForIndexing();
            ServiceLocator.Resolve<DocumentIndexer>().ForceReaderRefresh();
            Thread.Sleep((int)GetTimeToCommit().Value.TotalMilliseconds*4);
            ServiceLocator.Resolve<DocumentIndexer>().ForceReaderRefresh();
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
            done = true;
        }

        private List<string> GetFileList(string filesInThisDirectory, List<string> incoming = null)
        {
            if (filesInThisDirectory.EndsWith("LIBS") || filesInThisDirectory.EndsWith("bin") || filesInThisDirectory.EndsWith("Debug"))
                return incoming;
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
            SwumManager.Instance.Initialize(PathManager.Instance.GetIndexPath(ServiceLocator.Resolve<Sando.Core.Tools.SolutionKey>()), false);
            SwumManager.Instance.Archive = _srcMLArchive;
        }


        private void CreateArchive(string filesInThisDirectory)
        {
            var srcMlArchiveFolder = Path.Combine(_indexPath, "archive");
            var srcMLFolder = Path.Combine(".", "SrcML", "CSharp");
            Directory.CreateDirectory(srcMlArchiveFolder);
            var generator = new SrcMLGenerator(TestUtils.SrcMLDirectory);
            _srcMLArchive = new SrcMLArchive(_indexPath,  false, generator);
        }



        private void CreateIndexer()
        {
            ServiceLocator.Resolve<UIPackage>();

            ServiceLocator.RegisterInstance(new IndexFilterManager());

            PerFieldAnalyzerWrapper analyzer = new PerFieldAnalyzerWrapper(new SnowballAnalyzer("English"));            
            analyzer.AddAnalyzer(SandoField.Source.ToString(), new KeywordAnalyzer());
            analyzer.AddAnalyzer(SandoField.AccessLevel.ToString(), new KeywordAnalyzer());
            analyzer.AddAnalyzer(SandoField.ProgramElementType.ToString(), new KeywordAnalyzer());
            ServiceLocator.RegisterInstance<Analyzer>(analyzer);

            var currentIndexer = new DocumentIndexer(TimeSpan.FromSeconds(10), GetTimeToCommit());
            ServiceLocator.RegisterInstance(currentIndexer);
            ServiceLocator.RegisterInstance(new IndexUpdateManager());
            currentIndexer.ClearIndex();            
            ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingStarted();

            var dictionary = new DictionaryBasedSplitter();
            dictionary.Initialize(PathManager.Instance.GetIndexPath(ServiceLocator.Resolve<SolutionKey>()));

            var reformer = new QueryReformerManager(dictionary);
            reformer.Initialize(null);
            ServiceLocator.RegisterInstance(reformer);

            var history = new SearchHistory();
            history.Initialize(PathManager.Instance.GetIndexPath
                (ServiceLocator.Resolve<SolutionKey>()));
            ServiceLocator.RegisterInstance(history);

        }



        private void CreateKey(string filesInThisDirectory)
        {
            Directory.CreateDirectory(_indexPath);
            var key = new Sando.Core.Tools.SolutionKey(Guid.NewGuid(), filesInThisDirectory);
            ServiceLocator.RegisterInstance(key);
        }

        private void CreateSystemWideDefaults(string indexDirName)
        {
            _indexPath = Path.Combine(Path.GetTempPath(), indexDirName);
            TestUtils.InitializeDefaultExtensionPoints();
            ServiceLocator.RegisterInstance<ISandoOptionsProvider>(new FakeOptionsProvider(_indexPath,40,false));
            ServiceLocator.RegisterInstance(new SrcMLArchiveEventsHandlers());
            ServiceLocator.RegisterInstance(new InitialIndexingWatcher());
        }


        [TestFixtureTearDown]
        public void TearDown()
        {
            _srcMLArchive.Dispose();
            ServiceLocator.Resolve<IndexFilterManager>().Dispose();
            ServiceLocator.Resolve<DocumentIndexer>().Dispose();
            DeleteTestDirectoryContents();
        }

        private void DeleteTestDirectoryContents()
        {
            var deleted = false;
            while (!deleted)
            {
                try
                {
                    Directory.Delete(_indexPath, true);
                    deleted = true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private string _indexPath;
        private SrcMLArchive _srcMLArchive;
        private SrcMLArchiveEventsHandlers _handler;

        protected List<CodeSearchResult> EnsureRankingPrettyGood(string keywords, Predicate<CodeSearchResult> predicate, int expectedLowestRank)
        {
            _results = GetResults(keywords);
            if (expectedLowestRank > 0)
            {
                var methodSearchResult = CheckExistance(keywords, predicate);
                CheckRanking(keywords, expectedLowestRank, methodSearchResult);
            }
            return _results;
        }

        private void CheckRanking(string keywords, int expectedLowestRank, CodeSearchResult methodSearchResult)
        {
            var rank = _results.IndexOf(methodSearchResult) + 1;
            Assert.IsTrue(rank <= expectedLowestRank,
                          "Searching for " + keywords + " doesn't return a result in the top " + expectedLowestRank + "; rank=" +
                          rank);
        }

        private CodeSearchResult CheckExistance(string keywords, Predicate<CodeSearchResult> predicate)
        {
            var methodSearchResult = _results.Find(predicate);
            if (methodSearchResult == null)
            {
                string info = PrintFailInformation();
                Assert.Fail("Failed to find relevant search result for search: " + keywords+"\n"+info);                
            }
            return methodSearchResult;
        }

        public string PrintFailInformation(bool includeFiles = true)
        {
            StringBuilder info = new StringBuilder();
            if (includeFiles)
            {
                info.AppendLine("Indexed Documents: " + ServiceLocator.Resolve<DocumentIndexer>().GetNumberOfIndexedDocuments());
                foreach (var file in GetFileList(GetFilesDirectory()))
                    info.AppendLine("file: " + file);
            }
            if (_results != null)
                foreach (var result in _results)
                    info.AppendLine(result.Name+" in "+ result.FileName);
            return info.ToString();
        }

        private List<CodeSearchResult> GetResults(string keywords)
        {
            var manager = SearchManagerFactory.GetNewBackgroundSearchManager();
            manager.AddListener(this);
            _results = null;
            manager.Search(keywords);
            int i = 0;
            while (_results == null)
            {
                Thread.Sleep(50);
                i++;
                if (i > 100)
                    break;
            }
            return _results;
        }

        public System.Xml.Linq.XElement GetXElementForSourceFile(string sourceFilePath)
        {
            return _srcMLArchive.GetXElementForSourceFile(sourceFilePath);
        }

        public ISrcMLArchive GetSrcMLArchive()
        {
            throw new NotImplementedException();
        }


        public event EventHandler<EventArgs> MonitoringStopped;

        public event EventHandler<FileEventRaisedArgs> SourceFileChanged;


        public event EventHandler<EventArgs> StartupCompleted;
        private List<CodeSearchResult> _results;
        protected string _myMessage;
        private bool done = false;


        public void StopMonitoring()
        {
            throw new NotImplementedException();
        }

        public void Update(string searchString, IQueryable<CodeSearchResult> results)
        {
            var newResults = new List<CodeSearchResult>();
            foreach(var result in results)
                 newResults.Add(result);
            _results = newResults;
        }

        public void UpdateMessage(string message)
        {
            _myMessage = message;            
        }

        public void UpdateRecommendedQueries(IQueryable<string> queries)
        {
            
        }

        public class FakeOptionsProvider : ISandoOptionsProvider
        {
            private string _myIndex;
            private int _myResultsNumber;
			private bool _myAllowLogs;

            public FakeOptionsProvider(string index, int num, bool allowLogs)
            {
                _myIndex = index;
                _myResultsNumber = num;
				_myAllowLogs = allowLogs;
            }

            public SandoOptions GetSandoOptions()
            {
                return new SandoOptions(_myIndex,_myResultsNumber, _myAllowLogs);
            }
        }


       

        public void StartMonitoring()
        {
            throw new NotImplementedException();
        }

        public ABB.SrcML.Data.DataRepository GetDataRepository() {
            throw new NotImplementedException();
        }

        public bool IsReady
        {
            get { return done; }
        }

        public void AddDirectoryToMonitor(string pathToDirectory)
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<string> MonitoredDirectories
        {
            get { throw new NotImplementedException(); }
        }

        public void RemoveDirectoryFromMonitor(string pathToDirectory)
        {
            throw new NotImplementedException();
        }

        public double ScanInterval
        {
            get
            {
                return 60;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
