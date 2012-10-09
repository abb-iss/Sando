using System;
using System.Collections.Generic;
using System.Linq;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.InterleavingExperiment.Logging;
using System.Diagnostics.Contracts;


namespace Sando.UI.InterleavingExperiment
{
	public class InterleavingExperimentManager 
	{
        public IQueryable<CodeSearchResult> InterleaveResults(IQueryable<CodeSearchResult> AResults, IQueryable<CodeSearchResult> BResults)
		{
            Contract.Requires(IsInitialized == true, "Interleaving experiment was started but was not initialized");

            WriteExpRoundToFile();
            WriteLogToS3();

            ClickIdx.Clear();
            SearchRecievedClick = false;

            fltA.Results = AResults.ToList();
            fltB.Results = BResults.ToList();
            InterleavedResults = BalancedInterleaving.Interleave(fltA.Results, fltB.Results);
            return InterleavedResults.AsQueryable(); 
        }

		//called from UI.FileOpener
		public void NotifyClicked(CodeSearchResult clickedElement)
		{
            Contract.Requires(IsInitialized == true, "Interleaving experiment was started but was not initialized");

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
					BalancedInterleaving.DetermineWinner(fltA.Results, fltB.Results, InterleavedResults,
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

            fltA = new FeatureLocationTechnique("Sando");
            fltA = new FeatureLocationTechnique("Samurai");

			ExtensionPointsRepository.InitializeInterleavingExperiment();
            //ExtensionPointsRepository.GetInstance(ExperimentFlow.B).RegisterSplitterImplementation(?);

            IsInitialized = true;
        }


        public static InterleavingExperimentManager Instance
		{
			get
			{
                if (interleavingMgr == null)
                {
                    interleavingMgr = new InterleavingExperimentManager();
                }
			    return interleavingMgr;
			}
		}

        private InterleavingExperimentManager()
		{
			ClickIdx = new List<int>();
            IsInitialized = false;
		}

        private static InterleavingExperimentManager interleavingMgr;

		private string PluginDirectory;
		private string LogFile;
		private int LogEntriesPerFile = 15;
		private int LogCount = 0;

        public bool IsInitialized { get; private set; }

        private FeatureLocationTechnique fltA;
        private FeatureLocationTechnique fltB;
		private bool SearchRecievedClick = false;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
	}
}
