using System.Linq;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.View
{
	public interface ISearchResultListener
	{
		void Update(IQueryable<CodeSearchResult> results);
	    void UpdateMessage(string message);
	}
}
