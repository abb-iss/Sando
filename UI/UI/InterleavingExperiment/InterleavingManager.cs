using System;
using System.Collections.Generic;
using System.Linq;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;
using Sando.UI.InterleavingExperiment.FLTs;
using Sando.UI.InterleavingExperiment.Logging;
using Sando.UI.InterleavingExperiment.Multiplexing;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;

namespace Sando.UI.InterleavingExperiment
{
	public class InterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public string RewriteQuery(string query)
		{
            WriteExpRoundToFile();
            WriteLogToS3();

			fltA.IssueQuery(query);
			fltB.IssueQuery(query);

			ClickIdx.Clear();
			SearchRecievedClick = false;
			
            return query;
		}

		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
            InterleavedResults = BalancedInterleaving.Interleave(fltA.GetResults(), fltB.GetResults());
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

		private void WriteExpRoundToFile()
		{
			if(SearchRecievedClick)
			{
				try
				{
					LogCount++;
					int scoreA, scoreB;
					BalancedInterleaving.DetermineWinner(fltA.GetResults(), fltB.GetResults(), InterleavedResults,
														 ClickIdx, out scoreA, out scoreB);
					string entry = LogCount + ": " + fltA.Name + "=" + scoreA + ", " +
								   fltB.Name + "=" + scoreB + Environment.NewLine;
					System.IO.File.AppendAllText(LogFile, entry);
				}
				catch(Exception e)
				{
					FileLogger.DefaultLogger.Error(e.StackTrace);
				}
			}
		}

		private void WriteLogToS3()
		{
			if (LogCount < LogEntriesPerFile) return;
			var success = S3LogWriter.WriteLogFile(LogFile);
			if(success == true)
			{
				System.IO.File.Delete(LogFile);
				InitializeNewLogFileName(PluginDirectory);
				LogCount = 0;
			}
			else
			{
				//try again after 10 more log entries
				LogEntriesPerFile += 10; 
			}
		}

		private void InitializeNewLogFileName(string logDir)
		{
			LogFile = logDir + "\\PairedInterleaving-" + Environment.MachineName + "-" + Guid.NewGuid() + ".log";
		}


        public void InitializeExperimentParticipants(string pluginDir)
        {
            S3LogWriter.S3CredentialDirectory = pluginDir;
            InitializeNewLogFileName(pluginDir);
            PluginDirectory = pluginDir;

            fltA = new SandoFLT();
            fltB = new SamuraiSplitterFLT();

            ExtensionPointsRepository.Instance.CloneExtensionSet();
            ExtensionPointsRepository.Instance.SwitchExtensionSet();

            //TODO:
            //ExtensionPointsRepository.Instance.RegisterParserImplementation(?);
            //ExtensionPointsRepository.Instance.RegisterSplitterImplementation(?);
            ExtensionPointsRepository.Instance.SwitchExtensionSet();
        }


        public static InterleavingManager Instance
		{
			get
			{
                if (interleavingMgr == null)
                {
                    interleavingMgr = new InterleavingManager();
                }
			    return interleavingMgr;
			}
		}

        private InterleavingManager()
		{
			ClickIdx = new List<int>();
		}

        private static InterleavingManager interleavingMgr;

		private string PluginDirectory;
		private string LogFile;
		private int LogEntriesPerFile = 15;
		private int LogCount = 0;

		private FeatureLocationTechnique fltA;
		private FeatureLocationTechnique fltB;
		private bool SearchRecievedClick = false;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
	}
}
