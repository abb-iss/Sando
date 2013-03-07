using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using NUnit.Framework;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.Parser;
using UnitTestHelpers;
using ABB.SrcML.VisualStudio.SolutionMonitor;
using Sando.Core.Tools;

namespace Sando.Indexer.UnitTests.TestFiles.Searching.Results
{
    public class SearchTester
    {
        private readonly SrcMLCSharpParser _parser;
        private readonly string _luceneTempIndexesDirectory;        
        
        private DocumentIndexer _indexer;

        public static SearchTester Create()
        {
            return new SearchTester();
        }

        private SearchTester()
        {
            TestUtils.InitializeDefaultExtensionPoints();
            //set up generator
            _parser = new SrcMLCSharpParser(new ABB.SrcML.SrcMLGenerator(@"SrcML"));
            _luceneTempIndexesDirectory = PathManager.Instance.GetIndexPath(ServiceLocator.Resolve<SolutionKey>());
            Directory.CreateDirectory(_luceneTempIndexesDirectory);
            TestUtils.ClearDirectory(_luceneTempIndexesDirectory);
        }

        public void CheckFolderForExpectedResults(string searchString, string methodNameToFind, string solutionPath)
        {
            ServiceLocator.RegisterInstance<Analyzer>(new SnowballAnalyzer("English"));
            _indexer = new DocumentIndexer(TimeSpan.FromSeconds(1));
            ServiceLocator.RegisterInstance(_indexer);

            try
            {
                IndexFilesInDirectory(solutionPath);
                var results = GetResults(searchString);
                Assert.IsTrue(HasResults(methodNameToFind, results), "Can't find expected results");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + ". " + ex.StackTrace);
            }
            finally
            {
                if (_indexer != null)
                    _indexer.Dispose(true);
            }
        }

        private void IndexFilesInDirectory(string solutionPath)
        {

            var files = Directory.GetFiles(solutionPath);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                var srcMl = _parser.Parse(fullPath);
                foreach (var programElement in srcMl)
                {
                    _indexer.AddDocument(DocumentFactory.Create(programElement));
                }
            }
        }

        private IEnumerable<CodeSearchResult> GetResults(string searchString)
        {
            var searcher = new IndexerSearcher();
            var criteria = new SimpleSearchCriteria
                {
                    SearchTerms = new SortedSet<string>(searchString.Split(' ').ToList())
                };
            var results = searcher.Search(criteria);
            return results;
        }


        private bool HasResults(string methodNameToFind, IEnumerable<CodeSearchResult> results)
        {
            return results.Select(result => result.ProgramElement).OfType<MethodElement>().Any(method => method.Name.Equals(methodNameToFind));
        }
    }
}
