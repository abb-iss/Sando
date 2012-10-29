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
using System.IO;


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
                WriteAllIncompleteLogs();
			}
			else
			{
				//try again after 5 more log entries
				LogEntriesPerFile += 5; 
			}
		}

         
        private void WriteAllIncompleteLogs()
        {
            string[] files = Directory.GetFiles(PluginDirectory);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                string fileName = Path.GetFileName(fullPath);

                if (fileName.StartsWith("PI-") && fileName.EndsWith(".log"))
                {
                    bool success = S3LogWriter.WriteLogFile(fullPath);
                    if (success == true)
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
        }

		private void InitializeNewLogFileName(string logDir)
		{
			LogFile = logDir + "\\PI-" + Environment.MachineName + "-" + Guid.NewGuid() + ".log";
		}


        public void InitializeExperimentParticipants(string pluginDir)
        {
            S3LogWriter.S3CredentialDirectory = pluginDir;
            InitializeNewLogFileName(pluginDir);
            PluginDirectory = pluginDir;

            fltA = new FeatureLocationTechnique("Sando");
            fltB = new NoWeightsFLT("SandoNoWeights");

			ExtensionPointsRepository.InitializeInterleavingExperiment();

            //flow B will use no weights
            ExtensionPointsRepository.GetInstance(ExperimentFlow.B).RegisterQueryWeightsSupplierImplementation(fltB as IQueryWeightsSupplier);

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
        private bool SearchRecievedClick = false;

        public bool IsInitialized { get; private set; }
        public FeatureLocationTechnique fltA { get; private set; }
        public FeatureLocationTechnique fltB { get; private set; }
        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
	}
}
