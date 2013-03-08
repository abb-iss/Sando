﻿using System;
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
using Sando.UI.View;
using System.Diagnostics;
using System.Text;

namespace Sando.IntegrationTests.Search
{
    public class AutomaticallyIndexingTestClass : ISrcMLGlobalService, ISearchResultListener
    {

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
            WaitForAllFilesToBeCommitted(filesInThisDirectory);
            ServiceLocator.Resolve<DocumentIndexer>().ForceReaderRefresh();
        }

        private void WaitForAllFilesToBeCommitted(string filesInThisDirectory)
        {            
            int numFiles = 0;
            int updatedFiles = -1;
            while (updatedFiles != numFiles && numFiles <=0)
            {
                Thread.Sleep(int.Parse(GetTimeToCommit().Value.TotalMilliseconds*1.5+""));
                numFiles = updatedFiles;
                updatedFiles = ServiceLocator.Resolve<DocumentIndexer>().GetNumberOfIndexedDocuments();
            }
            Thread.Sleep(int.Parse(GetTimeToCommit().Value.TotalMilliseconds * 2 + ""));
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
            var currentIndexer = new DocumentIndexer(TimeSpan.FromSeconds(1), GetTimeToCommit());
            ServiceLocator.RegisterInstance(currentIndexer);
            ServiceLocator.RegisterInstance(new IndexUpdateManager());
            currentIndexer.ClearIndex();            
            ServiceLocator.Resolve<InitialIndexingWatcher>().InitialIndexingStarted();
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
            ServiceLocator.RegisterInstance<ISandoOptionsProvider>(new FakeOptionsProvider(_indexPath,40));
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
            var methodSearchResult = CheckExistance(keywords, predicate);
            CheckRanking(keywords, expectedLowestRank, methodSearchResult);
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

        private string PrintFailInformation()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine("Indexed Documents: "+ ServiceLocator.Resolve<DocumentIndexer>().GetNumberOfIndexedDocuments());
            foreach(var file in GetFileList(GetFilesDirectory()))
                info.AppendLine("file: "+file);
            if (_results != null)
                foreach (var result in _results)
                    info.AppendLine(result.Name+" in "+ result.FileName);
            return info.ToString();
        }

        private List<CodeSearchResult> GetResults(string keywords)
        {
            SearchManager manager = new SearchManager(this);
            _results = null;
            manager.Search(keywords);
            while (_results == null)
                Thread.Sleep(50);
            return _results;
        }

        public System.Xml.Linq.XElement GetXElementForSourceFile(string sourceFilePath)
        {
            return _srcMLArchive.GenerateXmlAndXElementForSource(sourceFilePath);
        }

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
        private List<CodeSearchResult> _results;


        public void StopMonitoring()
        {
            throw new NotImplementedException();
        }

        public void Update(System.Linq.IQueryable<CodeSearchResult> results)
        {
            var newResults = new List<CodeSearchResult>();
            foreach(var result in results)
                 newResults.Add(result);
            _results = newResults;
        }

        public void UpdateMessage(string message)
        {
            //throw new NotImplementedException();
        }

        public class FakeOptionsProvider : ISandoOptionsProvider
        {
            private string _myIndex;
            private int _myResultsNumber;

            public FakeOptionsProvider(string index, int num)
            {
                _myIndex = index;
                _myResultsNumber = num;
            }

            public SandoOptions GetSandoOptions()
            {
                return new SandoOptions(_myIndex,_myResultsNumber);
            }
        }
    }
}
