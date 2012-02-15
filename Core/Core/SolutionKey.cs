using System;
using System.Diagnostics.Contracts;

namespace Sando.Core
{
	public class SolutionKey : ISolutionKey
	{
		public SolutionKey(string solutionPath, string luceneDirectoryPath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(solutionPath), "SolutionKey:Constructor - solution path cannot be null or an empty string!");
			Contract.Requires(!String.IsNullOrWhiteSpace(luceneDirectoryPath), "SolutionKey:Constructor - lucene directory path cannot be null or an empty string!");

			this.solutionPath = solutionPath;
			this.luceneDirectoryPath = luceneDirectoryPath;
		}
		
		public string GetKeyRepresentation()
		{
			return luceneDirectoryPath;
		}

		public string GetSolutionPath()
		{
			return solutionPath;
		}

		private string solutionPath;
		private string luceneDirectoryPath;
	}
}
