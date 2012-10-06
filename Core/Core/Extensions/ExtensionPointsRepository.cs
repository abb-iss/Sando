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

			if(currentExtensionSet.parsers.ContainsKey(fileExtension))
				return currentExtensionSet.parsers[fileExtension];
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
				currentExtensionSet.parsers[supportedFileExtension] = parserImplementation;
		}

		public IWordSplitter GetWordSplitterImplementation()
		{
			return currentExtensionSet.wordSplitter;
		}

		public void RegisterWordSplitterImplementation(IWordSplitter wordSplitter)
		{
			Contract.Requires(wordSplitter != null, "ExtensionPointsManager:RegisterWordSplitterImplementation - wordSplitter cannot be null!");
			
			currentExtensionSet.wordSplitter = wordSplitter;
		}

		public IResultsReorderer GetResultsReordererImplementation()
		{
			return currentExtensionSet.resultsReorderer;
		}

		public void RegisterResultsReordererImplementation(IResultsReorderer resultsReorderer)
		{
			Contract.Requires(resultsReorderer != null, "ExtensionPointsManager:RegisterResultsReordererImplementation - resultsReorderer cannot be null!");

			currentExtensionSet.resultsReorderer = resultsReorderer;
		}

		public IQueryWeightsSupplier GetQueryWeightsSupplierImplementation()
		{
			return currentExtensionSet.queryWeightsSupplier;
		}

		public void RegisterQueryWeightsSupplierImplementation(IQueryWeightsSupplier queryWeightsSupplier)
		{
			Contract.Requires(queryWeightsSupplier != null, "ExtensionPointsManager:RegisterQueryWeightsSupplierImplementation - queryWeightsSupplier cannot be null!");

			currentExtensionSet.queryWeightsSupplier = queryWeightsSupplier;
		}

		public IQueryRewriter GetQueryRewriterImplementation()
		{
			return currentExtensionSet.queryRewriter;
		}

		public void RegisterQueryRewriterImplementation(IQueryRewriter queryRewriter)
		{
			Contract.Requires(queryRewriter != null, "ExtensionPointsManager:RegisterQueryRewriterImplementation - queryRewriter cannot be null!");

			currentExtensionSet.queryRewriter = queryRewriter;
		}

		public void SwitchToClonedSet()
		{
            if (!IsCloned) 
                CloneExtensionSet();
            currentExtensionSet = clonedExtensionSet;
		}

        private void CloneExtensionSet()
        {
            clonedExtensionSet = originalExtensionSet.Clone();
            IsCloned = true;
        }

        public void SwitchToOriginalSet()
        {
            currentExtensionSet = originalExtensionSet;
        }

		public void ClearRepository()
		{
            originalExtensionSet.ClearSet();
            if(IsCloned)
                clonedExtensionSet.ClearSet();
            currentExtensionSet = null;
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
			originalExtensionSet = new ExtensionPointsSet();
            currentExtensionSet = originalExtensionSet;
            IsCloned = false;
		}

        public bool IsCloned { get; private set; }

		private static ExtensionPointsRepository extensionManager;

		private ExtensionPointsSet currentExtensionSet;

        private ExtensionPointsSet originalExtensionSet;
        private ExtensionPointsSet clonedExtensionSet;
	}
}
