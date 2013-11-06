using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SearchContracts;
using Sando.Indexer.Searching.Criteria;
using Sando.Translation;
using Sando.Recommender;
using FocusTestVC;
using Sando.UI.View.Search;
using Sando.UI.Actions;
using Sando.Core.Logging.Events;
using Sando.Indexer.Searching.Metrics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Sando.Core.Tools;
using Sando.DependencyInjection;
using ABB.SrcML.VisualStudio.SrcMLService;
using System.IO;
using Microsoft.Practices.Unity;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Windows.Media;
using ABB.SrcML;

namespace Sando.UI.View
{
    public partial class SearchViewControl : ISearchResultListener
    {
        public SearchViewControl()
        {
            DataContext = this; //so we can show results
            InitializeComponent();

            _searchManager = SearchManagerFactory.GetUserInterfaceSearchManager();
            _searchManager.AddListener(this);
            SearchResults = new ObservableCollection<CodeSearchResult>();
            MonitoredFiles = new ObservableCollection<CheckedListItem>();
            //SearchCriteria = new SimpleSearchCriteria();
            InitAccessLevels();
            InitProgramElements();

            ((INotifyCollectionChanged)searchResultListbox.Items).CollectionChanged += SelectFirstResult;
            ((INotifyCollectionChanged)searchResultListbox.Items).CollectionChanged += ScrollToTop;

            SearchStatus = "Enter search terms.";

            _recommender = new QueryRecommender();            
            ServiceLocator.RegisterInstance<QueryRecommender>(_recommender);
            ServiceLocator.RegisterInstance<SearchViewControl>(this);
        }

     


        public ObservableCollection<AccessWrapper> AccessLevels
        {
            get { return (ObservableCollection<AccessWrapper>) GetValue(AccessLevelsProperty); }
            set { SetValue(AccessLevelsProperty, value); }
        }


        public ObservableCollection<ProgramElementWrapper> ProgramElements
        {
            get { return (ObservableCollection<ProgramElementWrapper>) GetValue(ProgramElementsProperty); }
            set { SetValue(ProgramElementsProperty, value); }
        }

        public ObservableCollection<CodeSearchResult> SearchResults
        {
            get { return (ObservableCollection<CodeSearchResult>) GetValue(SearchResultsProperty); }
            set { SetValue(SearchResultsProperty, value); }
        }

        public ObservableCollection<CheckedListItem> MonitoredFiles
        {
            get { return (ObservableCollection<CheckedListItem>)GetValue(MonitoredFilesProperty); }
            set { SetValue(MonitoredFilesProperty, value); }
        }

        public string SearchStatus
        {
            get { return (string) GetValue(SearchStatusProperty); }
            private set { SetValue(SearchStatusProperty, value); }
        }

        public string OpenSolutionPaths
        {
            get { return (string)GetValue(OpenSolutionPathsProperty); }
            set { SetValue(OpenSolutionPathsProperty, value); }
        }

 
        public string SearchLabel
        {
            get { return Translator.GetTranslation(TranslationCode.SearchLabel); }
        }

        public string ExpandCollapseResultsLabel
        {
            get { return Translator.GetTranslation(TranslationCode.ExpandResultsLabel); }
        }

        public string ComboBoxItemCurrentDocument
        {
            get { return Translator.GetTranslation(TranslationCode.ComboBoxItemCurrentDocument); }
        }

        public string ComboBoxItemEntireSolution
        {
            get { return Translator.GetTranslation(TranslationCode.ComboBoxItemEntireSolution); }
        }

        private void searchBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (searchBox != null)
            {
                var textBox = searchBox.Template.FindName("Text", searchBox) as TextBox;
                if (textBox != null)
                {
                    TextBoxFocusHelper.RegisterFocus(textBox);
                    textBox.KeyDown += HandleTextBoxKeyDown;
                }

                var listBox = searchBox.Template.FindName("Selector", searchBox) as ListBox;
                if (listBox != null)
                {
                    listBox.SelectionChanged += searchBoxListBox_SelectionChanged;
                }
            }
        }

        private void HandleTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                searchResultListbox.Focus();
                e.Handled = true;
            }
        }

        private void InitProgramElements()
        {
            ProgramElements = new ObservableCollection<ProgramElementWrapper>
                {
                    new ProgramElementWrapper(true, ProgramElementType.Class),
                    new ProgramElementWrapper(false, ProgramElementType.Comment),
                    new ProgramElementWrapper(true, ProgramElementType.Custom),
                    new ProgramElementWrapper(true, ProgramElementType.Enum),
                    new ProgramElementWrapper(true, ProgramElementType.Field),
                    new ProgramElementWrapper(true, ProgramElementType.Method),
                    new ProgramElementWrapper(true, ProgramElementType.MethodPrototype),
                    new ProgramElementWrapper(true, ProgramElementType.Property),
                    new ProgramElementWrapper(true, ProgramElementType.Struct),
                    new ProgramElementWrapper(true, ProgramElementType.XmlElement)
                    //new ProgramElementWrapper(true, ProgramElementType.TextLine)
                };
        }

        private void InitAccessLevels()
        {
            AccessLevels = new ObservableCollection<AccessWrapper>
                {
                    new AccessWrapper(true, AccessLevel.Private),
                    new AccessWrapper(true, AccessLevel.Protected),
                    new AccessWrapper(true, AccessLevel.Internal),
                    new AccessWrapper(true, AccessLevel.Public)
                };
        }

        private void SelectFirstResult(object sender, NotifyCollectionChangedEventArgs e)
        {
            //searchResultListbox.SelectedIndex = 0;
            //searchResultListbox_SelectionChanged(searchResultListbox,null);
            searchResultListbox.SelectedIndex = -1;
            searchResultListbox.Focus();
        }

        private void ScrollToTop(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(searchResultListbox.Items.Count > 0)
                searchResultListbox.ScrollIntoView(searchResultListbox.Items[0]);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {

            BeginSearch(searchBox.Text);
        }

        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var text = sender as AutoCompleteBox;
                if (text != null)
                {
                    BeginSearch(text.Text);
                }
            }
        }

        private void BeginSearch(string searchString)
        {
            RemoveOldResults();

            AddSearchHistory(searchString);

            SimpleSearchCriteria Criteria = new SimpleSearchCriteria();

            //Store the search key
            this.searchKey = searchBox.Text;
            
            //Clear the old recommendation.
            UpdateRecommendedQueries(Enumerable.Empty<String>().AsQueryable());

            var selectedAccessLevels = AccessLevels.Where(a => a.Checked).Select(a => a.Access).ToList();
            if (selectedAccessLevels.Any())
            {
                Criteria.SearchByAccessLevel = true;
                Criteria.AccessLevels = new SortedSet<AccessLevel>(selectedAccessLevels);
            }
            else
            {
                Criteria.SearchByAccessLevel = false;
                Criteria.AccessLevels.Clear();
            }

            var selectedProgramElementTypes =
                ProgramElements.Where(e => e.Checked).Select(e => e.ProgramElement).ToList();
            if (selectedProgramElementTypes.Any())
            {
                Criteria.SearchByProgramElementType = true;
                Criteria.ProgramElementTypes = new SortedSet<ProgramElementType>(selectedProgramElementTypes);
            }
            else
            {
                Criteria.SearchByProgramElementType = false;
                Criteria.ProgramElementTypes.Clear();
            }

            SearchAsync(searchString, Criteria);
        }

        private void RemoveOldResults()
        {
            try
            {
                while (SearchResults.Count > 0)
                    SearchResults.RemoveAt(0);
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }

        private void SearchAsync(String text, SimpleSearchCriteria searchCriteria)
        {
            var sandoWorker = new BackgroundWorker();
            sandoWorker.DoWork += sandoWorker_DoWork;
            var workerSearchParams = new WorkerSearchParameters { Query = text, Criteria = searchCriteria };
            sandoWorker.RunWorkerAsync(workerSearchParams);
        }

        private class WorkerSearchParameters
        {
            public SimpleSearchCriteria Criteria { get; set; }
            public String Query { get; set; }
        }
      
        void sandoWorker_DoWork(object sender, DoWorkEventArgs e)
        {            
            var searchParams = (WorkerSearchParameters) e.Argument;
            _searchManager.Search(searchParams.Query, searchParams.Criteria);
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileWithSelectedResult(sender);
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                OpenFileWithSelectedResult(sender);
            }
        }

        private delegate void HighlightStuffDelegate(string path,int linenumber,string source,string[] searchkeys);
        private static HighlightStuffDelegate HighlightStuffInvoker = new HighlightStuffDelegate(HighlightStuff);

        private static void HighlightStuff(string path, int linenumber, string source, string[] searchkeys)
        {
            HighlightedEntitySet.GetInstance().Clear();
            HighlightedEntitySet.GetInstance().AddEntity(path, linenumber, source, searchkeys);
        }

        private void OpenFileWithSelectedResult(object sender)
        {
            try
            {
                var result = sender as ListBoxItem;
                if (result != null)
                {
                    string[] searchKeys = GetKeys(this.searchKey);
                    var searchResult = result.Content as CodeSearchResult;
                    FileOpener.OpenItem(searchResult);
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(500);
                        this.Dispatcher.BeginInvoke(HighlightStuffInvoker, searchResult.ProgramElement.FullFilePath, searchResult.
                            ProgramElement.DefinitionLineNumber, searchResult.ProgramElement.RawSource, searchKeys);
                    });

                    var matchDescription = QueryMetrics.DescribeQueryProgramElementMatch(searchResult.ProgramElement, searchBox.Text);
                    LogEvents.OpeningCodeSearchResult(searchResult, SearchResults.IndexOf(searchResult) + 1, matchDescription);
                }
            }
            catch (ArgumentException aex)
            {
                LogEvents.UIGenericError(this, aex);
                MessageBox.Show(FileNotFoundPopupMessage, FileNotFoundPopupTitle, MessageBoxButton.OK);
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
                MessageBox.Show(FileNotFoundPopupMessage, FileNotFoundPopupTitle, MessageBoxButton.OK);
            }
        }
               
        public void Update(string searchString, IQueryable<CodeSearchResult> results)
        {         
            //prepare results for highlighting in the background, in parallel of course :)
            object[] parameter = { results };
            var exceptions = new ConcurrentQueue<Exception>();
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(results, item =>
                {
                    try
                    {
                        string highlight;
                        string highlightRaw;
                        item.HighlightOffsets = GenerateHighlight(item.Raw, this.searchKey,
                            out highlight, out highlightRaw);
                        item.Highlight = highlight;
                        item.HighlightRaw = highlightRaw;
                    }
                    catch (Exception exc)
                    {
                        exceptions.Enqueue(exc);
                    }
                }
               );
            }).
            //then update the UI in the UI thread
            ContinueWith(updateUi => this.Dispatcher.BeginInvoke(new UiUpdateDelagate(UpdateUiResults), parameter));

        }

        public delegate void UiUpdateDelagate(IEnumerable<CodeSearchResult> results);

        private void UpdateUiResults(IEnumerable<CodeSearchResult> results)
        {
            try
            {                
                foreach (var codeSearchResult in results)
                {
                    SearchResults.Add(codeSearchResult);
                }
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }

        public int[] GenerateHighlight(string raw, string searchKey, out string highlight_out, 
            out string highlightRaw_out) {
            try
            {
                StringBuilder highlight = new StringBuilder();
                StringBuilder highlight_Raw = new StringBuilder();

                string[] lines = raw.Split('\n');
                StringBuilder newLine = new StringBuilder();

                string[] searchKeys = GetKeys(searchKey);
                string[] containedKeys;


                var highlightOffsets = new List<int>();
                int offest = 0;
                foreach (string line in lines)
                {

                    containedKeys = GetContainedSearchKeys(searchKeys, line);

                    if (containedKeys.Length != 0)
                    {

                        string temp_line = string.Copy(line);
                        int loc;
                        //One line contain multiple words
                        foreach (string key in containedKeys)
                        {
                            newLine.Clear();
                            while ((loc = temp_line.IndexOf(key, StringComparison.InvariantCultureIgnoreCase)) >= 0)
                            {

                                string replaceKey = "|~S~|" + temp_line.Substring(loc, key.Length) + "|~E~|";
                                newLine.Append(temp_line.Substring(0, loc) + replaceKey);
                                temp_line = temp_line.Remove(0, loc + key.Length);

                            }

                            newLine.Append(temp_line);
                            temp_line = newLine.ToString();

                        }
                        highlightOffsets.Add(offest);
                        highlight.Append(newLine + Environment.NewLine);
                        highlight_Raw.Append(newLine + Environment.NewLine);
                    }
                    else
                    {
                        highlight_Raw.Append(line + Environment.NewLine);
                    }
                    offest++;
                }

                highlight_out = highlight.ToString().Replace("\t", "    ");
                highlightRaw_out = highlight_Raw.ToString().Replace("\t", "    ");
                return highlightOffsets.ToArray();
            }
            catch (Exception e)
            {
                highlightRaw_out = raw;
                var lines = raw.Split('\n');
                var keys = GetKeys(searchKey);
                var sb = new StringBuilder();
                var offesets = new List<int>();
                for (int i = 0; i < lines.Count(); i ++)
                {
                    var containedKeys = GetContainedSearchKeys(keys, lines.ElementAt(i));
                    if (containedKeys.Any())
                    {
                        sb.AppendLine(lines.ElementAt(i));
                        offesets.Add(i);
                    }
                }
                highlight_out = sb.ToString();
               return offesets.ToArray();
            }
        }

        public static string[] GetKeys(string searchKey) {
            SandoQueryParser parser = new SandoQueryParser();
            var description = parser.Parse(searchKey);
            var terms = description.SearchTerms;
            HashSet<string> keys = new HashSet<string>();
            foreach(var term in terms) {
                keys.Add(DictionaryHelper.GetStemmedQuery(term));
                keys.Add(term);
            }
            foreach (var quote in description.LiteralSearchTerms)
            {

                var toAdd = quote.Substring(1);
                toAdd = toAdd.Substring(0, toAdd.Length - 1);
                //unescape '\' and '"'s
                toAdd = toAdd.Replace("\\\"","\"");
                toAdd = toAdd.Replace("\\\\", "\\");
                keys.Add(toAdd);
            }
            return keys.ToArray();
        }

        //Return the contained search key
        private string[] GetContainedSearchKeys(string[] searchKeys, string line)
        {
            searchKeys = RemovePartialWords(searchKeys.Where(k => line.IndexOf(k, 
                StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray());       
            var containedKeys = new Dictionary<String, int>();
            foreach (string key in searchKeys){
                var index = line.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
                if (index >= 0)
                {
                    containedKeys.Add(key, index);
                }
            }
            return containedKeys.OrderBy(p => p.Value).Select(p => p.Key).ToArray();
        }

        private string[] RemovePartialWords(string[] words)
        {
            var removedIndex = new List<int>();
            var sortedWords = words.OrderByDescending(w => w.Length).ToList();
            for (int i = sortedWords.Count() - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (sortedWords[j].IndexOf(sortedWords[i], StringComparison.
                        InvariantCultureIgnoreCase) >= 0)
                    {
                        removedIndex.Add(i);
                        break;
                    }
                }
            }
            foreach (var index in removedIndex.Distinct().OrderByDescending(i => i))
            {
                sortedWords.RemoveAt(index);
            }
            return sortedWords.ToArray();
        }


        public void UpdateMessage(string message)
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                SearchStatus = message;
            }
            else
            {
                Dispatcher.Invoke((Action)(() => SearchStatus = message));
            }
        }

        private void UpdateExpansionState(ListView view)
        {
            return;

            if (view != null)
            {
                var selectedIndex = view.SelectedIndex;

                if (IsExpandAllChecked())
                {
                    for (var currentIndex = 0; currentIndex < view.Items.Count; ++currentIndex)
                    {
                        var currentItem = view.ItemContainerGenerator.ContainerFromIndex(currentIndex) as ListViewItem;
                        if (currentItem != null)
                            currentItem.Height = 89;
                    }
                }
                else
                {
                    for (var currentIndex = 0; currentIndex < view.Items.Count; ++currentIndex)
                    {
                        var currentItem = view.ItemContainerGenerator.ContainerFromIndex(currentIndex) as ListViewItem;                        
                        if (currentItem != null)
                            currentItem.Height = currentIndex == selectedIndex ? 89 : 24;
                    }
                }
            }
        } 

        private bool IsExpandAllChecked()
        {
            if (expandButton == null)
                return false;
            var check = expandButton.IsChecked;
            return check.HasValue && check == true;
        }

        private void searchBox_Populating(object sender, PopulatingEventArgs e)
        {
            var recommendationWorker = new BackgroundWorker();
            recommendationWorker.DoWork += recommendationWorker_DoWork;
            e.Cancel = true;
            recommendationWorker.RunWorkerAsync(searchBox.Text);
        }

        private void recommendationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var queryString = (string) e.Argument;

            var result = _recommender.GenerateRecommendations(queryString);
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                UpdateRecommendations(result, queryString);
            }
            else
            {
                Dispatcher.Invoke((Action) (() => UpdateRecommendations(result, queryString)));
            }
        }

        private void searchResultListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var listview = sender as ListView;
                ShowPopup(listview, e.AddedItems, true);
                ShowPopup(listview, e.RemovedItems, false);
                LogEvents.SelectingCodeSearchResult(this, listview.SelectedIndex + 1);
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }

        private void MyToolWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.searchResultListbox.Items.Count > 0)
                {
                    foreach (var item in this.searchResultListbox.Items)
                        ShowPopupOneItem(this.searchResultListbox, false, item);
                }
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }

        private static void ShowPopup( ListView listview, IList list, bool showOrRemove)
        {
            if (list.Count > 0)
            {
                var item = list[0];
                ShowPopupOneItem(listview, showOrRemove, item);
            }
        }

        private static void ShowPopupOneItem(ListView listview, bool showOrRemove, object item)
        {
            ListViewItem lvi = (ListViewItem)listview.ItemContainerGenerator.ContainerFromItem(item);
            foreach (Popup popup in FindVisualChildren<Popup>(lvi))
            {
                (popup as Popup).IsOpen = showOrRemove;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void UpdateRecommendations(IEnumerable<ISwumRecommendedQuery> recommendations, string query)
        {
            if (query == searchBox.Text)
            {
                searchBox.ItemsSource = recommendations;
                searchBox.PopulateComplete();
            }
            else
            {
                Debug.WriteLine("Query \"{0}\" doesn't match current text \"{1}\"; no update.", query, searchBox.Text);
            }
        }

        public void FocusOnText()
        {
            var textBox = searchBox.Template.FindName("Text", searchBox) as TextBox;
            if (textBox != null)
                textBox.Focus();
        }

        public void ShowProgressBar(bool visible)
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                ProgBar.Visibility = (visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            else
            {
                Dispatcher.Invoke((Action)(() => ProgBar.Visibility = (visible) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed));
            }
        }

        private void searchBoxListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var listBox = sender as ListBox;
                if (listBox != null)
                {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                    LogEvents.SelectingRecommendationItem(this, listBox.SelectedIndex + 1);
                }
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }

        private void Toggled_Popup(object sender, RoutedEventArgs e)
        {
            if(SelectionPopup!=null && !SelectionPopup.IsOpen)
                SelectionPopup.IsOpen = true;
        }


        public static readonly DependencyProperty AccessLevelsProperty =
            DependencyProperty.Register("AccessLevels", typeof (ObservableCollection<AccessWrapper>), typeof (SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ProgramElementsProperty =
            DependencyProperty.Register("ProgramElements", typeof(ObservableCollection<ProgramElementWrapper>), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty MonitoredFilesProperty =
           DependencyProperty.Register("MonitoredFiles", typeof(ObservableCollection<CheckedListItem>), typeof(SearchViewControl), new UIPropertyMetadata(null));


        public static readonly DependencyProperty SearchResultsProperty =
            DependencyProperty.Register("SearchResults", typeof(ObservableCollection<CodeSearchResult>), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SearchStringProperty =
            DependencyProperty.Register("SearchString", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SearchStatusProperty =
            DependencyProperty.Register("SearchStatus", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty OpenSolutionPathsProperty =
            DependencyProperty.Register("OpenSolutionPaths", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));        

        public static readonly DependencyProperty SearchCriteriaProperty =
            DependencyProperty.Register("SearchCriteria", typeof(SimpleSearchCriteria), typeof(SearchViewControl), new UIPropertyMetadata(null));

        private const string FileNotFoundPopupMessage = "This file cannot be opened. It may have been deleted, moved, or renamed since your last search.";
        private const string FileNotFoundPopupTitle = "File opening error";

        private readonly SearchManager _searchManager;
        private readonly QueryRecommender _recommender;
        
        //Search Key 
        private string searchKey;

        private void Remove_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = searchResultListbox.SelectedItems[0];
                if (result != null)
                {
                    FileRemover.Remove((result as CodeSearchResult).ProgramElement.FullFilePath, RemoverCompleted);
                }
            }
            catch (ArgumentException aex)
            {
                LogEvents.UIGenericError(this, aex);
            }
        }

        private void RemoverCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SearchButtonClick(null, null);
        }


        //private Color Good;//(Color)ColorConverter.ConvertFromString("#E9FFDE");
        //private Color OK; //(Color)ColorConverter.ConvertFromString("#FFFFE6");
        //private Color Bad; //(Color)ColorConverter.ConvertFromString("#FFF0F0");

        //internal Color GetGood()
        //{
        //    var key = Microsoft.VisualStudio.Shell.VsBrushes.ScrollBarBackgroundKey;
        //    return (Application.Current.Resources[key] as SolidColorBrush).Color; ;
        //}
        
        //internal Color GetOK()
        //{
        //    var key = Microsoft.VisualStudio.Shell.VsBrushes.AccentLightKey;
        //    return (Application.Current.Resources[key] as SolidColorBrush).Color; ;
        //}
        
        //internal Color GetBad()
        //{
        //    var key = Microsoft.VisualStudio.Shell.VsBrushes.AccentMediumKey;
        //    return (Application.Current.Resources[key] as SolidColorBrush).Color; ;
        //}

        //bool initedColors = false;

        //private void RespondToLoad(object sender, RoutedEventArgs e)
        //{
        //    if (!initedColors)
        //    {
        //        Good = GetGood();
        //        OK = GetOK();
        //        Bad = GetBad();
        //        initedColors = true;
        //    }
        //    try
        //    {
        //        var item = sender as Border;
        //        var gradientBrush = item.Background as System.Windows.Media.LinearGradientBrush;
        //        Color myColor = Colors.White;
        //        var result = item.DataContext as CodeSearchResult;
        //        if (result != null)
        //        {
        //            double score = result.Score;
        //            if (score >= 0.6)
        //                myColor = Good;
        //            else if (score >= 0.4)
        //                myColor = OK;
        //            else if (score < 0.4)
        //                myColor = Bad;
        //            if (score > .99)
        //            {
        //                foreach (var stop in gradientBrush.GradientStops)
        //                    stop.Color = myColor;
        //            }
        //            else
        //            {
        //                gradientBrush.GradientStops.First().Color = myColor;
        //                gradientBrush.GradientStops.ElementAt(1).Color = myColor;
        //            }
        //        }

        //    }
        //    catch (Exception problem)
        //    {
        //        //ignore for now, as this is not a crucial feature
        //    }
        //}


        private void ListViewItem_LostFocus(object sender, RoutedEventArgs e) {
            //searchResultListbox.SelectedItem = null;
            //// Mark as handled to prevent this event from bubbling up the element tree.
            //e.Handled = true;
        }

        private void MouseLeaveEvent(object sender, MouseEventArgs e) {
            try
            {
                searchResultListbox.SelectedItem = null;
                // Mark as handled to prevent this event from bubbling up the element tree.
                e.Handled = true;
            }
            catch (Exception ee)
            {
                LogEvents.UIGenericError(this, ee);
            }
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            var uiPackage = ServiceLocator.Resolve<UIPackage>();
            if (uiPackage != null)
                uiPackage.OpenSandoOptions();
        }





        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OpenFolderSelection();
        }

        private void OpenFolderSelection()
        {
            try
            {
                var srcMlService = ServiceLocator.Resolve<ISrcMLGlobalService>();
                if (srcMlService != null)
                {
                    if (srcMlService.MonitoredDirectories != null)
                    {
                        while (MonitoredFiles.Count > 0)
                            MonitoredFiles.RemoveAt(0);
                        foreach (var dir in srcMlService.MonitoredDirectories)
                            MonitoredFiles.Add(new CheckedListItem(dir));
                        CurrentlyIndexingFoldersPopup.IsOpen = true;
                    }
                }
            }
            catch (ResolutionFailedException resFailed)
            {
                //ignore
            }
        }

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFolderSelection();
        }

        public class CheckedListItem
        {

            public CheckedListItem(string path)
            {
                Name = Path.GetFileName(path);
                if (string.IsNullOrWhiteSpace(Name))
                    Name = Path.GetDirectoryName(path);
                if (string.IsNullOrWhiteSpace(Name))
                    Name = path;
                Id = path;
                IsChecked = true;
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public bool IsChecked { get; set; }
        }


        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {            
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (MonitoredFiles.Count > 0)
                dialog.SelectedPath = MonitoredFiles.First().Id;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (System.Windows.Forms.DialogResult.OK.Equals(result))
            {
                MonitoredFiles.Add(new CheckedListItem(dialog.SelectedPath));
            }
        }

        private void UpdateFolders_Click(object sender, RoutedEventArgs e)
        {
            try{
                    var srcMlService = ServiceLocator.Resolve<ISrcMLGlobalService>();
                    if (srcMlService != null)
                    {
                        foreach (var folder in MonitoredFiles)
                        {
                            if (!folder.IsChecked)
                                srcMlService.RemoveDirectoryFromMonitor(folder.Id);
                            else
                            {
                                try
                                {
                                    srcMlService.AddDirectoryToMonitor(folder.Id);
                                }
                                catch (DirectoryScanningMonitorSubDirectoryException cantAdd)
                                {
                                    MessageBox.Show("Sub-directories of existing directories cannot be added - " + cantAdd.Message, "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);                                    
                                }
                                catch (ForbiddenDirectoryException cantAdd)
                                {
                                    MessageBox.Show( cantAdd.Message, "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }
                    CloseFolderPopup();
            }
            catch (ResolutionFailedException resFailed)
            {
                //ignore
            }
            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseFolderPopup();            
        }

        private void CloseFolderPopup()
        {
            CurrentlyIndexingFoldersPopup.IsOpen = false;
            UpdateMethodIndexingList();
        }

        private void UpdateMethodIndexingList()
        {
            try
            {
                var srcMlService = ServiceLocator.Resolve<ISrcMLGlobalService>();
                if (srcMlService != null)
                {
                    if (srcMlService.MonitoredDirectories != null && srcMlService.MonitoredDirectories.Count > 0)
                        OpenSolutionPaths = UIPackage.GetDisplayPathMonitoredFiles(srcMlService, this);
                }
            }
            catch (ResolutionFailedException resFailed)
            {
                //ignore
            }
        }

        private void IndexingList_KeyDown(object sender, KeyEventArgs e)
        {
            OpenFolderSelection();
            e.Handled = true;
        }

        private void IndexingList_MouseLeave(object sender, MouseEventArgs e)
        {
            IndexingList.Background = GetToolBackgroundcolor();
        }

        private void IndexingList_MouseEnter(object sender, MouseEventArgs e)
        {
            IndexingList.Background = GetToolBackgroundHighlightColor();
        }

    

    }
}