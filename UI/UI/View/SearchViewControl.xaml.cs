using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.Indexer;
using Sando.Indexer.Searching.Criteria;
using Sando.Translation;
using System.ComponentModel;

namespace Sando.UI.View
{
    /// <summary>
    /// Interaction logic for SearchViewControl.xaml
    /// </summary>
    public partial class SearchViewControl : UserControl, IIndexUpdateListener, ISearchResultListener
    {
	

		private static IIndexUpdateListener _instance;

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

		public string SearchString
		{
			get
			{
				return (string)GetValue(SearchStringProperty);
			}
			set
			{
				SetValue(SearchStringProperty, value);
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

		public ObservableCollection<string> ProgramElementTypeList
		{
			get
			{
				ObservableCollection<string> list = (ObservableCollection<string>)GetValue(ProgramElementTypeListProperty);
				return list;
			}
		}

		public List<string> ProgramElementTypes
		{
			get
			{
				List<string> list = Enum.GetValues(typeof(ProgramElementType)).Cast<string>().ToList<string>();
				list.Add("all");
				return list;
			}
		}

		public static readonly DependencyProperty SearchResultsProperty =
			DependencyProperty.Register("SearchResults", typeof(ObservableCollection<CodeSearchResult>), typeof(SearchViewControl), new UIPropertyMetadata(null));

		public static readonly DependencyProperty SearchStringProperty =
			DependencyProperty.Register("SearchString", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SearchStatusProperty =
             DependencyProperty.Register("SearchStatus", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));


        public static readonly DependencyProperty SearchCriteriaProperty =
			DependencyProperty.Register("SearchCriteria", typeof(SimpleSearchCriteria), typeof(SearchViewControl), new UIPropertyMetadata(null));

		public static readonly DependencyProperty ProgramElementTypeListProperty =
			DependencyProperty.Register("ProgramElementTypes", typeof(ObservableCollection<string>), typeof(SearchViewControl), new UIPropertyMetadata(null));

    	private SearchManager _searchManager;

        private class WorkerSearchParameters
        {
            public SimpleSearchCriteria criteria;
            public String query;
        }


    	public SearchViewControl()
    	{
    		_instance = this;
            this.DataContext = this; //so we can show results
            InitializeComponent();

    		_searchManager = new SearchManager(this);
			SearchResults = new ObservableCollection<CodeSearchResult>();
			SearchCriteria = new SimpleSearchCriteria();
            ((INotifyCollectionChanged)searchResultListbox.Items).CollectionChanged += selectFirstResult;

    	    SearchStatus = "Enter search terms - only complete words or partial words followed by a '*' are accepted as input.";
    	}

        private void selectFirstResult(object sender, NotifyCollectionChangedEventArgs e)
        {
            //searchResultListbox.SelectedIndex = 0;
            //searchResultListbox_SelectionChanged(searchResultListbox,null);
            searchResultListbox.SelectedIndex = -1;
            searchResultListbox.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
    	{
			if (searchAccessLevel.SelectedIndex == 0)
				SearchCriteria.SearchByAccessLevel = false;
			else
			{
				SearchCriteria.SearchByAccessLevel = true;
				SearchCriteria.AccessLevels.Clear();
				SearchCriteria.AccessLevels.Add((AccessLevel)searchAccessLevel.SelectedItem);
			}
			if (searchElementType.SelectedIndex == 0)
				SearchCriteria.SearchByProgramElementType = false;
			else
			{
				SearchCriteria.SearchByProgramElementType = true;
				SearchCriteria.ProgramElementTypes.Clear();
				SearchCriteria.ProgramElementTypes.Add((ProgramElementType)searchElementType.SelectedItem);
			}

            BackgroundWorker sandoWorker = new BackgroundWorker();
            sandoWorker.DoWork += new DoWorkEventHandler(sandoWorker_DoWork);
            var workerSearchParams = new WorkerSearchParameters() { query = SearchString, criteria = SearchCriteria };
            sandoWorker.RunWorkerAsync(workerSearchParams);
			//SearchStatus = _searchManager.Search(SearchString, SearchCriteria);            
    	}

    	private void OnKeyDownHandler(object sender, KeyEventArgs e)
    	{
			if(e.Key == Key.Return)
			{
				var text = sender as TextBox;
				if (text != null)
				{
					if (searchAccessLevel.SelectedIndex == 0)
						SearchCriteria.SearchByAccessLevel = false;
					else
					{
						SearchCriteria.SearchByAccessLevel = true;
						SearchCriteria.AccessLevels.Clear();
						SearchCriteria.AccessLevels.Add((AccessLevel)searchAccessLevel.SelectedItem);
					}
					if (searchElementType.SelectedIndex == 0)
						SearchCriteria.SearchByProgramElementType = false;
					else
					{
						SearchCriteria.SearchByProgramElementType = true;
						SearchCriteria.ProgramElementTypes.Clear();
						SearchCriteria.ProgramElementTypes.Add((ProgramElementType)searchElementType.SelectedItem);
					}

                    BackgroundWorker sandoWorker = new BackgroundWorker();
                    sandoWorker.DoWork += new DoWorkEventHandler(sandoWorker_DoWork);
                    var workerSearchParams = new WorkerSearchParameters() { query = text.Text, criteria = SearchCriteria };
                    sandoWorker.RunWorkerAsync(workerSearchParams);
                    //SearchStatus = _searchManager.SearchOnReturn(sender, e, text.Text, SearchCriteria);                    
				}
			}
    	}

        void sandoWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var searchParams = (WorkerSearchParameters)e.Argument;
            var searchStatus = _searchManager.Search(searchParams.query, searchParams.criteria);
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
				UIPackage.GetInstance().EnsureSandoRunning();
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
		    string resourceName = string.Format("../Resources/VS2010Icons/VSObject_{0}{1}.png", programElementType, accessLevel);
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