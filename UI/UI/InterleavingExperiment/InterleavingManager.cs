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
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.InterleavingExperiment
{
	public class InterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public InterleavingManager(string pluginDir)
		{
			LogCount = 0;
			ClickIdx = new List<int>();
			SearchRecievedClick = false;

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

                    string entry = LogCount + ": " + FLT_A_NAME + "=" + scoreA + ", " + FLT_B_NAME + "=" + scoreB + " ; "
                                   + "query='" + lastQuery + "' ; "
                                   + SandoResults.Count + ", " + SecondaryResults.Count + "(" + NumRawSecondaryResults + ") ; "
                                   + "ptmatchclick=" + PartialTermMatchInClicked + ", exmatchclick=" + ExactTermMatchInClicked + " ; "
								   + "clicked=" + ClickedElementTypes 
                                   + Environment.NewLine;
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
            NumRawSecondaryResults = LexSearch.NumRawResults;

			//write log to S3
			var s3UploadWorker = new BackgroundWorker();
			s3UploadWorker.DoWork += new DoWorkEventHandler(s3UploadWorker_DoWork);
			s3UploadWorker.RunWorkerAsync();

			ClickIdx.Clear();
			SearchRecievedClick = false;
            ExactTermMatchInClicked = "";
            PartialTermMatchInClicked = "";
            ClickedElementTypes = "";

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
            SandoResults = searchResults.ToList();
            //TagResultsForDebugging(SandoResults, SecondaryResults);
            InterleavedResults = BalancedInterleaving.Interleave(searchResults.ToList(), SecondaryResults);
            return InterleavedResults.AsQueryable(); 
        }

        private void TagResultsForDebugging(List<CodeSearchResult> SandoResults, List<CodeSearchResult> SecondaryResults)
        {
            foreach (CodeSearchResult sResult in SandoResults)
            {
                sResult.Element.Name = "S: " + sResult.Element.Name;
            }
            foreach (CodeSearchResult lResult in SecondaryResults)
            {
                lResult.Element.Name = "L: " + lResult.Element.Name;
            }
        }

		//called from UI.FileOpener
		public void NotifyClicked(CodeSearchResult clickedElement)
		{
            if (InterleavedResults != null && InterleavedResults.Count > 0)
            {
                ClickIdx.Add(InterleavedResults.IndexOf(clickedElement));
                SearchRecievedClick = true;

				//record whether clicked element has a perfect lexical match for the query
                string name = clickedElement.Element.Name.ToLower();
                PartialTermMatchInClicked += (name.Contains(lastQuery)) ?"N" :".";
				string spacedLastQuery = " " + lastQuery.Trim().ToLower() + " ";
                string spacedName = " " + name + " ";
                ExactTermMatchInClicked += (spacedName.Contains(spacedLastQuery)) ?"N" :".";
                if (clickedElement.Element is MethodElement)
                {
                    string body = (clickedElement.Element as MethodElement).Body.ToLower();
                    PartialTermMatchInClicked += (body.Contains(lastQuery)) ? "B" : ":";
                    string spacedBody = " " + body + " ";
                    ExactTermMatchInClicked += (spacedBody.Contains(spacedLastQuery)) ? "B" : ".";
                }
                else
                {
                    PartialTermMatchInClicked += ".";
                    ExactTermMatchInClicked += ".";
                }

				//record the clicked element type
				if(clickedElement.Element is MethodElement)
				{
					ClickedElementTypes += "M";
				}
				else if(clickedElement.Element is ClassElement)
				{
					ClickedElementTypes += "C";
				}
				else if(clickedElement.Element is FieldElement || clickedElement.Element is PropertyElement)
				{
					ClickedElementTypes += "F";
				}
				else if(clickedElement.Element is CommentElement || clickedElement.Element is DocCommentElement)
				{
					ClickedElementTypes += "Co";
				}
				else
				{
					ClickedElementTypes += "O";
				}
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
			LogFile = Dir + "\\PI4a-" + machine + "-" + Guid.NewGuid() + ".log";
		}

		private const int LOG_ENTRIES_PER_FILE = 3;
		private const string FLT_A_NAME = "Sando";
        private const string FLT_B_NAME = "Lex";
		private string LogFile;
		private string PluginDirectory;
        private int NumRawSecondaryResults = 0;
        private string ExactTermMatchInClicked;
        private string PartialTermMatchInClicked;
		private string ClickedElementTypes = "";

		private string lastQuery = "?";
        private List<CodeSearchResult> SecondaryResults;
        private List<CodeSearchResult> SandoResults;
        private bool SearchRecievedClick;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
        public int LogCount { get; private set; }
	}
}
