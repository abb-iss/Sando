using System;
using System.Collections.Generic;
using System.Linq;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace Sando.UI.InterleavingExperiment
{
	public class InterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public InterleavingManager(string pluginDir)
		{
			LogCount = 0;
			ClickIdx = new List<int>();
			SearchRecievedClick = false;
			semaphore = new AutoResetEvent(false);
			S3LogWriter.S3CredentialDirectory = pluginDir;
			InitializeNewLogFileName(pluginDir);
			PluginDirectory = pluginDir;
		}

		public string RewriteQuery(string query)
		{
			//dump the previous query stuff to the log, assuming it was clicked
            if (SearchRecievedClick)
            {
                try
                {
                    LogCount++;
                    int scoreA, scoreB;
                    BalancedInterleaving.DetermineWinner(SandoResults, SecondaryResults, InterleavedResults,
                                                         ClickIdx, out scoreA, out scoreB);

                    string entry = LogCount + ": " + FLT_A_NAME + "=" + scoreA + ", " +
                                   FLT_B_NAME + "=" + scoreB + Environment.NewLine;
                    WriteLogEntry(entry);
                }
				catch(Exception e)
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
            	bool success = S3LogWriter.WriteLogFile(LogFile);
				if(success == true)
				{
					System.IO.File.Delete(LogFile);
                    WriteIncompleteLogs();
					InitializeNewLogFileName(PluginDirectory);
					LogCount = 0;
				}
				else
				{
					FileLogger.DefaultLogger.Debug("Uploading the S3 was not successful for unknown reasons. Retry in 2 searches.");
					LogCount -= 2; //try again after 10 more entries
				}
            }

			ClickIdx.Clear();
			SearchRecievedClick = false;
			
            return query;
		}

        private void WriteIncompleteLogs()
        {
            string[] files = Directory.GetFiles(PluginDirectory);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                string fileName = Path.GetFileName(fullPath);

                if (fileName.StartsWith("PI-") && fileName.EndsWith("log"))
                {
                    bool success = S3LogWriter.WriteLogFile(fullPath);
                    if (success == true)
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
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
                SearchRecievedClick = true;
            }
		}

		private void WriteLogEntry(string entry)
		{
			System.IO.File.AppendAllText(LogFile, entry);
		}

		private void InitializeNewLogFileName(string Dir)
		{
			string machine = Environment.MachineName;
			machine = machine.Replace(' ', '_');
			machine = machine.Substring(0, (machine.Length < 10) ? machine.Length : 9);
			LogFile = Dir + "\\PI-" + machine + "-" + Guid.NewGuid() + ".log";
		}

		private const int LOG_ENTRIES_PER_FILE = 5;
		private const string FLT_A_NAME = "Sando";
        private const string FLT_B_NAME = "Lex";
		private string LogFile;
		private string PluginDirectory;

        private List<CodeSearchResult> SecondaryResults;
        private List<CodeSearchResult> SandoResults;
        private bool SearchRecievedClick;
        private static AutoResetEvent semaphore;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
        public int LogCount { get; private set; }
	}
}
