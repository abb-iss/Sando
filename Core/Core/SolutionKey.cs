using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class SolutionKey
	{
		public SolutionKey(Guid solutionId, string solutionPath, string indexPath)
		{
			Contract.Requires(solutionId != null, "SolutionKey:Constructor - solution id cannot be null!");
			Contract.Requires(solutionId != Guid.Empty, "SolutionKey:Constructor - solution id cannot be an empty guid!");
			Contract.Requires(!String.IsNullOrWhiteSpace(solutionPath), "SolutionKey:Constructor - solution path cannot be null or an empty string!");
			Contract.Requires(!String.IsNullOrWhiteSpace(indexPath), "SolutionKey:Constructor - index path cannot be null or an empty string!");

			this.solutionId = solutionId;
			this.solutionPath = solutionPath;
			this.indexPath = indexPath;
		}
		
		public Guid GetSolutionId()
		{
			return this.solutionId;
		}

		public string GetIndexPath()
		{
			return indexPath;
		}

		public string GetSolutionPath()
		{
			return solutionPath;
		}

		private Guid solutionId;
		private string solutionPath;
		private string indexPath;
	}
}
