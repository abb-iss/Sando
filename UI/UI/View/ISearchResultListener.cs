using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.SearchEngine;

namespace Sando.UI.View
{
	public interface ISearchResultListener
	{
		void Update(List<CodeSearchResult> results);
	}
}
