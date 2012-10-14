using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.Indexer.Searching.Criteria;
using Sando.Core.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sando.ExtensionContracts;
using Sando.UI.InterleavingExperiment;

namespace Sando.UI.View
{
    public class SearchManager
    {

        private static CodeSearcher _currentSearcher;
        private string _currentDirectory = "";
        private bool _invalidated = true;
        private ISearchResultListener _myDaddy;

        public SearchManager(ISearchResultListener daddy)
        {
            _myDaddy = daddy;
        }

        public static CodeSearcher GetCurrentSearcher()
        {
            return _currentSearcher;
        }
        private CodeSearcher GetSearcher(UIPackage myPackage)
        {
            CodeSearcher codeSearcher = _currentSearcher;
            if (codeSearcher == null || !myPackage.GetCurrentDirectory().Equals(_currentDirectory) || _invalidated)
            {
                _invalidated = false;
                _currentDirectory = myPackage.GetCurrentDirectory();
                codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(myPackage.GetCurrentSolutionKey()));
            }
            return codeSearcher;
        }

        public void Search(String searchString, SimpleSearchCriteria searchCriteria = null, bool interactive = true)
        {
            try
            {
                var returnString = "";
                if (!string.IsNullOrEmpty(searchString))
                {
                    var myPackage = UIPackage.GetInstance();
                    if (myPackage.GetCurrentDirectory() != null)
                    {
                        _currentSearcher = GetSearcher(myPackage);
                        IQueryable<CodeSearchResult> results = null;
                        if (ExtensionPointsRepository.IsInterleavingExperimentOn)
                        {
                            IQueryable<CodeSearchResult> resultsA = null;
                            IQueryable<CodeSearchResult> resultsB = null;
                            var taskA = new Task(() =>
                            {
                                ExtensionPointsRepository.ExpFlow.Value = ExperimentFlow.A;
                                resultsA = PerformSearch(searchString, searchCriteria, myPackage);
                            });
                            var taskB = new Task(() =>
                            {
                                ExtensionPointsRepository.ExpFlow.Value = ExperimentFlow.B;
                                resultsB = PerformSearch(searchString, searchCriteria, myPackage);
                            });
                            taskA.Start();
                            taskB.Start();

                            taskA.Wait();
                            taskB.Wait();

                            results = InterleavingExperimentManager.Instance.InterleaveResults(resultsA, resultsB);
                        }
                        else
                        {
                            results = PerformSearch(searchString, searchCriteria, myPackage);
                        }

                        if (results == null) return;
                        _myDaddy.Update(results);

                        if (!results.Any())
                        {
                            returnString = "No results found. " + returnString;
                        }
                        else if (returnString.Length == 0)
                        {
                            returnString = results.Count() + " results returned";
                        }
                        else
                        {
                            returnString = results.Count() + " results returned. " + returnString;
                        }
                        _myDaddy.UpdateMessage(returnString);
                    }
                    else
                    {
                        _myDaddy.UpdateMessage(
                            "Sando searches only the currently open Solution.  Please open a Solution and try again.");
                    }
                }
            }
            catch (Exception e)
            {
                _myDaddy.UpdateMessage(
                   "Invalid Query String - only complete words or partial words followed by a '*' are accepted as input.");
            }
        }

        private IQueryable<CodeSearchResult> PerformSearch(String searchString, SimpleSearchCriteria searchCriteria, UIPackage myPackage)
        {
            bool searchStringContainedInvalidCharacters = false;
            IQueryable<CodeSearchResult> results =
                _currentSearcher.Search(
                    GetCriteria(searchString, out searchStringContainedInvalidCharacters, searchCriteria),
                    GetSolutionName(myPackage)).AsQueryable();
            if (searchStringContainedInvalidCharacters)
            {
                _myDaddy.UpdateMessage(
                    "Invalid Query String - only complete words or partial words followed by a '*' are accepted as input.");
            }

            IResultsReorderer resultsReorderer =
                ExtensionPointsRepository.GetInstance(ExtensionPointsRepository.ExpFlow.Value).GetResultsReordererImplementation();
            results = resultsReorderer.ReorderSearchResults(results);
            return results;
        }

        private string GetSolutionName(UIPackage myPackage)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(myPackage.GetCurrentSolutionKey().GetSolutionPath());
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public void SearchOnReturn(object sender, KeyEventArgs e, String searchString, SimpleSearchCriteria searchCriteria)
        {
            if (e.Key == Key.Return)
            {
                Search(searchString, searchCriteria);
            }
        }

        public void MarkInvalid()
        {
            _invalidated = true;
        }

        #region Private Mthods
        /// <summary>
        /// Gets the criteria.
        /// </summary>
        /// <param name="searchString">Search string.</param>
        /// <returns>search criteria</returns>
        private SearchCriteria GetCriteria(string searchString, out bool searchStringContainedInvalidCharacters, SimpleSearchCriteria searchCriteria = null)
        {
            if (searchCriteria == null)
                searchCriteria = new SimpleSearchCriteria();
            var criteria = searchCriteria;
            criteria.NumberOfSearchResultsReturned = UIPackage.GetSandoOptions(UIPackage.GetInstance()).NumberOfSearchResultsReturned;
            searchString = ExtensionPointsRepository.GetInstance().GetQueryRewriterImplementation().RewriteQuery(searchString);
            searchStringContainedInvalidCharacters = WordSplitter.InvalidCharactersFound(searchString);
            List<string> searchTerms = WordSplitter.ExtractSearchTerms(searchString);
            criteria.SearchTerms = new SortedSet<string>(searchTerms);
            return criteria;
        }
        #endregion
    }

}
