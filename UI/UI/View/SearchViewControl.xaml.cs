using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Sando.Core.Extensions.Logging;
using Sando.Core.Extensions;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching.Criteria;
using Sando.Translation;

namespace Sando.UI.View
{
    /// <summary>
    /// Interaction logic for SearchViewControl.xaml
    /// </summary>
    public partial class SearchViewControl : UserControl, IIndexUpdateListener, ISearchResultListener
    {
    

        private static IIndexUpdateListener _instance;

        public ObservableCollection<AccessWrapper> AccessLevels
        {
            get { return (ObservableCollection<AccessWrapper>)GetValue(AccessLevelsProperty); }
            set
            {
                SetValue(AccessLevelsProperty, value);
            }
        }


        public ObservableCollection<ProgramElementWrapper> ProgramElements
        {
            get { return (ObservableCollection<ProgramElementWrapper>)GetValue(ProgramElementsProperty); }
            set
            {
                SetValue(ProgramElementsProperty, value);
            }
        }

        public ObservableCollection<CodeSearchResult> SearchResults
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(SearchResultsProperty);
            }
            set
            {
                SetValue(SearchResultsProperty, value);
            }
        }

        public string SearchStatus
        {
            get
            {
                return (string)GetValue(SearchStatusProperty);
            }
            private set
            {
                SetValue(SearchStatusProperty, value);
            }
        }

        public SimpleSearchCriteria SearchCriteria
        {
            get
            {                
                return (SimpleSearchCriteria)GetValue(SearchCriteriaProperty);
            }
            set
            {
                SetValue(SearchCriteriaProperty, value);
            }
        }

        public string SearchLabel
        {
            get
            {
                return Translator.GetTranslation(TranslationCode.SearchLabel);
            }
        }

        public string ComboBoxItemCurrentDocument
        {
            get
            {
                return Translator.GetTranslation(TranslationCode.ComboBoxItemCurrentDocument);
            }
        }

        public string ComboBoxItemEntireSolution
        {
            get
            {
                return Translator.GetTranslation(TranslationCode.ComboBoxItemEntireSolution);
            }
        }




        public static readonly DependencyProperty AccessLevelsProperty =
    DependencyProperty.Register("AccessLevels", typeof(ObservableCollection<AccessWrapper>), typeof(SearchViewControl), new UIPropertyMetadata(null));

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

        

        private SearchManager _searchManager;


        public SearchViewControl()
        {
            _instance = this;
            this.DataContext = this; //so we can show results
            InitializeComponent();

            _searchManager = new SearchManager(this);
            SearchResults = new ObservableCollection<CodeSearchResult>();
            SearchCriteria = new SimpleSearchCriteria();
            InitAccessLevels();
            InitProgramElements();
            ((INotifyCollectionChanged)searchResultListbox.Items).CollectionChanged += selectFirstResult;

            SearchStatus = "Enter search terms - only complete words or partial words followed by a '*' are accepted as input.";
        }

        private void InitProgramElements()
        {
            ProgramElements = new ObservableCollection<ProgramElementWrapper>();
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Class));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Comment));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Custom));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.DocComment));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Enum));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Field));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Method));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.MethodPrototype));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Property));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.Struct));
            ProgramElements.Add(new ProgramElementWrapper(true, ProgramElementType.TextLine));
        }

        private void InitAccessLevels()
        {
            AccessLevels = new ObservableCollection<AccessWrapper>();           
            AccessLevels.Add(new AccessWrapper(true, AccessLevel.Private));
            AccessLevels.Add(new AccessWrapper(true, AccessLevel.Protected));
            AccessLevels.Add(new AccessWrapper(true, AccessLevel.Internal));
            AccessLevels.Add(new AccessWrapper(true, AccessLevel.Public));
        }

        private void selectFirstResult(object sender, NotifyCollectionChangedEventArgs e)
        {
            //searchResultListbox.SelectedIndex = 0;
            //searchResultListbox_SelectionChanged(searchResultListbox,null);
            searchResultListbox.SelectedIndex = -1;
            searchResultListbox.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e) {
            BeginSearch(searchBox.Text);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e) {
            if(e.Key == Key.Return) {
                var text = sender as TextBox;
                if(text != null) {
                    BeginSearch(text.Text);
                }
            }
        }

        private void BeginSearch(string searchString) {
            //set access type
            SearchCriteria.SearchByAccessLevel = true;
            bool allchecked = true;
            SearchCriteria.AccessLevels.Clear();
            foreach (var accessWrapper in AccessLevels)
            {
                if(accessWrapper.Checked)
                {
                    SearchCriteria.AccessLevels.Add(accessWrapper.Access);       
                }else
                {
                    allchecked = false;
                }
            }
            if (allchecked) SearchCriteria.SearchByAccessLevel = false;


            allchecked = true;
            SearchCriteria.SearchByProgramElementType = true;
            SearchCriteria.ProgramElementTypes.Clear();
            foreach (var progElement in ProgramElements)
            {
                if (progElement.Checked)
                {
                    SearchCriteria.ProgramElementTypes.Add(progElement.ProgramElement);
                }
                else
                {
                    allchecked = false;
                }
            }
            if (allchecked) SearchCriteria.SearchByProgramElementType = false;
                      
            //execute search
            SearchAsync(searchString, SearchCriteria);
        }
        
        private void SearchAsync(String text, SimpleSearchCriteria searchCriteria)
        {
            BackgroundWorker sandoWorker = new BackgroundWorker();
            sandoWorker.DoWork += new DoWorkEventHandler(sandoWorker_DoWork);
            var workerSearchParams = new WorkerSearchParameters() { query = text, criteria = searchCriteria };
            sandoWorker.RunWorkerAsync(workerSearchParams);
        }

        private class WorkerSearchParameters
        {
            public SimpleSearchCriteria criteria;
            public String query;
        }
      
        void sandoWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var searchParams = (WorkerSearchParameters)e.Argument;
        	string searchStatus = String.Empty;

			if(ExtensionPointsRepository.IsInterleavingExperimentOn)
			{
				Task.Factory.StartNew(() =>
				{
					ExtensionPointsRepository.ExpFlow.Value = ExperimentFlow.A;
					searchStatus = _searchManager.Search(searchParams.query, searchParams.criteria);
				});
				Task.Factory.StartNew(() =>
				{
					ExtensionPointsRepository.ExpFlow.Value = ExperimentFlow.B;
					_searchManager.Search(searchParams.query, searchParams.criteria);
				});
			}
			else
			{
				searchStatus = _searchManager.Search(searchParams.query, searchParams.criteria);
			}

            e.Result = searchStatus;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FileOpener.OpenItem(sender);
            }
            catch (ArgumentException aex)
            {
                FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(aex));
                MessageBox.Show(fileNotFoundPopupMessage, fileNotFoundPopupTitle, MessageBoxButton.OK);
            }
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    FileOpener.OpenItem(sender);
                }
                catch(ArgumentException aex)
                {
                    FileLogger.DefaultLogger.Error(ExceptionFormatter.CreateMessage(aex));
                    MessageBox.Show(fileNotFoundPopupMessage, fileNotFoundPopupTitle, MessageBoxButton.OK);
                }
            }
        }

        public static IIndexUpdateListener GetInstance()
        {
            if(_instance==null)
            {
                UIPackage.GetInstance().EnsureViewExists();
            }
            return _instance;
        }

        #region Implementation of IIndexUpdateListener

        public void NotifyAboutIndexUpdate()
        {
            _searchManager.MarkInvalid();
        }

        #endregion

        #region Implementation of ISearchResultListener

        public void Update(IQueryable<CodeSearchResult> results)
        {
            if (Thread.CurrentThread == this.Dispatcher.Thread)
            {
                UpdateResults(results);
            }else
            {
                Dispatcher.Invoke(
                (Action)(() => UpdateResults(results)));
            }     	    
        }

        private void UpdateResults(IQueryable<CodeSearchResult> results)
        {
            SearchResults.Clear();
            foreach (var codeSearchResult in results)
            {
                SearchResults.Add(codeSearchResult);
            }
        }

        public void UpdateMessage(string message)
        {
            if (Thread.CurrentThread == this.Dispatcher.Thread)
            {
                SetMessage(message);
            }
            else
            {
                Dispatcher.Invoke(
                (Action)(() =>  SetMessage(message)));
            }
        }

        private void SetMessage(string message)
        {
            SearchStatus = message;
        }

        #endregion

        private void searchResultListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as ListView;                        
            if(view!=null)
            {
                int index = view.SelectedIndex;
                int currentIndex = 0;
                foreach(var item in view.Items)
                {
                    var currentItem = view.ItemContainerGenerator.ContainerFromIndex(currentIndex) as ListViewItem;
                    if (currentItem != null)
                    {
                        if (Math.Abs(currentIndex - index) == 0 && index != -1)
                        {
                            currentItem.Height = 89;
                        }
                        else
                        {
                            currentItem.Height = 24;
                        }
                    }
                    currentIndex++;
                }
            }
        }

        private static string fileNotFoundPopupMessage = "This file cannot be opened. It may have been deleted, moved, or renamed since your last search.";
        private static string fileNotFoundPopupTitle = "File opening error";
    }

    public  class AccessWrapper
    {
        public AccessWrapper(bool b, AccessLevel access
            )
        {
            this.Checked = b;
            this.Access = access;
        }

        public bool Checked { get; set; }
        public AccessLevel Access { get; set; }
    }


    public class ProgramElementWrapper
    {
        public ProgramElementWrapper(bool b, ProgramElementType access
            )
        {
            this.Checked = b;
            this.ProgramElement = access;
        }

        public bool Checked { get; set; }
        public ProgramElementType ProgramElement { get; set; }
    }

    #region ValueConverter of SearchResult's Icon
    [ValueConversion(typeof(string), typeof(BitmapImage))] 
    public class ElementToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }

            ProgramElement element = value as ProgramElement;
            if (element == null) { return GetBitmapImage("../Resources/VS2010Icons/generic.png"); }
            if(element as CommentElement !=null || element as DocCommentElement !=null)
            {                
                string resource = "../Resources/VS2010Icons/comment.png";
                return GetBitmapImage(resource);   
            }
            string accessLevel;
            PropertyInfo info = element.GetType().GetProperty("AccessLevel");
            if (info != null)
                accessLevel = "_" + info.GetValue(element, null).ToString();
            else
                accessLevel = string.Empty;
            if (accessLevel.ToLower() == "_public")
                accessLevel = "";

            ProgramElementType programElementType = element.ProgramElementType;
            if(programElementType.Equals(ProgramElementType.MethodPrototype))
            {
                programElementType = ProgramElementType.Method;
            }
            if (programElementType.Equals(ProgramElementType.Field))
            {
                programElementType = ProgramElementType.Property;
            }
            string resourceName = "";
            if (programElementType.Equals(ProgramElementType.Struct))
            {
                resourceName = string.Format("../Resources/VS2010Icons/VSObject_{0}{1}.png", "Structure", accessLevel);    
            }
            else if(programElementType.Equals(ProgramElementType.TextLine))
            {
                resourceName = string.Format("../Resources/VS2010Icons/xmlIcon.png", programElementType, accessLevel);    
            }else
            {
                resourceName = string.Format("../Resources/VS2010Icons/VSObject_{0}{1}.png", programElementType, accessLevel);    
            }		    
            return GetBitmapImage(resourceName);
        }

        static Dictionary<string,BitmapImage>  images = new Dictionary<string, BitmapImage>();

        private static BitmapImage GetBitmapImage(string resource)
        {
            BitmapImage image;
            if (images.TryGetValue(resource, out image))
                return image;
            else
            {
                image = new BitmapImage(new Uri(resource, UriKind.Relative));
                images[resource] = image;
                return image;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class FileTypeToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }

            string type = (string)value;
            string resourceName = string.Format("../Resources/Code_{0}.png", type.Substring(type.LastIndexOf('.') + 1));
            return new BitmapImage(new Uri(resourceName, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
    #endregion

    #region NullableBoolToBool

    [ValueConversion(typeof(bool?), typeof(bool))]
    public class NullableBoolToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((bool?)value).HasValue)
                return ((bool?)value).Value;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    #endregion

    #region NullOrEmptyToVisibility Converter
    [ValueConversion(typeof(int), typeof(string))]
    public class NullOrEmptyToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "Hidden";
            if ((int)value == 0)
            {
                return "Hidden";
            }
            else
                return "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    #endregion

    #region NullOrEmptyIsHidden Converter
    [ValueConversion(typeof(int), typeof(string))]
    public class NullOrEmptyIsHidden : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "21";
            if ((int)value == 0)
            {
                return "21";
            }
            else
                return "41";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    #endregion
}