using System;
using System.IO;
using System.Linq;
using System.Text;
using Configuration.OptionsPages;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.DependencyInjection;
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

        public void Search(String searchString, SimpleSearchCriteria searchCriteria = null, bool interactive = true)
        {
            try
            {
                var codeSearcher = new CodeSearcher(new IndexerSearcher());
                if (String.IsNullOrEmpty(searchString))
                    return;
                
                var solutionKey = ServiceLocator.ResolveOptional<SolutionKey>(); //no opened solution
                if (solutionKey == null)
                {
                    _searchResultListener.UpdateMessage("Sando searches only the currently open Solution.  Please open a Solution and try again.");
                    return;
                }

                searchString = ExtensionPointsRepository.Instance.GetQueryRewriterImplementation().RewriteQuery(searchString);
                var searchStringContainedInvalidCharacters = WordSplitter.InvalidCharactersFound(searchString);
                if (searchStringContainedInvalidCharacters)
                {
                    _searchResultListener.UpdateMessage("Invalid Query String - only complete words or partial words followed by a '*' are accepted as input.");
                    return;
                }

                var criteria = GetCriteria(searchString, searchCriteria);
                var solutionName = GetSolutionName();
                var results = codeSearcher.Search(criteria, solutionName).AsQueryable();
                var resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
                results = resultsReorderer.ReorderSearchResults(results);

                var returnString = new StringBuilder();
                if (!results.Any())
                {
                    returnString.Append("No results found. ");
                }
                else
                {
                    returnString.Append(results.Count() + " results returned. ");
                }
                if (ServiceLocator.Resolve<InitialIndexingWatcher>().IsInitialIndexingInProgress())
                {
                    returnString.Append("Sando is still performing its initial index of this project, results may be incomplete.");
                }
                _searchResultListener.Update(results);
                _searchResultListener.UpdateMessage(returnString.ToString());
            }
            catch (Exception e)
            {
                _searchResultListener.UpdateMessage("Sando is experiencing difficulties. See log file for details.");
                FileLogger.DefaultLogger.Error(e.Message, e);
            }
        }

        private static string GetSolutionName()
        {
            var solutionKey = ServiceLocator.Resolve<SolutionKey>();
            return Path.GetFileNameWithoutExtension(solutionKey.SolutionPath);
        }

        private static SearchCriteria GetCriteria(string searchString, SimpleSearchCriteria searchCriteria = null)
        {            
            var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();                       
            return CriteriaBuilder.GetBuilder().
                AddCriteria(searchCriteria).
                AddSearchString(searchString).
                NumResults(sandoOptions.NumberOfSearchResultsReturned).
                Ext(searchString).GetCriteria();                        
        }
    }

}
