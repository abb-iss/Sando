using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Sando.ExtensionContracts;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Core.Extensions
{
	public class ExtensionPointsRepository
	{
		public IParser GetParserImplementation(string fileExtension)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fileExtension), "ExtensionPointsManager:GetParserImplementation - fileExtension cannot be null or an empty string!");

			if(parsers.ContainsKey(fileExtension))
				return parsers[fileExtension];
			else
				return null;
		}

		public void RegisterParserImplementation(List<string> supportedFileExtensions, IParser parserImplementation)
		{
			Contract.Requires(supportedFileExtensions != null, "ExtensionPointsManager:RegisterParserImplementation - supportedFileExtensions cannot be null!");
			Contract.Requires(supportedFileExtensions.Count > 0, "ExtensionPointsManager:RegisterParserImplementation - supportedFileExtensions must contain at least one item!");
			Contract.Requires(supportedFileExtensions.FindAll(sfe => String.IsNullOrWhiteSpace(sfe)).Count == 0, "ExtensionPointsManager:RegisterParserImplementation - supportedFileExtensions cannot contain empty items!");
			Contract.Requires(parserImplementation != null, "ExtensionPointsManager:RegisterParserImplementation - parserImplementation cannot be null!");
			
			foreach(string supportedFileExtension in supportedFileExtensions)
				parsers[supportedFileExtension] = parserImplementation;
		}

		public IWordSplitter GetWordSplitterImplementation()
		{
			return wordSplitter;
		}

		public void RegisterWordSplitterImplementation(IWordSplitter wordSplitter)
		{
			Contract.Requires(wordSplitter != null, "ExtensionPointsManager:RegisterWordSplitterImplementation - wordSplitter cannot be null!");
			
			this.wordSplitter = wordSplitter;
		}

		public IResultsReorderer GetResultsReordererImplementation()
		{
			return resultsReorderer;
		}

		public void RegisterResultsReordererImplementation(IResultsReorderer resultsReorderer)
		{
			Contract.Requires(resultsReorderer != null, "ExtensionPointsManager:RegisterResultsReordererImplementation - resultsReorderer cannot be null!");

			this.resultsReorderer = resultsReorderer;
		}

		public IQueryWeightsSupplier GetQueryWeightsSupplierImplementation()
		{
			return queryWeightsSupplier;
		}

		public void RegisterQueryWeightsSupplierImplementation(IQueryWeightsSupplier queryWeightsSupplier)
		{
			Contract.Requires(queryWeightsSupplier != null, "ExtensionPointsManager:RegisterQueryWeightsSupplierImplementation - queryWeightsSupplier cannot be null!");

			this.queryWeightsSupplier = queryWeightsSupplier;
		}

		public IQueryRewriter GetQueryRewriterImplementation()
		{
			return queryRewriter;
		}

		public void RegisterQueryRewriterImplementation(IQueryRewriter queryRewriter)
		{
			Contract.Requires(queryRewriter != null, "ExtensionPointsManager:RegisterQueryRewriterImplementation - queryRewriter cannot be null!");

			this.queryRewriter = queryRewriter;
		}

		public void ClearRepository()
		{
			parsers.Clear();
			wordSplitter = null;
			resultsReorderer = null;
			queryWeightsSupplier = null;
			queryRewriter = null;
		}

		public static ExtensionPointsRepository GetInstance(ExperimentFlow extensionSet = ExperimentFlow.A)
		{
			if (extensionSet == ExperimentFlow.A)
			{
				if(ExtensionManagerSetA == null)
				{
					ExtensionManagerSetA = new ExtensionPointsRepository();
					IsInterleavingExperimentOn = false;
				}
				return ExtensionManagerSetA;
			}
			else if (extensionSet == ExperimentFlow.B)
			{
				if(ExtensionManagerSetB == null)
				{
					ExtensionManagerSetB = new ExtensionPointsRepository();
				}
				return ExtensionManagerSetB;
			}
			return null;
		}

		public static void InitializeInterleavingExperiment()
		{
			ExtensionManagerSetB.parsers = ExtensionManagerSetA.parsers;
			ExtensionManagerSetB.queryRewriter = ExtensionManagerSetA.queryRewriter;
			ExtensionManagerSetB.queryWeightsSupplier = ExtensionManagerSetA.queryWeightsSupplier;
			ExtensionManagerSetB.resultsReorderer = ExtensionManagerSetA.resultsReorderer;
			ExtensionManagerSetB.wordSplitter = ExtensionManagerSetB.wordSplitter;

			IsInterleavingExperimentOn = true;
		}

		private ExtensionPointsRepository()
		{
			parsers = new Dictionary<string, IParser>();
		}

		public static bool IsInterleavingExperimentOn { get; private set; }

		public static ThreadLocal<ExperimentFlow> ExpFlow = new ThreadLocal<ExperimentFlow>(() =>
																	  (Thread.CurrentThread.ManagedThreadId % 2 == 0)
																		? ExperimentFlow.A
																		: ExperimentFlow.B);

		private static ExtensionPointsRepository ExtensionManagerSetA;
		private static ExtensionPointsRepository ExtensionManagerSetB;

		private Dictionary<string, IParser> parsers;
		private IWordSplitter wordSplitter;
		private IResultsReorderer resultsReorderer;
		private IQueryWeightsSupplier queryWeightsSupplier;
		private IQueryRewriter queryRewriter;
	}
}
