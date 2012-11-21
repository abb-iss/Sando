using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.ComponentModel;
using System.Threading;
using System.IO;
using Sando.SearchEngine;

namespace Sando.UI.InterleavingExperiment
{
	public class InterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public InterleavingManager(string pluginDir)
		{
			LogCount = 0;
			ClickIdx = new List<int>();
			SearchRecievedClick = false;
			boostClassesMethodsReorderer = new BoostClassesMethodsReorderer();

			S3LogWriter.S3CredentialDirectory = pluginDir;
			InitializeNewLogFileName(pluginDir);
			PluginDirectory = pluginDir;

			SecondaryResults = new List<CodeSearchResult>();
			SandoResults = new List<CodeSearchResult>();
			InterleavedResults = new List<CodeSearchResult>();
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
                                   FLT_B_NAME + "=" + scoreB + " ; query='" + lastQuery 
								   + "'" + Environment.NewLine;
                    WriteLogEntry(entry);
                }
				catch(Exception e)
                {
                    //TODO - Kosta, something messed up here
                    FileLogger.DefaultLogger.Error(e.StackTrace);
                }
            }

			lastQuery = query;

            //capture the query and reissue it to the secondary FLT getting the secondary results
			SecondaryResults.Clear();
            SecondaryResults = LexSearch.GetResults(query);

			//write log to S3
			var s3UploadWorker = new BackgroundWorker();
			s3UploadWorker.DoWork += new DoWorkEventHandler(s3UploadWorker_DoWork);
			s3UploadWorker.RunWorkerAsync();

			ClickIdx.Clear();
			SearchRecievedClick = false;
			
            return query;
		}


		void s3UploadWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			WriteLogToS3();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void WriteLogToS3()
		{
			if(LogCount < LOG_ENTRIES_PER_FILE) return;
			var success = S3LogWriter.WriteLogFile(LogFile);
			if(success == true)
			{
				System.IO.File.Delete(LogFile);
				InitializeNewLogFileName(PluginDirectory);
				LogCount = 0;
				WriteIncompleteLogs();
			}
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
			searchResults = boostClassesMethodsReorderer.ReorderSearchResults(searchResults);
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

		private const int LOG_ENTRIES_PER_FILE = 3;
		private const string FLT_A_NAME = "Sando";
        private const string FLT_B_NAME = "Lex";
		private string LogFile;
		private string PluginDirectory;

		private string lastQuery = "?";
        private List<CodeSearchResult> SecondaryResults;
        private List<CodeSearchResult> SandoResults;
        private bool SearchRecievedClick;
		private BoostClassesMethodsReorderer boostClassesMethodsReorderer;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
        public int LogCount { get; private set; }
	}
}
