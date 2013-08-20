using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Configuration.OptionsPages;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SearchContracts;
using Sando.ExtensionContracts.SplitterContracts;
using Sando.Indexer.Searching;
using Sando.Recommender;
using Sando.SearchEngine;
using Sando.Indexer.Searching.Criteria;
using Sando.Core.Tools;
using Sando.Core.Logging;
using Sando.UI.Monitoring;
using Sando.Indexer;
using Sando.Core.Logging.Events;
using Sando.Indexer.Searching.Metrics;
using Lucene.Net.Analysis;
using Sando.Indexer.Metrics;

namespace Sando.UI.View
{
    
    public class SearchManagerFactory
    {
        private static SearchManager _uiSearchManager;

        public static SearchManager GetUserInterfaceSearchManager()
        {
            return _uiSearchManager ?? (_uiSearchManager = new SearchManager());
        }

        public static SearchManager GetNewBackgroundSearchManager()
        {
            return new SearchManager();
        }
    }


    public class SearchManager
    {
        private readonly MultipleListeners _searchResultListener;

        internal SearchManager()
        {
            _searchResultListener = new MultipleListeners();
        }


        private class MultipleListeners : ISearchResultListener
        {
            private readonly List<ISearchResultListener> listeners = new List<ISearchResultListener>();
 
            internal void AddListener(ISearchResultListener listener)
            {
                this.listeners.Add(listener);
            }
           public void Update(String searchString, IQueryable<CodeSearchResult> results)
           {
               foreach (var listener in listeners)
               {
                   listener.Update(searchString, results);
               }
           }

           public void UpdateMessage(string message)
           {
               foreach (var listener in listeners)
               {
                   listener.UpdateMessage(message);
               }
           }

           public void UpdateRecommendedQueries(IQueryable<string> queries)
           {
               if (queries != null)
               {
                   foreach (var listener in listeners)
                   {
                       listener.UpdateRecommendedQueries(queries);
                   }
               }
           }
        }


        public void Search(String searchString, SimpleSearchCriteria searchCriteria = null, bool interactive = true)
        {            
            if (!EnsureSolutionOpen())
                return;
                        
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

				PreRetrievalMetrics preMetrics = new PreRetrievalMetrics(ServiceLocator.Resolve<DocumentIndexer>().Reader, ServiceLocator.Resolve<Analyzer>());
				LogEvents.PreSearch(this, preMetrics.MaxIdf(searchString), preMetrics.AvgIdf(searchString), preMetrics.AvgSqc(searchString), preMetrics.AvgVar(searchString));
                LogEvents.PreSearchQueryAnalysis(this, QueryMetrics.ExamineQuery(searchString).ToString(), QueryMetrics.DiceCoefficient(QueryMetrics.SavedQuery, searchString));
                QueryMetrics.SavedQuery = searchString;

				var criteria = GetCriteria(searchString, searchCriteria);
                var results = codeSearcher.Search(criteria, true).AsQueryable();
                var resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
                results = resultsReorderer.ReorderSearchResults(results);

                var returnString = new StringBuilder();

                if (criteria.IsQueryReformed())
                {
                    returnString.Append(criteria.GetQueryReformExplanation());
                }

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
                _searchResultListener.Update(searchString, results);
                _searchResultListener.UpdateMessage(returnString.ToString());
                _searchResultListener.UpdateRecommendedQueries(criteria.GetRecommendedQueries());
				
				LogEvents.PostSearch(this, results.Count(), criteria.NumberOfSearchResultsReturned, PostRetrievalMetrics.AvgScore(results.ToList()), PostRetrievalMetrics.StdDevScore(results.ToList()));
            }
            catch (Exception e)
            {
                _searchResultListener.UpdateMessage("Sando is experiencing difficulties. See log file for details.");
                LogEvents.UISandoSearchingError(this, e);
            }
        }

        private bool EnsureSolutionOpen()
        {
            DocumentIndexer indexer = null;
            var isOpen = true;
            try
            {
                indexer = ServiceLocator.Resolve<DocumentIndexer>();
                if (indexer == null || indexer.IsDisposingOrDisposed())
                {
                    _searchResultListener.UpdateMessage("Sando searches only the currently open Solution.  Please open a Solution and try again.");
                    isOpen = false;
                }
            }
            catch (Exception e)
            {
                _searchResultListener.UpdateMessage("Sando searches only the currently open Solution.  Please open a Solution and try again.");
                if (indexer != null)
                    LogEvents.UISolutionOpeningError(this, e);
                isOpen = false;
            }
            return isOpen;
        }

        private SearchCriteria GetCriteria(string searchString, SimpleSearchCriteria searchCriteria = null)
        {            
            var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();
            var description = new SandoQueryParser().Parse(searchString);            
            var builder = CriteriaBuilder.GetBuilder().
                AddCriteria(searchCriteria).                
                NumResults(sandoOptions.NumberOfSearchResultsReturned).AddFromDescription(description);
            var simple = builder.GetCriteria() as SimpleSearchCriteria;
            // SearchCriteriaReformer.ReformSearchCriteria(simple);
            return simple;
        }

        public void AddListener(ISearchResultListener listener)
        {
            this._searchResultListener.AddListener(listener);
        }
    }
}
