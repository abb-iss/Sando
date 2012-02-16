using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
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

		public static readonly DependencyProperty SearchStringProperty =
			DependencyProperty.Register("SearchString", typeof(string), typeof(SearchViewControl), new UIPropertyMetadata(null));


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
	

    	private CodeSearcher _currentSearcher;
    	private string _currentDirectory="";    	

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
			if(!string.IsNullOrEmpty(SearchString))
			{
				var myPackage = UIPackage.GetInstance();
				_currentSearcher = GetSearcher(myPackage);
				if (SearchResults == null)
					SearchResults = new ObservableCollection<CodeSearchResult>();
				else
					SearchResults.Clear();
				foreach (var result in _currentSearcher.Search(SearchString))
				{
					SearchResults.Add(result);
				}
			}
        }

    	private CodeSearcher GetSearcher(UIPackage myPackage)
    	{
    		CodeSearcher codeSearcher = _currentSearcher;
			if(codeSearcher == null || !myPackage.GetCurrentDirectory().Equals(_currentDirectory))
			{
				_currentDirectory = myPackage.GetCurrentDirectory();
				codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(_currentDirectory));
			}
    		return codeSearcher;
    	}

    	private void OnKeyDownHandler(object sender, KeyEventArgs e)
    	{
			if(e.Key == Key.Return)
			{
				var text = sender as TextBox;
				if(text!=null)
				{
					SearchString = text.Text;
				}
				SearchButtonClick(null,null);
			}
    	}

    	private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    	{
    		var result = sender as ListBox;
			if(result!=null)
			{
				var myResult = result.SelectedItem as CodeSearchResult;
				FileOpener.OpenFile(myResult.Element.FullFilePath, myResult.Element.DefinitionLineNumber);	
			}
			
    	}

    	private static class FileOpener
    	{
			
			private static DTE2 dte = null;

    		public static void OpenFile(string filePath, int lineNumber)
    		{
    			InitDte2();
    			dte.ItemOperations.OpenFile(filePath, Constants.vsViewKindCode);
    			try
    			{
    				var selection = (TextSelection) dte.ActiveDocument.Selection;
    				selection.GotoLine(lineNumber);
    				selection.SelectLine();
    			}
    			catch (Exception)
    			{
    				//ignore, we don't want this feature ever causing a crash
    			}
    		}    	

    		private static void InitDte2()
    		{
    			if (dte == null)
    			{
    				dte = Package.GetGlobalService(typeof (DTE)) as DTE2;
    			}
    		}
    	}
    }
}