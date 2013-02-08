using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using NUnit.Framework;
using Sando.Core;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Searching;
using Sando.Indexer.Searching.Criteria;
using Sando.Parser;
using UnitTestHelpers;

namespace Sando.Indexer.UnitTests.Searching.Results
{
    public class SearchTester
    {
        private readonly SrcMLCSharpParser _parser;
        private readonly string _luceneTempIndexesDirectory;
        private readonly string _sandoAssemblyDirectoryPath;

        public SearchTester()
        {
            //set up generator
            _parser = new SrcMLCSharpParser(new ABB.SrcML.SrcMLGenerator(@"LIBS\SrcML"));
            _luceneTempIndexesDirectory = Path.Combine(Path.GetTempPath(), "basic");
            _sandoAssemblyDirectoryPath = _luceneTempIndexesDirectory;
            Directory.CreateDirectory(_luceneTempIndexesDirectory);
            TestUtils.ClearDirectory(_luceneTempIndexesDirectory);
        }



        public  bool HasResults(string methodNameToFind, List<Tuple<ProgramElement, float>> results)
        {
            foreach (var result in results)
            {
                var method = result.Item1 as MethodElement;
                if (method != null)
                {
                    if (method.Name.Equals(methodNameToFind))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public  List<Tuple<ProgramElement, float>> GetResults(string searchString, SolutionKey key)
        {
                var searcher = IndexerSearcherFactory.CreateSearcher();
                var criteria = new SimpleSearchCriteria();
                criteria.SearchTerms = new SortedSet<string>(searchString.Split(' ').ToList());
                var results = searcher.Search(criteria);
                return results;            
        }


        public  SolutionKey IndexFilesInDirectory(string solutionPath, out DocumentIndexer indexer)
        {

            var key = new SolutionKey(Guid.NewGuid(), solutionPath, _luceneTempIndexesDirectory, _sandoAssemblyDirectoryPath);
            ServiceLocator.RegisterInstance(key);
            indexer = DocumentIndexerFactory.CreateIndexer(AnalyzerType.Snowball);

            string[] files = Directory.GetFiles(solutionPath);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                var srcML = _parser.Parse(fullPath);
                foreach (var programElement in srcML)
                {
                    indexer.AddDocument(DocumentFactory.Create(programElement));
                }
            }
            indexer.CommitChanges();
            return key;
        }

        public  void CheckFolderForExpectedResults(string searchString, string methodNameToFind, string solutionPath)
        {
            
            Analyzer analyzer = new SimpleAnalyzer();
            DocumentIndexer indexer = null;
            try
            {
                var key = IndexFilesInDirectory(solutionPath, out indexer);
                var results = GetResults(searchString, key);
                Assert.IsTrue(HasResults(methodNameToFind, results), "Can't find expected results");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + ". " + ex.StackTrace);
            }
            finally
            {
                if (indexer != null)
                    indexer.Dispose(true);
            }
        }

        public static SearchTester Create()
        {
            return new SearchTester();
        }

     
    }
}
