using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using EnvDTE;
using EnvDTE80;
using System.ComponentModel;
using System.Threading;

namespace Sando.Core.Extensions.PairedInterleaving
{
	public class InterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public InterleavingManager()
		{
			LogCount = 0;
            ClickIdx = new List<int>();
            searchRecievedClick = false;
            semaphore = new AutoResetEvent(false);
			InitializeLogFileName();
		}

		public string RewriteQuery(string query)
		{
			//dump the previous query stuff to the log, assuming it was clicked
            if (searchRecievedClick)
            {
                try
                {
                    LogCount++;
                    int scoreA, scoreB;
                    BalancedInterleaving.DetermineWinner(SandoResults, SecondaryResults, InterleavedResults,
                                                         ClickIdx, out scoreA, out scoreB);

                    string entry = LogCount + ": " + FLT_A_NAME + "=" + scoreA + ", " +
                                   FLT_B_NAME + "=" + scoreB + Environment.NewLine;
                    WriteLogEntry(LogFile, entry);
                }catch(Exception e)
                {
                    //TODO - Kosta, something messed up here
                    FileLogger.DefaultLogger.Error(e.StackTrace);
                }
            }

            //capture the query and reissue it to the secondary FLT getting the secondary results
            SecondaryResults = LexSearch.GetResults(query);

			//write log to S3
            if (LogCount >= LOG_ENTRIES_PER_FILE)
            {
            	S3LogWriter.WriteLogFile(LogFile);
				InitializeLogFileName();
            }

            return query;
		}

        void findInFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //semaphore.Set();
        }

        void findInFilesWorker_DoWork(object sender, DoWorkEventArgs e, string query)
        {
            SecondaryResults = LexSearch.GetResults(query);
        }

		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
            //semaphore.WaitOne();
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
        private static AutoResetEvent semaphore;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
        public int LogCount { get; private set; }
	}
}
