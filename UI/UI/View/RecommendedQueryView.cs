using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using Sando.Core.Logging.Events;
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
                LogEvents.SelectRecommendedQuery((sender as SandoQueryHyperLink).Query);
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
            var text = searchBox.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                CreateTagCloud(new string[]{});
            }
            else
            {
                TagCloudNavigationSession.CreateNewSession(text);
                CreateTagCloud(new []{TagCloudNavigationSession.
                    CurrentSession().GetNextTerm()});
            }
        }

        private void ClearSearchHistory(object sender, RoutedEventArgs e)
        {
            var history = ServiceLocator.Resolve<SearchHistory>();
            history.ClearHistory();
        }

        private void ChangeSelectedTag(object sender, RoutedEventArgs e)
        {
            var term = sender == previousTagButton
                ? TagCloudNavigationSession.CurrentSession().GetPreviousTerm()
                    : TagCloudNavigationSession.CurrentSession().GetNextTerm();
            CreateTagCloud(new []{term});
        }
 
        private class TagCloudNavigationSession
        {
            private static TagCloudNavigationSession session;
            private readonly string[] terms;
            private readonly object locker = new object();
            private int index;


            public static TagCloudNavigationSession CurrentSession()
            {
                return session;
            }

            public static void CreateNewSession(String query)
            {
                session = new TagCloudNavigationSession(query);
            }

            private TagCloudNavigationSession(String query)
            {
                terms = query.Split().Where(s => !string.IsNullOrWhiteSpace(s)).
                    Distinct().ToArray();
                index = terms.Count() * 10;
            }

            public string GetNextTerm()
            {
                lock (locker)
                {
                    var term = terms[MakeModulePositive()];
                    index++;
                    return term;
                }
            }

            public string GetPreviousTerm()
            {
                lock (locker)
                {
                    var term = terms[MakeModulePositive()];
                    index--;
                    return term;
                }
            }

            private int MakeModulePositive()
            {
                var result = index;
                for (; result < 0; result += terms.Count());
                for (; result >= terms.Count(); result -= terms.Count());
                return result;
            }
        }

        private void CreateTagCloud(String[] words)
        {
            var dictionary = ServiceLocator.Resolve<DictionaryBasedSplitter>();
            var builder = new TagCloudBuilder(dictionary, words);
            var hyperlinks = builder.Build().Select(CreateHyperLinkByShapedWord);

            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                UpdateTagCloudWindow(words, hyperlinks);
            }
            else
            {
                Dispatcher.Invoke((Action) (() => UpdateTagCloudWindow(words
                    , hyperlinks)));
            }
        }


        private void UpdateTagCloudWindow(string[] title, IEnumerable<Hyperlink> hyperlinks)
        {
            string currentQuery;
            if (!title.Any())
            {
                tagCloudTitle.Content = "Overall";
                previousTagButton.Visibility = Visibility.Hidden;
                nextTagButton.Visibility = Visibility.Hidden;
                currentQuery = string.Empty;
            }
            else
            {
                currentQuery = title.Aggregate((w1, w2) => w1 + " " + w2);
                tagCloudTitle.Content = currentQuery;
                previousTagButton.Visibility = Visibility.Visible;
                nextTagButton.Visibility = Visibility.Visible;
            }
            TagCloudPopUpWindow.IsOpen = false;
            TagCloudTextBlock.Inlines.Clear();
            foreach (var link in hyperlinks)
            {
                TagCloudTextBlock.Inlines.Add(link);
                TagCloudTextBlock.Inlines.Add(" ");
            }
            TagCloudPopUpWindow.IsOpen = true;
            LogEvents.TagCloudShowing(currentQuery);
        }

        private Hyperlink CreateHyperLinkByShapedWord(IShapedWord shapedWord)
        {
            var link = new SandoQueryHyperLink(new Run(shapedWord.Word), 
                searchBox.Text + " " + shapedWord.Word)
            {
                FontSize = shapedWord.FontSize,
                Foreground = shapedWord.Color,
                IsEnabled = true,
            };
            link.Click += (sender, args) => LogEvents.AddWordFromTagCloud(searchBox.Text,
                (String)tagCloudTitle.Content, shapedWord.Word);
            link.Click += StartSearchAfterClick;
            link.Click += (sender, args) => TagCloudPopUpWindow.IsOpen = false;
            return link;
        }
    }
}