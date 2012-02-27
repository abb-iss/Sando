using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Sando.Indexer;
using Sando.Indexer.Searching;
using Sando.SearchEngine;
using Sando.Translation;

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
}