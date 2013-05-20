using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.Indexer.Metrics
{
	public class PostRetrievalMetrics
	{
		public static double StdDevScore(List<CodeSearchResult> results)
		{
			double avgScores = AvgScore(results);
			double var = 0.0;
			foreach(var result in results)
			{
				var += Math.Pow(result.Score - avgScores, 2.0);
			}
			return Math.Sqrt(var / results.Count);
		}

		public static double AvgScore(List<CodeSearchResult> results)
		{
			double sum = 0.0;
			foreach(var result in results)
			{
				sum += result.Score;
			}
			return sum / results.Count;
		}

	}
}
