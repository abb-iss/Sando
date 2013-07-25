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
using Sando.Core.Logging;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer.Searching.Criteria;
using Sando.Translation;
using Sando.Recommender;
using FocusTestVC;
using Sando.UI.View.Search;
using Sando.UI.Actions;
using System.Windows.Media;
using Sando.Core.Logging.Events;
using Sando.Indexer.Searching.Metrics;

using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Sando.Core.Tools;
using Sando.DependencyInjection;

namespace Sando.UI.View
{
    public partial class SearchViewControl : ISearchResultListener
    {
        public SearchViewControl()
        {
            DataContext = this; //so we can show results
            InitializeComponent();

            _searchManager = new SearchManager(this);
            SearchResults = new ObservableCollection<CodeSearchResult>();
            //SearchCriteria = new SimpleSearchCriteria();
            InitAccessLevels();
            InitProgramElements();

            ((INotifyCollectionChanged)searchResultListbox.Items).CollectionChanged += SelectFirstResult;
            ((INotifyCollectionChanged)searchResultListbox.Items).CollectionChanged += ScrollToTop;

            SearchStatus = "Enter search terms - only complete words or partial words followed by a '*' are accepted as input.";

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

        public string SearchStatus
        {
            get { return (string) GetValue(SearchStatusProperty); }
            private set { SetValue(SearchStatusProperty, value); }
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

        private void OpenFileWithSelectedResult(object sender)
        {
            try
            {
                var result = sender as ListBoxItem;
                if (result != null)
                {
                    string[] searchKeys = GetKeys(this.searchKey);
                    var searchResult = result.Content as CodeSearchResult;
                    FileOpener.OpenItem(searchResult, searchBox.Text);
                    HighlightedEntitySet.GetInstance().Clear();
                    HighlightedEntitySet.GetInstance().AddEntity(searchResult.ProgramElement.FullFilePath, searchResult.
                        ProgramElement.DefinitionLineNumber, searchResult.ProgramElement.RawSource, searchKeys);

                    var matchDescription = QueryMetrics.DescribeQueryProgramElementMatch(searchResult.ProgramElement, searchBox.Text);
                    LogEvents.OpeningCodeSearchResult(searchResult, SearchResults.IndexOf(searchResult) + 1, matchDescription);
                }
            }
            catch (ArgumentException aex)
            {
                LogEvents.UIGenericError(this, aex);
                MessageBox.Show(FileNotFoundPopupMessage, FileNotFoundPopupTitle, MessageBoxButton.OK);
            }
        }

        public void Update(IQueryable<CodeSearchResult> results)
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                UpdateResults(results);
            }
            else
            {
                Dispatcher.Invoke((Action) (() => UpdateResults(results)));
            }
        }

        //Update for the Popup window Zhao
        private void UpdateResults(IEnumerable<CodeSearchResult> results)
        {
            SearchResults.Clear();
            //Concurrent opportunity (No, SearchResults is not thread safe)
            foreach (var codeSearchResult in results)
            {
                SearchResults.Add(codeSearchResult);
            }


            ////For each item in the SearchResults, generate the highlight results (Serial version)
            //foreach(var item in SearchResults) {
            //    string highlight;
            //    string highlightRaw;
            //    GenerateHighlight(item.Raw, this.searchKey, out highlight, out highlightRaw);
            //    item.Highlight = highlight;
            //    item.HighlightRaw = highlightRaw;
            //}

            //Concurrent version 
            var exceptions = new ConcurrentQueue<Exception>();
            Parallel.ForEach(SearchResults, item => {
                try {
                    string highlight;
                    string highlightRaw;
                    GenerateHighlight(item.Raw, this.searchKey, out highlight, out highlightRaw);
                    item.Highlight = highlight;
                    item.HighlightRaw = highlightRaw;
                } catch(Exception exc) { exceptions.Enqueue(exc); }
            }
                );

            //Donot consider capture the exception for the time being 
            //if(!exceptions.IsEmpty) 
            //    throw new AggregateException(exceptions);
        }

        public void GenerateHighlight(string raw, string searchKey, out string highlight_out, out string highlightRaw_out) {

            StringBuilder highlight = new StringBuilder();
            StringBuilder highlight_Raw = new StringBuilder();

            string[] lines = raw.Split('\n');
            StringBuilder newLine = new StringBuilder();

            string[] searchKeys = GetKeys(searchKey);
            string[] containedKeys;

            foreach(string line in lines) {
                
                containedKeys = IsContainSearchKey(searchKeys, line);
                
                if(containedKeys.Length != 0) {

                    string temp_line = string.Copy(line);
                    int loc;
                    //One line contain multiple words
                    foreach(string key in containedKeys) {
                        newLine.Clear();
                        while((loc = temp_line.IndexOf(key, StringComparison.InvariantCultureIgnoreCase)) >= 0) {

                            string replaceKey = "|~S~|" + temp_line.Substring(loc, key.Length) + "|~E~|";
                            newLine.Append(temp_line.Substring(0, loc) + replaceKey);
                            temp_line = temp_line.Remove(0, loc + key.Length);

                        }

                        newLine.Append(temp_line);
                        temp_line = newLine.ToString();

                    }

                    highlight.Append(newLine.ToString());
                    highlight.Append('\n');

                    highlight_Raw.Append(newLine.ToString());
                    highlight_Raw.Append('\n');

                } else {
                    highlight_Raw.Append(line);
                    highlight_Raw.Append('\n');
                }
            }

            highlight_out = highlight.ToString().Replace('\t', ' ');
            highlightRaw_out = highlight_Raw.ToString().Replace('\t', ' ');
        }

        private string[] GetKeys(string searchKey) {
            SandoQueryParser parser = new SandoQueryParser();
            var description = parser.Parse(searchKey);
            var terms = description.SearchTerms;
            HashSet<string> keys = new HashSet<string>();
            foreach(var term in terms) {
                keys.Add(DictionaryHelper.GetStemmedQuery(term));
                keys.Add(term);
            }
            foreach(var quote in description.LiteralSearchTerms)
                keys.Add(quote.Trim('"'));
            return keys.ToArray();
        }

        //Return the contained search key
        private string[] IsContainSearchKey(string[] searchKeys, string line) {

            List<string> containedSearchKey = new List<string>();

            foreach(string key in searchKeys) 
                if(line.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) > 0){
                    containedSearchKey.Add(key);
                }
            
            return containedSearchKey.ToArray();
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
            var listview = sender as ListView;
            LogEvents.SelectingCodeSearchResult(this, listview.SelectedIndex + 1);
            UpdateExpansionState(searchResultListbox);
        }

        private void Toggled(object sender, RoutedEventArgs e)
        {
            UpdateExpansionState(searchResultListbox);
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
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
                LogEvents.SelectingRecommendationItem(this, listBox.SelectedIndex + 1);
            }
        }

        private void Toggled_Popup(object sender, RoutedEventArgs e)
        {
            if(!SelectionPopup.IsOpen)
                SelectionPopup.IsOpen = true;
        }


        public static readonly DependencyProperty AccessLevelsProperty =
            DependencyProperty.Register("AccessLevels", typeof (ObservableCollection<AccessWrapper>), typeof (SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ProgramElementsProperty =
            DependencyProperty.Register("ProgramElements", typeof(ObservableCollection<ProgramElementWrapper>), typeof(SearchViewControl), new UIPropertyMetadata(null));


        public static readonly DependencyProperty SearchResultsProperty =
            DependencyProperty.Register("SearchResults", typeof(ObservableCollection<CodeSearchResult>), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SearchStringProperty =
            DependencyProperty.Register("SearchString", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SearchStatusProperty =
            DependencyProperty.Register("SearchStatus", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));


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

        private Color Good = (Color)ColorConverter.ConvertFromString("#E9FFDE");
        private Color OK = (Color)ColorConverter.ConvertFromString("#FFFFE6");
        private Color Bad = (Color)ColorConverter.ConvertFromString("#FFF0F0");

        private void RespondToLoad(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = sender as Border;
                var gradientBrush = item.Background as System.Windows.Media.LinearGradientBrush;
                Color myColor = Colors.White;
                var result = item.DataContext as CodeSearchResult;
                if (result != null)
                {
                    double score = result.Score;
                    if (score >= 0.6)
                        myColor = Good;
                    else if (score >= 0.4)
                        myColor = OK;
                    else if (score < 0.4)
                        myColor = Bad;
                    if (score > .99)
                    {
                        foreach (var stop in gradientBrush.GradientStops)
                            stop.Color = myColor;
                    }
                    else
                    {
                        gradientBrush.GradientStops.First().Color = myColor;
                        gradientBrush.GradientStops.ElementAt(1).Color = myColor;
                    }
                }

            }
            catch (Exception problem)
            {
                //ignore for now, as this is not a crucial feature
            }
        }


        private void ListViewItem_LostFocus(object sender, RoutedEventArgs e) {
            //searchResultListbox.SelectedItem = null;
            //// Mark as handled to prevent this event from bubbling up the element tree.
            //e.Handled = true;
        }

        private void MouseLeaveEvent(object sender, MouseEventArgs e) {
            searchResultListbox.SelectedItem = null;
            // Mark as handled to prevent this event from bubbling up the element tree.
            e.Handled = true;

        }

    }
}