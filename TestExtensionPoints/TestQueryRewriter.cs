using Sando.ExtensionContracts.QueryContracts;

namespace Sando.TestExtensionPoints
{
	public class TestQueryRewriter : IQueryRewriter
	{
		public string RewriteQuery(string query)
		{
			return query.ToLowerInvariant();
		}
	}
}
