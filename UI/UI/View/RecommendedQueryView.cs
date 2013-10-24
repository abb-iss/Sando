using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Sando.Core.Logging.Events;
using Sando.Core.QueryRefomers;
using Sando.Core.Tools;
using Sando.DependencyInjection;
using Sando.Recommender;
using Sando.UI.Actions;

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
            var toRemoveList = new List<string>();
            toRemoveList.AddRange(searchBox.Text.Split());
            int index = 0;
            foreach (string query in quries)
            {
                var hyperlink = new SandoQueryHyperLink(new Run(RemoveDuplicateTerms(query, 
                    toRemoveList)), query, index ++);                
                hyperlink.Click += RecommendedQueryOnClick;
                RecommendedQueryTextBlock.Inlines.Add(hyperlink);
                RecommendedQueryTextBlock.Inlines.Add("  ");
            }
        }

        private string RemoveDuplicateTerms(string query, List<string> toRemoveList)
        {
            var addedTerms = query.Split().Except(toRemoveList, 
                new StringEqualityComparer()).ToArray();
            toRemoveList.AddRange(addedTerms);
            return addedTerms.Any() ? addedTerms.Aggregate((t1, t2) => t1 + " " + t2).
                Trim() : string.Empty;
        }


        private class StringEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return 0;
            }
        }

        private class SandoQueryHyperLink : Hyperlink
        {
            public String Query { private set; get; }
            public int Index { private set; get; }

            internal SandoQueryHyperLink(Run run, String query, int index)
                : base(run)
            {
                this.Query = query;
                this.Foreground = GetHistoryTextColor();
                this.Index = index;
            }
        }

        private void RecommendedQueryOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender as SandoQueryHyperLink != null)
            {
                StartSearchAfterClick(sender, routedEventArgs);
                LogEvents.SelectRecommendedQuery((sender as SandoQueryHyperLink).Query, 
                    (sender as SandoQueryHyperLink).Index);
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
            System.Threading.Tasks.Task.Factory.StartNew(() =>
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
                        Dispatcher.Invoke((Action)(() => UpdateTagCloudWindow(words
                            , hyperlinks)));
                    }
            });
        }


        private void UpdateLabel(string[] highlightedTerms)
        {
            var terms = searchBox.Text.Split().Select(s => s.Trim().
                ToLower()).Distinct().Where(t => !string.IsNullOrWhiteSpace(t)).
                    ToArray();
            tagCloudTitleTextBlock.Inlines.Clear();
            if (!terms.Any())
            {
                tagCloudTitleTextBlock.Inlines.Add(new Run("Tag cloud for this project")
                    {
                      //  FontSize = 24, 
                        Foreground = Brushes.CadetBlue
                    });
            }
            else
            {
                var runs = terms.Select(t => new Run(t + " ")
                    {
                        FontSize = highlightedTerms.Contains(t) ? 28 : 24,
                        Foreground = highlightedTerms.Contains(t)
                                        ? Brushes.CadetBlue : Brushes.CadetBlue
                    }).ToArray();
                runs.Last().Text = runs.Last().Text.Trim();
                tagCloudTitleTextBlock.Inlines.AddRange(runs);
            }
        }

        private void UpdateTagCloudWindow(string[] title, IEnumerable<Hyperlink> hyperlinks)
        {
            string currentQuery;
            if (!title.Any())
            {
                UpdateLabel(new string[]{});
                previousTagButton.Visibility = Visibility.Hidden;
                nextTagButton.Visibility = Visibility.Hidden;
                currentQuery = string.Empty;
            }
            else
            {
                currentQuery = title.Aggregate((w1, w2) => w1 + " " + w2);
                UpdateLabel(title);
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
                searchBox.Text + " " + shapedWord.Word, 0)
            {
                FontSize = shapedWord.FontSize,
                Foreground = shapedWord.Color,
                IsEnabled = true,
                TextDecorations = null,
            };
            link.Click += (sender, args) => LogEvents.AddWordFromTagCloud(searchBox.Text,
                "TOFIXTHE", shapedWord.Word);
            link.Click += StartSearchAfterClick;
            link.Click += (sender, args) => TagCloudPopUpWindow.IsOpen = false;
            return link;
        }

        internal Brush GetNormalTextColor()
        {
            var key = Microsoft.VisualStudio.Shell.VsBrushes.ToolWindowTextKey;
            var brush = (Brush)Application.Current.Resources[key];
            return brush;
        }

        internal Brush GetToolBackgroundcolor()
        {
            var key = Microsoft.VisualStudio.Shell.VsBrushes.ToolWindowBackgroundKey;
            var brush = (Brush)Application.Current.Resources[key];
            return brush;
        }

        internal Brush GetToolBackgroundHighlightColor()
        {
            var key = Microsoft.VisualStudio.Shell.VsBrushes.DropDownBackgroundKey;
            var brush = (Brush)Application.Current.Resources[key];
            return brush;
        }

        internal static Brush GetHistoryTextColor()
        {
            if (FileOpener.Is2012OrLater())
            {
                var key = Microsoft.VisualStudio.Shell.VsBrushes.ToolWindowTabMouseOverTextKey;
                var color = (Brush)Application.Current.Resources[key];
                var other = (Brush)Application.Current.Resources[Microsoft.VisualStudio.Shell.VsBrushes.ToolWindowBackgroundKey];
                if (color.ToString().Equals(other.ToString()))
                {
                    return (Brush)Application.Current.Resources[Microsoft.VisualStudio.Shell.VsBrushes.HelpSearchResultLinkSelectedKey];
                }
                else
                    return color;
            }
            else
            {
                var key = Microsoft.VisualStudio.Shell.VsBrushes.HelpSearchResultLinkSelectedKey;
                return (Brush)Application.Current.Resources[key];
            }
        }

        internal Color GetHighlightColor()
        {
            var key = Microsoft.VisualStudio.Shell.VsBrushes.HighlightKey;
            var brush = (SolidColorBrush)Application.Current.Resources[key];
            return brush.Color;
        }

        internal Color GetHighlightBorderColor()
        {
            var key = Microsoft.VisualStudio.Shell.VsBrushes.HighlightTextKey;
            var brush = (SolidColorBrush)Application.Current.Resources[key];
            return brush.Color;
        }

        private void SearchBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count == 0)
                    return;
                var item = (ISwumRecommendedQuery)e.AddedItems[0];
                if (item.Type == SwumRecommnedationType.History)
                    LogEvents.SelectHistoryItem();
                else
                    LogEvents.SelectSwumRecommendation(item.Query);
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }
    }
}