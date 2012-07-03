using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ProgramElementContracts;
using System.Diagnostics;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.QueryContracts;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Sando.ExperimentalExtensions.RelevanceFeedbackExtension
{
	public enum RFMode
	{
		Training,
		Operating,
		Evaluating,
		Off
	}

	public class RelevanceFeedbackExtension : IQueryRewriter, IResultsReorderer
	{
		//TODO: this is not the directory I expected (sando part should already have been there)
		public readonly string StandardSvmRankLocation = Environment.CurrentDirectory + "\\..\\..\\LIBS\\svm_rank_windows";

		public static readonly int DEFAULT_TRAINING_CORPUS_SIZE = 10000;

		//TODO: figure out how to make them per solution
		public readonly string TrainingFile = Environment.CurrentDirectory + "\\trainRF.dat";
		public readonly string ModelFile = Environment.CurrentDirectory + "\\modelRF.dat";
		public readonly string InputRankingFile = Environment.CurrentDirectory + "\\inputRF.dat";
		public readonly string OutputRankingFile = Environment.CurrentDirectory + "\\outputRF.dat"; 

		public RFMode OpMode { get; private set; }

		public readonly int TrainingSetSize;
		private int TrainingCount;

		private string RecQuery;
		private RFTrainingEntry CurrentTrainingEntry;
		private RFRankGenerator RankGenerator;
		private List<IMetric> Metrics;

		public RelevanceFeedbackExtension()
			: this(DEFAULT_TRAINING_CORPUS_SIZE)  
		{
		}

		public RelevanceFeedbackExtension(int trainingSetSize)
		{
			Contract.Requires(trainingSetSize > 0, "Relevance feedback needs a training set size that is greater than 0.");

			OpMode = RFMode.Training;
			TrainingSetSize = trainingSetSize;
			TrainingCount = 0;
			RecQuery = String.Empty;
			CurrentTrainingEntry = null;
			RankGenerator = new RFRankGenerator(StandardSvmRankLocation);

			Metrics = new List<IMetric>();
			Metrics.Add(new QueryFileNameCosine());
			Metrics.Add(new QueryElementNameCosine());
			Metrics.Add(new QuerySnippetCosine());
			Metrics.Add(new OriginalSandoRank());
			Metrics.Add(new TopOfFile());
			Metrics.Add(new Method_QueryArgsCosine());
			Metrics.Add(new Method_QueryClassNameCosine());
			Metrics.Add(new IsMethod());
			Metrics.Add(new IsComment());

			ReadNumberOfTrainingEntries(TrainingFile);
		}

		public string RewriteQuery(string query)
		{
			RecQuery = query;
			return query;
		}

		//this method is the driver for this extension
		public IQueryable<CodeSearchResult> ReorderSearchResults(IQueryable<CodeSearchResult> searchResults)
		{
			if(OpMode == RFMode.Training)
			{
				//serialize previous training entry (if it got clicked) to training file
				if(CurrentTrainingEntry != null && CurrentTrainingEntry.IsComplete())
				{
					WriteTrainingEntry(TrainingFile, CurrentTrainingEntry);
				}

				if(TrainingCount >= TrainingSetSize)
				{
					//build model and change op mode
					//TODO: this could take a while, so use a background thread
					RankGenerator.GenerateModel(TrainingFile, ModelFile);

					//training done, switch mode
					OpMode = RFMode.Operating;
				}
				else 
				{
					//generating an entry that will be serialized the next this this method is invoked
					TrainingCount++;
					CurrentTrainingEntry = new RFTrainingEntry(TrainingCount, RecQuery, searchResults.ToList(), Metrics);
				}
			}
			
			if(OpMode == RFMode.Operating || OpMode == RFMode.Evaluating)
			{
				WriteRankingEntry(InputRankingFile, new RFTrainingEntry(1, RecQuery, searchResults.ToList(), Metrics));
				RankGenerator.GenerateRanking(InputRankingFile, ModelFile, OutputRankingFile);
				var rerankedResults = RerankResults(searchResults, OutputRankingFile);

				if(OpMode == RFMode.Evaluating)
				{
					//call InterleavedEvaluation.GetInterleaving(searchResults,rerankedResults)
				}

				return rerankedResults;
			}
			return searchResults;
		}

		//called from UI.FileOpener
		public void NotifySelection(CodeSearchResult clickedElement)
		{
			if(OpMode == RFMode.Training)
			{
				if(CurrentTrainingEntry != null)
				{
					CurrentTrainingEntry.AddRelevance(clickedElement);
				}
			}
			else if(OpMode == RFMode.Evaluating)
			{
				//call InterleavedEvaluation.NotifySelection
			}
		}

		public IQueryable<CodeSearchResult> RerankResults(IQueryable<CodeSearchResult> inputResults, string rankFile)
		{
			string rankFileText = System.IO.File.ReadAllText(rankFile);
			char[] delimiters = new char[] { '\n', '\r', ' ' };
			string[] rankStrings = rankFileText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			IEnumerable<double> ranks = rankStrings.Select(s => Double.Parse(s));
			var taggedResults = inputResults.Select((item, i) => new { Item = item, Index = i });
			taggedResults = taggedResults.OrderByDescending(tr => (tr.Index <= ranks.Count()) ? ranks.ElementAt(tr.Index) : 0);
			var rankedResults = taggedResults.Select(tr => tr.Item); 
			return rankedResults;
		}

		private void WriteTrainingEntry(string trainingDataFile, RFTrainingEntry entry)
		{
			System.IO.File.AppendAllText(trainingDataFile, entry.SerializeInClickedOrder());
		}

		private void WriteRankingEntry(string rankingDataFile, RFTrainingEntry entry)
		{
			System.IO.File.WriteAllText(rankingDataFile, entry.SerializeInSandoOrder());
		}

		private int ReadNumberOfTrainingEntries(string trainingDataFile)
		{
			int bufferSize = 1000; //number of bytes to read from the end of the file
			int entryNum = 0;

			System.IO.FileStream fs = new System.IO.FileStream(trainingDataFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
			if(fs == null)
			{
				return 0;
			}

			if(bufferSize < fs.Length)
			{
				fs.Seek(-bufferSize, System.IO.SeekOrigin.End);
			}
			else
			{
				bufferSize = (int)fs.Length;
				fs.Seek(0, System.IO.SeekOrigin.Begin);
			}

			byte[] buffer = new byte[bufferSize];
			fs.Read(buffer, 0, bufferSize);
			String strBuf = System.Text.Encoding.Default.GetString(buffer);
			MatchCollection matches = Regex.Matches(strBuf, @"#query [0-9]+");
			foreach(Match match in matches)
			{
				String number = match.Value.Split().ElementAt(1);
				int num = Int32.Parse(number);
				if(num > entryNum) entryNum = num;
			}

			return entryNum;
		}

	}
}
