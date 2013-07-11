using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
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
            foreach (string qury in quries)
            {
                var hyperlink = new SandoQueryHyperLink(new Run(qury), qury);
                hyperlink.Click += HyperlinkOnClick;
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

        private void HyperlinkOnClick(object sender, RoutedEventArgs routedEventArgs)
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
            if (!TagCloudPopUpWindow.IsOpen)
            {
                TagCloudPopUpWindow.IsOpen = true;
            }
        }

        private void CreateTagCloud()
        {
            if (TagCloudTextBlock.Inlines.Count < 5){
                var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
                var builder = new TagCloudBuilder(dictionary);
                var hyperlinks = builder.Build().Select(CreateHyperLinkByShapedWord);
                foreach (var link in hyperlinks)
                {
                    TagCloudTextBlock.Inlines.Add(link);
                    TagCloudTextBlock.Inlines.Add(" ");
                }
            }
        }

        private Hyperlink CreateHyperLinkByShapedWord(IShapedWord shapedWord)
        {
            var link = new SandoQueryHyperLink(new Run(shapedWord.Word), 
                shapedWord.Word)
            {
                FontSize = shapedWord.FontSize,
                Foreground = Brushes.Navy,
                IsEnabled = true,
            };
            link.Click += HyperlinkOnClick;
            link.Click += (sender, args) => TagCloudPopUpWindow.IsOpen = false;
            return link;
        }
    }
}
