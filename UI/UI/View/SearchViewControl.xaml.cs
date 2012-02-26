using System;
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
    public partial class SearchViewControl : UserControl, IIndexUpdateListener
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

		private CodeSearcher _currentSearcher;
		private string _currentDirectory = "";
		private bool _invalidated = true;

    	public SearchViewControl()
    	{
    		_instance = this;
            this.DataContext = this; //so we can show results
            InitializeComponent();            
        }

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void SearchButtonClick(object sender, RoutedEventArgs e)
    	{
    		Search();
    	}

    	private void OnKeyDownHandler(object sender, KeyEventArgs e)
    	{
    		SearchOnReturn(sender, e);
    	}

    	private void SearchOnReturn(object sender, KeyEventArgs e)
    	{
    		if (e.Key == Key.Return)
    		{
    			var text = sender as TextBox;
    			if (text != null)
    			{
    				SearchString = text.Text;
    			}
    			Search();
    		}
    	}
		private CodeSearcher GetSearcher(UIPackage myPackage)
		{
			CodeSearcher codeSearcher = _currentSearcher;
			if(codeSearcher == null || !myPackage.GetCurrentDirectory().Equals(_currentDirectory) || _invalidated)
			{
				_invalidated = false;
				_currentDirectory = myPackage.GetCurrentDirectory();
				codeSearcher = new CodeSearcher(IndexerSearcherFactory.CreateSearcher(myPackage.GetCurrentSolutionKey()));
			}
			return codeSearcher;
		}

		private void Search()
		{
			if(!string.IsNullOrEmpty(SearchString))
			{
				var myPackage = UIPackage.GetInstance();
				_currentSearcher = GetSearcher(myPackage);
				if(SearchResults == null)
					SearchResults = new ObservableCollection<CodeSearchResult>();
				else
					SearchResults.Clear();
				foreach(var result in _currentSearcher.Search(SearchString))
				{
					SearchResults.Add(result);
				}
			}
		}

    	private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    	{
    		FileOpener.OpenItem(sender);
    	}

    	private static class FileOpener
    	{
			
			private static DTE2 dte = null;

    		public static void OpenItem(object sender)
    		{
    			var result = sender as ListBox;
    			if(result != null)
    			{
    				var myResult = result.SelectedItem as CodeSearchResult;
    				OpenFile(myResult.Element.FullFilePath, myResult.Element.DefinitionLineNumber);
    			}
    		}

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
    		_invalidated = true;
    	}

    	#endregion
    }
}