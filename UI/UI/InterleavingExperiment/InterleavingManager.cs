using System;
using System.Collections.Generic;
using System.Linq;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;
using Sando.UI.InterleavingExperiment.FLTs;

namespace Sando.UI.InterleavingExperiment
{
	public class InterleavingManager : IQueryRewriter, IResultsReorderer
	{
		public InterleavingManager(string pluginDir, ExtensionPointsRepository extensionPointsRepository)
		{
			ClickIdx = new List<int>();

			ExtPointsRepository = extensionPointsRepository;
			S3LogWriter.S3CredentialDirectory = pluginDir;
			InitializeNewLogFileName(pluginDir);
			PluginDirectory = pluginDir;

			InitializeExperimentParticipants();
		}

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


		private void InitializeExperimentParticipants()
		{
			fltA = new SandoFLT();
			fltB = new SamuraiFLT(); 

			//*register any additional extension points here
			ExtPointsRepository.RegisterParserImplementation(new List<string>() {".cs"}, (IParser)fltB);
			ExtPointsRepository.RegisterWordSplitterImplementation((IWordSplitter)fltB);
		}

		private void InitializeNewLogFileName(string logDir)
		{
			LogFile = logDir + "\\PairedInterleaving-" + Environment.MachineName + "-" + Guid.NewGuid() + ".log";
		}

		private readonly string PluginDirectory;

		private string LogFile;
		private int LogEntriesPerFile = 15;
		private int LogCount = 0;

		private FeatureLocationTechnique fltA;
		private FeatureLocationTechnique fltB;
		private bool SearchRecievedClick = false;
		private ExtensionPointsRepository ExtPointsRepository;

        public List<CodeSearchResult> InterleavedResults { get; private set; }
        public List<int> ClickIdx { get; private set; }
	}
}
