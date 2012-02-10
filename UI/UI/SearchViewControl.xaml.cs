using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Sando.Core;
using Sando.Indexer.Searching;
using Sando.SearchEngine;

namespace Sando.UI
{
    /// <summary>
    /// Interaction logic for SearchViewControl.xaml
    /// </summary>
    public partial class SearchViewControl : UserControl
    {
    	public SearchViewControl()
        {
            this.DataContext = this; //so we can show results
    		//InitFake();
            InitializeComponent();            
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

		public static readonly DependencyProperty SearchResultsProperty =
			DependencyProperty.Register("SearchResults", typeof(ObservableCollection<CodeSearchResult>), typeof(SearchViewControl), new UIPropertyMetadata(null));

   

    

		public String SearchString
		{
			get; set;
		}		

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
        	var myPackage = UIPackage.GetInstance();        	
			var searcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(myPackage.GetCurrentDirectory()));
			if(SearchResults==null)
				SearchResults = new ObservableCollection<CodeSearchResult>();				
			else
				SearchResults.Clear();
			foreach(var result in searcher.Search(SearchString))
        	{
        		SearchResults.Add(result);
        	}        	
        }
    }
}