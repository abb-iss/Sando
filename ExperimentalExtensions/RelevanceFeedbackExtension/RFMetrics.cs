using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.ExtensionContracts.ResultsReordererContracts;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.ExperimentalExtensions.RelevanceFeedbackExtension
{
	[ContractClass(typeof(ContractForIMetric))]
	public interface IMetric
	{
		 double runMetric(string query, CodeSearchResult result);
	}

	[ContractClassFor(typeof(IMetric))]
	public abstract class ContractForIMetric : IMetric{

		public double runMetric(string query, CodeSearchResult result)
		{
			Contract.Requires(query != String.Empty, "Metric constructor: cannot use an empty query to calculate metrics");
			Contract.Requires(result != null, "Metric constructor: cannot use a null code search result");
			Contract.Ensures(Contract.Result<double>() >= 0.0, "Metric constructor: metric return value out of range");
			Contract.Ensures(Contract.Result<double>() <= 1.0, "Metric constructor: metric return value out of range");
			return default(double);
		}
	}

	#region ProgramElementMetrics

	public class TopOfFile : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			//consider only the first 100 lines of code
			double lineNum = (result.Element.DefinitionLineNumber > 100) ? 100 : result.Element.DefinitionLineNumber;

			return (1 - (lineNum / 100));
		}
	}

	public class OriginalSandoRank : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			return 1 - (1 / (1 + result.Score));
		}
	}

	public class QueryFileNameCosine : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			string fileName = result.Element.FullFilePath;
			char[] delimiters = new char[] { '\\', ' ', ':', '.' };
			string[] fileNameParts = fileName.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			string[] queryParts = query.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			double cosine = MetricUtils.SentenceCosineSimilarity(fileNameParts, queryParts);
			return cosine;
		}		
	}

	public class QueryElementNameCosine : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			string elementName = result.Element.Name;

			//camel case splitting (from WordSplitter)
			elementName = Regex.Replace(elementName, @"([A-Z][a-z]+|[A-Z]+|[0-9]+)", "_$1").Replace(" _", "_");

			char[] delimiters = new char[] { '_', ' ' };
			string[] elementNameParts = elementName.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			string[] queryParts = query.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			double cosine = MetricUtils.SentenceCosineSimilarity(elementNameParts, queryParts);
			return cosine;
		}
	}

	public class QuerySnippetCosine : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			string elementName = result.Element.Snippet;
			char[] delimiters = new char[] { '_', ' ' };
			string[] elementNameParts = elementName.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			string[] queryParts = query.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			double cosine = MetricUtils.SentenceCosineSimilarity(elementNameParts, queryParts);
			return cosine;
		}
	}

	#endregion

	#region MethodElementMetrics

	public class Method_QueryArgsCosine : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			if(result.Element is MethodElement)
			{
				MethodElement methodElement = (MethodElement)result.Element;
				string argumentsStr = methodElement.Arguments;
				char[] delimiters = new char[] { '_', ' ', ',' };
				string[] argumentStrParts = argumentsStr.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				string[] queryParts = query.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				double cosine = MetricUtils.SentenceCosineSimilarity(argumentStrParts, queryParts);
				return cosine;
			}
			else
			{
				return 0.0;
			}
		}
	}

	public class Method_QueryClassNameCosine : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			if(result.Element is MethodElement)
			{
				MethodElement methodElement = (MethodElement)result.Element;
				string argumentsStr = methodElement.ClassName;
				char[] delimiters = new char[] { '_', ' ', ',' };
				string[] argumentStrParts = argumentsStr.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				string[] queryParts = query.ToLower().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				double cosine = MetricUtils.SentenceCosineSimilarity(argumentStrParts, queryParts);
				return cosine;
			}
			else
			{
				return 0.0;
			}
		}
	}
	 
	public class IsMethod : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			if(result.Element is MethodElement)
			{
				return 1.0;
			}
			else
			{
				return 0.0;
			}
		}
	}

	#endregion

	#region CommentElementMetrics

	public class IsComment : IMetric
	{
		public double runMetric(string query, CodeSearchResult result)
		{
			if(result.Element is CommentElement)
			{
				return 1.0;
			}
			else
			{
				return 0.0;
			}
		}
	}

	#endregion

	public static class MetricUtils
	{
		public static double SentenceCosineSimilarity(string[] sentOneParts, string[] sentTwoParts)
		{
			double dot;
			double magProduct;

			//create frequency of occurence vectors for the two sentences
			IEnumerable<string> wordUnion = sentOneParts.Union(sentTwoParts);
			int wordLength = wordUnion.Count();
			int[] occVectorSentOne = new int[wordUnion.Count()];
			int[] occVectorSentTwo = new int[wordUnion.Count()];
			for(int i = 0; i < wordLength; i++)
			{
				occVectorSentOne[i] = (sentOneParts.Contains(wordUnion.ElementAt(i))) ? 1 : 0;
				occVectorSentTwo[i] = (sentTwoParts.Contains(wordUnion.ElementAt(i))) ? 1 : 0;
			}

			//calc dot product of the two vectors
			dot = 0.0;
			for(int i = 0; i < wordLength; i++)
			{
				dot += occVectorSentOne[i] * occVectorSentTwo[i];
			}

			//calc product of vector magnitudes
			magProduct = 0.0;
			double v1mag = 0.0;
			double v2mag = 0.0;
			for(int i = 0; i < wordLength; i++)
			{
				v1mag += occVectorSentOne[i] * occVectorSentOne[i];
			}
			v1mag = Math.Sqrt(v1mag);
			for(int i = 0; i < wordLength; i++)
			{
				v2mag += occVectorSentTwo[i] * occVectorSentTwo[i];
			}
			v2mag = Math.Sqrt(v2mag);
			magProduct = v1mag * v2mag;

			return (dot / magProduct);
		}
	}

}
