using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.Core.Extensions.PairedInterleaving
{
	public class PairedInterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public PairedInterleavingManager()
		{
			LogCount = 0;
            ClickIdx = new List<int>();
            searchRecievedClick = false;
			InitializeLogFileName();
		}

		public string RewriteQuery(string query)
		{
			//dump the previous query stuff to the log, assuming it was clicked
            if (searchRecievedClick)
            {
                LogCount++;
                int scoreA, scoreB;
                BalancedInterleaving.DetermineWinner(SandoResults, SecondaryResults, InterleavedResults, 
														ClickIdx, out scoreA, out scoreB);

            	string entry = LogCount + ": " + FLT_A_NAME + "=" + scoreA + ", " + FLT_B_NAME + "=" + scoreB + Environment.NewLine;
				WriteLogEntry(LogFile, entry);
            }

			//TODO: capture the query and reissue it to the secondary FLT getting the secondary results
			SecondaryResults = SandoResults;
			SecondaryResults.Reverse();

			//write log to S3
            if (LogCount >= LOG_ENTRIES_PER_FILE)
            {
            	S3LogWriter.WriteLogFile(LogFile);
				InitializeLogFileName();
            }

            return query;
		}

		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
            SandoResults = searchResults.ToList();
            InterleavedResults = BalancedInterleaving.Interleave(searchResults.ToList(), SecondaryResults);
            return InterleavedResults.AsQueryable(); 
        }

		//called from UI.FileOpener
		public void NotifyClicked(CodeSearchResult clickedElement)
		{
            if (InterleavedResults != null && InterleavedResults.Count > 0)
            {
                ClickIdx.Add(InterleavedResults.IndexOf(clickedElement));
                searchRecievedClick = true;
            }
		}

		private void WriteLogEntry(string filename, string entry)
		{
			System.IO.File.AppendAllText(filename, entry);
		}

		private void InitializeLogFileName()
		{
			LogFile = Environment.CurrentDirectory + "\\PairedInterleaving-" + Environment.MachineName + "-" + Guid.NewGuid() + ".dat";
		}

		private const int LOG_ENTRIES_PER_FILE = 50;
		private const string FLT_A_NAME = "Sando";
        private const string FLT_B_NAME = "Lex";
		private string LogFile;

        private List<CodeSearchResult> SecondaryResults;
        private List<CodeSearchResult> SandoResults;
        private bool searchRecievedClick;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
        public int LogCount { get; private set; }
	}
}
