using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Configuration.OptionsPages;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.Indexer.Searching.Criteria;
using Sando.Core.Tools;
using System.Collections.Generic;
using Sando.Core.Extensions.Logging;
using Sando.UI.Monitoring;
using Sando.UI.Options;

namespace Sando.UI.View
{
    public class SearchManager
    {

        private CodeSearcher _currentSearcher;
        private string _currentDirectory = "";
        private bool _invalidated = true;
        private ISearchResultListener _myDaddy;

        public SearchManager(ISearchResultListener daddy)
        {
            _myDaddy = daddy;
        }

        private CodeSearcher GetSearcher()
        {
            CodeSearcher codeSearcher = _currentSearcher;
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            if (codeSearcher == null || !solutionKey.IndexPath.Equals(_currentDirectory) || _invalidated)
            {
                _invalidated = false;
                _currentDirectory = solutionKey.IndexPath;
                codeSearcher = new CodeSearcher(new IndexerSearcher());
            }
            return codeSearcher;
        }

        public string Search(String searchString, SimpleSearchCriteria searchCriteria = null, bool interactive = true)
        {
            try
            {
                var returnString = "";
                if (!string.IsNullOrEmpty(searchString))
                {
                    var solutionKey = ServiceLocator.Resolve<SolutionKey>();
                    if (!String.IsNullOrWhiteSpace(solutionKey.IndexPath))
                    {
                        _currentSearcher = GetSearcher();
                        bool searchStringContainedInvalidCharacters;
                        IQueryable<CodeSearchResult> results =
                            _currentSearcher.Search(
                                GetCriteria(searchString, out searchStringContainedInvalidCharacters, searchCriteria),
                                GetSolutionName()).AsQueryable();
                        IResultsReorderer resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
                        results = resultsReorderer.ReorderSearchResults(results);
                        _myDaddy.Update(results);
                        if (searchStringContainedInvalidCharacters)
                        {
                            _myDaddy.UpdateMessage("Invalid Query String - only complete words or partial words followed by a '*' are accepted as input.");
                            return null;
                        }
                        if (SolutionMonitorFactory.PerformingInitialIndexing())
                        {
                            returnString += "Sando is still performing its initial index of this project, results may be incomplete.";
                        }
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
                        return null;
                    }
                    _myDaddy.UpdateMessage("Sando searches only the currently open Solution.  Please open a Solution and try again.");
                    return null;
                }
            }
            catch (Exception e)
            {
                _myDaddy.UpdateMessage("Sando is experiencing difficulties. See log file for details.");
                FileLogger.DefaultLogger.Error(e.Message, e);
            }
            return null;
        }

        private static string GetSolutionName()
        {
            try
            {
                var solutionKey = ServiceLocator.Resolve<SolutionKey>();
                return Path.GetFileNameWithoutExtension(solutionKey.SolutionPath);
            }
            catch
            {
                return String.Empty;
            }
        }

        public string SearchOnReturn(object sender, KeyEventArgs e, String searchString, SimpleSearchCriteria searchCriteria)
        {
            if (e.Key == Key.Return)
            {
                return Search(searchString, searchCriteria);
            }
            return "";
        }

        public void MarkInvalid()
        {
            _invalidated = true;
        }

        private SearchCriteria GetCriteria(string searchString, out bool searchStringContainedInvalidCharacters, SimpleSearchCriteria searchCriteria = null)
        {            
            var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();                       
            searchString = ExtensionPointsRepository.Instance.GetQueryRewriterImplementation().RewriteQuery(searchString);
            searchStringContainedInvalidCharacters = WordSplitter.InvalidCharactersFound(searchString);
            return CriteriaBuilder.GetBuilder().
                AddCriteria(searchCriteria).
                AddSearchString(searchString).
                NumResults(sandoOptions.NumberOfSearchResultsReturned).
                Ext(searchString).GetCriteria();                        
        }
    }

}
