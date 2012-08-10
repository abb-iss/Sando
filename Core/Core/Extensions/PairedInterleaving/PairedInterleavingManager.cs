using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.Core.Extensions.PairedInterleaving
{
	class PairedInterleavingManager : IQueryRewriter, IResultsReorderer
	{
		private List<CodeSearchResult> SecondaryResults;
		private PairedInterleavingLogEntry LogEntry;
		private int LogCount;

		public PairedInterleavingManager()
		{
			LogCount = 0;
		}

		public string RewriteQuery(string query)
		{
			//write a log entry for the previous query (now that the clicking has completed for it)

			//capture the query and reissue it to the secondary FLT getting the secondary results

			//check the number of log entires collected and decide whether to push the log to S3

			

			throw new NotImplementedException();
		}

		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
			//interleave the search results from the primary and secondary FLT

			//create a new log entry
			LogCount++;
			LogEntry = new PairedInterleavingLogEntry(LogCount, searchResults.ToList());

			throw new NotImplementedException();
		}

		//called from UI.FileOpener
		public void NotifyClicked(CodeSearchResult clickedElement)
		{
			//determine which FLT this element belongs to and maintain a count 
			LogEntry.CountClick(clickedElement);

			throw new NotImplementedException();
		}
	}
}
