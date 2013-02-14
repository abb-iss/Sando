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
using Sando.Core.Extensions.Logging;
using Sando.UI.Monitoring;

namespace Sando.UI.View
{
    public class SearchManager
    {
        private readonly ISearchResultListener _searchResultListener;

        public SearchManager(ISearchResultListener searchResultListener)
        {
            _searchResultListener = searchResultListener;
        }

        public string Search(String searchString, SimpleSearchCriteria searchCriteria = null, bool interactive = true)
        {
            try
            {
                var codeSearcher = new CodeSearcher(new IndexerSearcher());
                var returnString = "";
                if (!string.IsNullOrEmpty(searchString))
                {
                    var solutionKey = ServiceLocator.Resolve<SolutionKey>();
                    if (!String.IsNullOrWhiteSpace(solutionKey.IndexPath))
                    {
                        bool searchStringContainedInvalidCharacters;
                        IQueryable<CodeSearchResult> results =
                            codeSearcher.Search(
                                GetCriteria(searchString, out searchStringContainedInvalidCharacters, searchCriteria),
                                GetSolutionName()).AsQueryable();
                        IResultsReorderer resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
                        results = resultsReorderer.ReorderSearchResults(results);
                        _searchResultListener.Update(results);
                        if (searchStringContainedInvalidCharacters)
                        {
                            _searchResultListener.UpdateMessage("Invalid Query String - only complete words or partial words followed by a '*' are accepted as input.");
                            return null;
                        }
                        if (ServiceLocator.Resolve<InitialIndexingWatcher>().IsInitialIndexingInProgress())
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
                        _searchResultListener.UpdateMessage(returnString);
                        return null;
                    }
                    _searchResultListener.UpdateMessage("Sando searches only the currently open Solution.  Please open a Solution and try again.");
                    return null;
                }
            }
            catch (Exception e)
            {
                _searchResultListener.UpdateMessage("Sando is experiencing difficulties. See log file for details.");
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
