using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.Diagnostics.Contracts;

namespace Sando.ExperimentalExtensions.RelevanceFeedbackExtension
{
	public class RFTrainingEntry
	{
		public RFTrainingEntry(int id, string query, List<CodeSearchResult> sandoResults, List<IMetric> metrics)
		{
			Id = id;
			Query = query;
			SandoResults = sandoResults;
			Metrics = metrics;
			IndexClickedResults = new List<int>();
		}

		public void AddRelevance(CodeSearchResult clicked)
		{
			var tagged = SandoResults.Select((item, i) => new { Item = item, Index = (int?)i });
			int? index = (from pair in tagged
						  where pair.Item == clicked
						  select pair.Index).FirstOrDefault();

			if(index.HasValue && (! IndexClickedResults.Contains(index.Value)))
			{
				IndexClickedResults.Add(index.Value);
			}
		}

		public string SerializeInClickedOrder()
		{
			string strTop = String.Empty;
			string strBottom = String.Empty;

			for(int i = 0; i <= IndexClickedResults.Max(); i++)
			{
				if(IndexClickedResults.Contains(i))
				{
					strTop += "2 ";
					strTop += SerializeCodeSearchResult(SandoResults.ElementAt(i));
				}
				else
				{
					strBottom += "1 ";
					strBottom += SerializeCodeSearchResult(SandoResults.ElementAt(i));
				}
			}

			return "#query " + Id + Environment.NewLine + strTop + strBottom;
		}

		public string SerializeInSandoOrder()
		{
			string str = String.Empty;

			for(int i = 0; i < SandoResults.Count(); i++)
			{
				str += i + " ";
				str += SerializeCodeSearchResult(SandoResults.ElementAt(i));
			}

			return str;
		}

		public bool IsComplete()
		{
			return (IndexClickedResults.Count > 0) && (IndexClickedResults.Count() < (IndexClickedResults.Max() + 1));
		}	

		private string SerializeCodeSearchResult(CodeSearchResult searchResult)
		{
			string str = "qid:" + Id;

			int metricNum = 1;
			foreach(IMetric metric in Metrics)
			{
				str += " " + metricNum + ":" + Math.Round(metric.runMetric(Query, searchResult), 2);
				metricNum++;
			}

			str += Environment.NewLine;
			return str;
		}

		public int Id { get; private set; }
		public string Query { get; private set; }
		public List<CodeSearchResult> SandoResults { get; private set; }
		private List<int> IndexClickedResults;
		private List<IMetric> Metrics;
	}
}
