using System.Linq;
using Sando.Core.Tools;
using Sando.ExtensionContracts.QueryContracts;

namespace Sando.Indexer.Searching
{
	public class DefaultQueryRewriter : IQueryRewriter
	{        

		public string RewriteQuery(string query)
		{
		    return query;
		}
	}
}
