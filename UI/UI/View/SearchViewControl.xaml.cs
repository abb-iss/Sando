using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using Sando.Indexer;
using Sando.SearchEngine;
using Sando.Translation;
using System.Windows.Media.Imaging;

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

		public string SearchLabel
		{
			get
			{
				return Translator.GetTranslation(TranslationCode.SearchLabel);
			}
		}

		public static readonly DependencyProperty SearchResultsProperty =
			DependencyProperty.Register("SearchResults", typeof(ObservableCollection<CodeSearchResult>), typeof(SearchViewControl), new UIPropertyMetadata(null));

		public static readonly DependencyProperty SearchStringProperty =
			DependencyProperty.Register("SearchString", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));

    	private SearchManager _searchManager;


    	public SearchViewControl()
    	{
    		_instance = this;
            this.DataContext = this; //so we can show results
            InitializeComponent();
    		_searchManager = new SearchManager(this);
			SearchResults = new ObservableCollection<CodeSearchResult>();
    	}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
    	{
    		_searchManager.Search(SearchString);
    	}

    	private void OnKeyDownHandler(object sender, KeyEventArgs e)
    	{
			if(e.Key == Key.Return)
			{
				var text = sender as TextBox;
				if (text != null)
				{
					_searchManager.SearchOnReturn(sender, e, text.Text);
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

	[ValueConversion(typeof(string), typeof(BitmapImage))] 
	public class StringToBitmapImage : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// empty images are empty...
			if (value == null) { return null; }
		
			/// I don't know why this converter doesn't work if I use Path=/
			/// However, I choose to use Path=Icon and add a property CodeSearchResult.Icon
			/*
			 * CodeSearchResult codeSearchResult = (CodeSearchResult)value;
			/// TODO: Finish the Icon Resource Name - codeSearchResult translation <hiprince>
			/// I can't access the AccessLevel property directly, therefore I just leave it to public as default.
			 * string resourceName = string.Format("../Resources/VS2010Icons/VSObject_{0}.png", codeSearchResult.Type);
			 * return new BitmapImage(new Uri(resourceName, UriKind.Relative));*/
			return new BitmapImage(new Uri((string)value, UriKind.Relative));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}