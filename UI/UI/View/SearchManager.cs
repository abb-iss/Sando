using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Sando.Core;
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
        private CodeSearcher GetSearcher(string packageDirectory, SolutionKey solutionKey)
        {
            CodeSearcher codeSearcher = _currentSearcher;
            if (codeSearcher == null || !packageDirectory.Equals(_currentDirectory) || _invalidated)
            {
                _invalidated = false;
                _currentDirectory = packageDirectory;
                codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(solutionKey));
            }
            return codeSearcher;
        }

    	public void Search(string searchString, string packageDirectory, SolutionKey solutionKey,
    	                   SimpleSearchCriteria searchCriteria = null, bool interactive = true)
    	{
    		try
    		{
    			var returnString = "";
    			if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(packageDirectory))
    			{
    				_currentSearcher = GetSearcher(packageDirectory, solutionKey);
    				IQueryable<CodeSearchResult> results = null;
    				if (ExtensionPointsRepository.IsInterleavingExperimentOn)
    				{
    					IQueryable<CodeSearchResult> resultsA = null;
    					IQueryable<CodeSearchResult> resultsB = null;
    					var taskA = new Task(() =>
    					                     	{
    					                     		ExtensionPointsRepository.ExpFlow.Value = ExperimentFlow.A;
    					                     		resultsA = PerformSearch(searchString, searchCriteria);
    					                     	});
    					var taskB = new Task(() =>
    					                     	{
    					                     		ExtensionPointsRepository.ExpFlow.Value = ExperimentFlow.B;
    					                     		resultsB = PerformSearch(searchString, searchCriteria);
    					                     	});
    					taskA.Start();
						taskA.Wait();
						taskB.Start();
    					taskB.Wait();
    					results = InterleavingExperimentManager.Instance.InterleaveResults(resultsA, resultsB);
    				}
    				else
    				{
    					results = PerformSearch(searchString, searchCriteria);
    				}
					_myDaddy.Update(results);

					//**** determine message 
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
    		catch (Exception e)
    		{
    			_myDaddy.UpdateMessage(
    				"Invalid Query String - only complete words or partial words followed by a '*' are accepted as input.");
    		}
    	}

        private IQueryable<CodeSearchResult> PerformSearch(String searchString, SimpleSearchCriteria searchCriteria)
        {
            bool searchStringContainedInvalidCharacters = false;
            IQueryable<CodeSearchResult> results =
                _currentSearcher.Search(GetCriteria(searchString, out searchStringContainedInvalidCharacters, searchCriteria)).AsQueryable();
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
                Search(searchString, UIPackage.GetInstance().GetCurrentDirectory(), UIPackage.GetInstance().GetCurrentSolutionKey(), searchCriteria);
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
			if(UIPackage.GetInstance() != null)
			{
				criteria.NumberOfSearchResultsReturned =
					UIPackage.GetSandoOptions(UIPackage.GetInstance()).NumberOfSearchResultsReturned;
			}
        	searchString = ExtensionPointsRepository.GetInstance().GetQueryRewriterImplementation().RewriteQuery(searchString);
            searchStringContainedInvalidCharacters = WordSplitter.InvalidCharactersFound(searchString);
            List<string> searchTerms = WordSplitter.ExtractSearchTerms(searchString);
            criteria.SearchTerms = new SortedSet<string>(searchTerms);
            return criteria;
        }
        #endregion
    }

}
