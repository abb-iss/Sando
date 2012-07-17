using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

		public static ExtensionPointsRepository Instance
		{
			get
			{
                if (extensionManager == null)
                {
                    extensionManager = new ExtensionPointsRepository();
                }
			    return extensionManager;
			}
		}

		private ExtensionPointsRepository()
		{
			parsers = new Dictionary<string, IParser>();
		}

		private static ExtensionPointsRepository extensionManager;

		private Dictionary<string, IParser> parsers;
		private IWordSplitter wordSplitter;
		private IResultsReorderer resultsReorderer;
		private IQueryWeightsSupplier queryWeightsSupplier;
		private IQueryRewriter queryRewriter;
	}
}
