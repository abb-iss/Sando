using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Sando.Core.QueryRefomers;
using Sando.Core.Tools;
using Sando.DependencyInjection;

namespace Sando.UI.View
{
    public partial class SearchViewControl
    {
        public void UpdateRecommendedQueries(IQueryable<String> queries)
        {
            queries = SortRecommendedQueriesInUI(ControlRecommendedQueriesCount(queries));
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                InternalUpdateRecommendedQueries(queries);
            }
            else
            {
                Dispatcher.Invoke((Action)(() =>
                    InternalUpdateRecommendedQueries(queries)));
            }
        }

        private static IEnumerable<string> ControlRecommendedQueriesCount(IEnumerable<string> queries)
        {
            return queries.TrimIfOverlyLong(QuerySuggestionConfigurations.
                MAXIMUM_RECOMMENDED_QUERIES_IN_USER_INTERFACE).AsQueryable();
        }

        private IQueryable<string> SortRecommendedQueriesInUI(IEnumerable<string> queries)
        {
            return queries.OrderBy(q => q.Split().Count()).AsQueryable();
        }


        private void InternalUpdateRecommendedQueries(IEnumerable<string> quries)
        {
            quries = quries.ToList();
            RecommendedQueryTextBlock.Inlines.Clear();
            RecommendedQueryTextBlock.Inlines.Add(quries.Any() ? "Search instead for: " : "");
            foreach (string query in quries)
            {
                var hyperlink = new SandoQueryHyperLink(new Run(query), query);
                hyperlink.Click += RecommendedQueryOnClick;
                RecommendedQueryTextBlock.Inlines.Add(hyperlink);
                RecommendedQueryTextBlock.Inlines.Add("  ");
            }
        }

        private class SandoQueryHyperLink : Hyperlink
        {
            public String Query { private set; get; }

            internal SandoQueryHyperLink(Run run, String query)
                : base(run)
            {
                this.Query = query;
            }
        }

        private void RecommendedQueryOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender as SandoQueryHyperLink != null)
            {
                StartSearchAfterClick(sender, routedEventArgs);
            }
        }

        private void TagCloudTagOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender as SandoQueryHyperLink != null)
            {
                var reformedQuery = (sender as SandoQueryHyperLink).Query;
                StartSearchAfterClick(sender, routedEventArgs);
                CreateTagCloud(reformedQuery);
            }
        }


        private void StartSearchAfterClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender as SandoQueryHyperLink != null)
            {
                var reformedQuery = (sender as SandoQueryHyperLink).Query;
                searchBox.Text = reformedQuery;
                BeginSearch(reformedQuery); 
            }
        }



        private void AddSearchHistory(String query)
        {
            var history = ServiceLocator.Resolve<SearchHistory>();
            history.IssuedSearchString(query);
        }

        private void TagCloudPopUp(object sender, RoutedEventArgs e)
        {
            CreateTagCloud();       
        }

        private void CreateTagCloud(String word = null)
        {
            var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
            var builder = new TagCloudBuilder(dictionary, word);
            var hyperlinks = builder.Build().Select(CreateHyperLinkByShapedWord);

            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                UpdateTagCloudWindow(hyperlinks);
            }
            else
            {
                Dispatcher.Invoke((Action) (() => UpdateTagCloudWindow(hyperlinks)));
            }
        }

    
        private void UpdateTagCloudWindow(IEnumerable<Hyperlink> hyperlinks)
        {
            TagCloudPopUpWindow.IsOpen = false;
            TagCloudTextBlock.Inlines.Clear();
            foreach (var link in hyperlinks)
            {
                TagCloudTextBlock.Inlines.Add(link);
                TagCloudTextBlock.Inlines.Add(" ");
            }
            TagCloudPopUpWindow.IsOpen = true;
        }

        private Hyperlink CreateHyperLinkByShapedWord(IShapedWord shapedWord)
        {
            var link = new SandoQueryHyperLink(new Run(shapedWord.Word), 
                shapedWord.Word)
            {
                FontSize = shapedWord.FontSize,
                Foreground = shapedWord.Color,
                IsEnabled = true,
            };
            link.Click += TagCloudTagOnClick;
           // link.Click += (sender, args) => TagCloudPopUpWindow.IsOpen = false;
            return link;
        }
    }
}