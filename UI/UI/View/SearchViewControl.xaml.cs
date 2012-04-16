using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer;
using Sando.SearchEngine;
using Sando.Translation;
using Sando.Indexer.Searching.Criteria;

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

		public ProgramElementType ElementType
		{
			get
			{
				return ElementType;
			}
		}

		public static readonly DependencyProperty SearchResultsProperty =
			DependencyProperty.Register("SearchResults", typeof(ObservableCollection<CodeSearchResult>), typeof(SearchViewControl), new UIPropertyMetadata(null));

		public static readonly DependencyProperty SearchStringProperty =
			DependencyProperty.Register("SearchString", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));

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
    	}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
    	{
			SearchCriteria = new SimpleSearchCriteria();
			SearchCriteria.AccessLevels.Add((AccessLevel)searchAccessLevel.SelectedItem);
			SearchCriteria.ProgramElementTypes.Add((ProgramElementType)searchElementType.SelectedItem);
			SearchCriteria.SearchByAccessLevel = searchByAccessLevel.IsChecked.HasValue && searchByAccessLevel.IsChecked.Value;
			SearchCriteria.SearchByProgramElementType = searchByProgramElementType.IsChecked.HasValue && searchByProgramElementType.IsChecked.Value;

			_searchManager.Search(SearchString, SearchCriteria);
			resultExpander.IsExpanded = true;
    	}

    	private void OnKeyDownHandler(object sender, KeyEventArgs e)
    	{
			if(e.Key == Key.Return)
			{
				var text = sender as TextBox;
				if (text != null)
				{
					SearchCriteria = new SimpleSearchCriteria();
					SearchCriteria.AccessLevels.Add((AccessLevel)searchAccessLevel.SelectedItem);
					SearchCriteria.ProgramElementTypes.Add((ProgramElementType)searchElementType.SelectedItem);
					SearchCriteria.SearchByAccessLevel = searchByAccessLevel.IsChecked.HasValue && searchByAccessLevel.IsChecked.Value;
					SearchCriteria.SearchByProgramElementType = searchByProgramElementType.IsChecked.HasValue && searchByProgramElementType.IsChecked.Value;
					
					//_searchManager.SearchOnReturn(sender, e, text.Text, SearchCriteria);
					_searchManager.Search(text.Text, SearchCriteria);
					resultExpander.IsExpanded = true;
				}
			}
    	}

    	private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    	{
    		FileOpener.OpenItem(sender);
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

    	public void Update(List<CodeSearchResult> results)
    	{
    		SearchResults.Clear();
    		foreach (var codeSearchResult in results)
    		{
    			SearchResults.Add(codeSearchResult);
    		}
    	}

    	#endregion
    }

	#region ValueConverter of SearchResult's Icon
	[ValueConversion(typeof(string), typeof(BitmapImage))] 
	public class ElementToIcon : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// empty images are empty...
			if (value == null) { return null; }

			ProgramElement element = (ProgramElement)value;
			string accessLevel;
			PropertyInfo info = element.GetType().GetProperty("AccessLevel");
			if (info != null)
				accessLevel = "_" + info.GetValue(element, null).ToString();
			else
				accessLevel = string.Empty;
			if (accessLevel.ToLower() == "_public")
				accessLevel = "";


			string resourceName = string.Format("../Resources/VS2010Icons/VSObject_{0}{1}.png", element.ProgramElementType, accessLevel);
			return new BitmapImage(new Uri(resourceName, UriKind.Relative));
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
}