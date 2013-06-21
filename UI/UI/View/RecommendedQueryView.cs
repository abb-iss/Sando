using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using Sando.Core.QueryRefomers;

namespace Sando.UI.View
{
    public partial class SearchViewControl
    {
        public void UpdateRecommendedQueries(IQueryable<String> queries)
        {
            queries = ControlRecommendedQueriesCount(queries);
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

        private static IQueryable<string> ControlRecommendedQueriesCount(IQueryable<string> queries)
        {
            if (queries.Count() > QuerySuggestionConfigurations.
                MAXIMUM_RECOMMENDED_QUERIES_IN_USER_INTERFACE)
            {
                queries = queries.ToList().GetRange(0, QuerySuggestionConfigurations
                    .MAXIMUM_RECOMMENDED_QUERIES_IN_USER_INTERFACE).AsQueryable();
            }
            return queries;
        }

        private void InternalUpdateRecommendedQueries(IEnumerable<string> quries)
        {
            RecommendedQueryTextBlock.Inlines.Clear();
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

    }
}
