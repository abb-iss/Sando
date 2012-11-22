using System.Linq;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.SearchEngine
{
	public class BoostClassesMethodsReorderer : IResultsReorderer
	{
		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
			foreach(CodeSearchResult result in searchResults)
			{
				if(result.Element is ClassElement)
				{
					result.Score = result.Score * 3;
				}
				if(result.Element is MethodElement)
				{
					result.Score = result.Score * 2;
				}
			}
	
			return searchResults.OrderByDescending(r => r.Score);
		}
	}
}

