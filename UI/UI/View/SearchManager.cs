using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Sando.Indexer.Searching;
using Sando.SearchEngine;

namespace Sando.UI.View
{
public  class SearchManager
		{

			private CodeSearcher _currentSearcher;
			private string _currentDirectory = "";
			private bool _invalidated = true;
			private ISearchResultListener _myDaddy;

			public SearchManager(ISearchResultListener daddy)
			{
				_myDaddy = daddy;
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

			public void Search(String searchString)
			{
				if(!string.IsNullOrEmpty(searchString))
				{
					var myPackage = UIPackage.GetInstance();
					_currentSearcher = GetSearcher(myPackage);
					_myDaddy.Update(_currentSearcher.Search(searchString));
				}
			}

			public void SearchOnReturn(object sender, KeyEventArgs e, String searchString)
			{
				if(e.Key == Key.Return)
				{
					Search(searchString);
				}
			}

			public void MarkInvalid()
			{
				_invalidated = true;
			}
		}

}
