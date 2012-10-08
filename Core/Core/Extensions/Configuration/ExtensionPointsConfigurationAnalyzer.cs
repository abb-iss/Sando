using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;

namespace Sando.Core.Extensions.Configuration
{
	public static class ExtensionPointsConfigurationAnalyzer
	{
		public static void FindAndRegisterValidExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			Contract.Requires(logger != null, "ExtensionPointsConfigurationReader:FindAndRegisterValidExtensionPoints - logger cannot be null!");

			logger.Info("-=#|#=- Analyzing configuration started -=#|#=-");
			RemoveInvalidConfigurations(extensionPointsConfiguration, logger);
			FindAndRegisterValidParserExtensionPoints(extensionPointsConfiguration, logger);
			FindAndRegisterValidWordSplitterExtensionPoints(extensionPointsConfiguration, logger);
			FindAndRegisterValidResultsReordererExtensionPoints(extensionPointsConfiguration, logger);
			FindAndRegisterValidQueryWeightsSupplierExtensionPoints(extensionPointsConfiguration, logger);
			FindAndRegisterValidQueryRewriterExtensionPoints(extensionPointsConfiguration, logger);
			logger.Info("-=#|#=- Analyzing configuration finished -=#|#=-");
		}

		private static void RemoveInvalidConfigurations(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			if(extensionPointsConfiguration.ParsersConfiguration != null)
				RemoveInvalidParserConfigurations(extensionPointsConfiguration, logger);
			if(extensionPointsConfiguration.WordSplitterConfiguration != null)
				RemoveInvalidWordSplitterConfiguration(extensionPointsConfiguration, logger);
			if(extensionPointsConfiguration.ResultsReordererConfiguration != null)
				RemoveInvalidResultsReordererConfiguration(extensionPointsConfiguration, logger);
			if(extensionPointsConfiguration.QueryWeightsSupplierConfiguration != null)
				RemoveInvalidQueryWeightsSupplierConfiguration(extensionPointsConfiguration, logger);
			if(extensionPointsConfiguration.QueryRewriterConfiguration != null)
				RemoveInvalidQueryRewriterConfiguration(extensionPointsConfiguration, logger);
		}

		private static void RemoveInvalidParserConfigurations(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			int invalidParserConfigurationCount = extensionPointsConfiguration.ParsersConfiguration.RemoveAll(
				p =>
					IsConfigurationInvalid(p) ||
					p.SupportedFileExtensions == null ||
					p.SupportedFileExtensions.Count == 0 ||
					p.ProgramElementsConfiguration.Count(pe => IsConfigurationInvalid(pe)) > 0);
			if(invalidParserConfigurationCount > 0)
				logger.Info(String.Format("{0} invalid parser configurations found - they will be omitted during registration process.", invalidParserConfigurationCount));
		}

		private static void RemoveInvalidWordSplitterConfiguration(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			if(IsConfigurationInvalid(extensionPointsConfiguration.WordSplitterConfiguration))
			{
				extensionPointsConfiguration.WordSplitterConfiguration = null;
				logger.Info(String.Format("Invalid word splitter configuration found - it will be omitted during registration process."));
			}
		}

		private static void RemoveInvalidResultsReordererConfiguration(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			if(IsConfigurationInvalid(extensionPointsConfiguration.ResultsReordererConfiguration))
			{
				extensionPointsConfiguration.ResultsReordererConfiguration = null;
				logger.Info(String.Format("Invalid results reorderer configuration found - it will be omitted during registration process."));
			}
		}

		private static void RemoveInvalidQueryWeightsSupplierConfiguration(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			if(IsConfigurationInvalid(extensionPointsConfiguration.QueryWeightsSupplierConfiguration))
			{
				extensionPointsConfiguration.QueryWeightsSupplierConfiguration = null;
				logger.Info(String.Format("Invalid query weights supplier configuration found - it will be omitted during registration process."));
			}
		}

		private static void RemoveInvalidQueryRewriterConfiguration(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			if(IsConfigurationInvalid(extensionPointsConfiguration.QueryRewriterConfiguration))
			{
				extensionPointsConfiguration.QueryRewriterConfiguration = null;
				logger.Info(String.Format("Invalid query rewriter configuration found - it will be omitted during registration process."));
			}
		}

		private static bool IsConfigurationInvalid(BaseExtensionPointConfiguration configuration)
		{
			return String.IsNullOrWhiteSpace(configuration.FullClassName) || String.IsNullOrWhiteSpace(configuration.LibraryFileRelativePath);
		}

		private static void FindAndRegisterValidParserExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			logger.Info("Reading parser extension points configuration started");
			foreach(ParserExtensionPointsConfiguration parserConfiguration in extensionPointsConfiguration.ParsersConfiguration)
			{
				try
				{
					logger.Info(String.Format("Parser found: {0}, from assembly: {1}", parserConfiguration.FullClassName, parserConfiguration.LibraryFileRelativePath));
					IParser parser = CreateInstance<IParser>(extensionPointsConfiguration.PluginDirectoryPath, parserConfiguration.LibraryFileRelativePath, parserConfiguration.FullClassName);
					parserConfiguration.ProgramElementsConfiguration.ForEach(pe => LoadAssembly(extensionPointsConfiguration.PluginDirectoryPath, pe.LibraryFileRelativePath));
					ExtensionPointsRepository.GetInstance().RegisterParserImplementation(parserConfiguration.SupportedFileExtensions, parser);
					logger.Info(String.Format("Parser {0} successfully registered.", parserConfiguration.FullClassName));
				}
				catch(Exception ex)
				{
					logger.Error(String.Format("Parser {0} cannot be registered: {1}", parserConfiguration.FullClassName, ex.Message));
				}
			}
			logger.Info("Reading parser extension points configuration finished");
		}

		private static void FindAndRegisterValidWordSplitterExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			logger.Info("Reading word splitter extension point configuration started");
			BaseExtensionPointConfiguration wordSplitterConfiguration = extensionPointsConfiguration.WordSplitterConfiguration;
			if(wordSplitterConfiguration != null)
			{
				try
				{
					logger.Info(String.Format("Word splitter found: {0}, from assembly: {1}", wordSplitterConfiguration.FullClassName, wordSplitterConfiguration.LibraryFileRelativePath));
					IWordSplitter wordSplitter = CreateInstance<IWordSplitter>(extensionPointsConfiguration.PluginDirectoryPath, wordSplitterConfiguration.LibraryFileRelativePath, wordSplitterConfiguration.FullClassName);
					ExtensionPointsRepository.GetInstance().RegisterWordSplitterImplementation(wordSplitter);
					logger.Info(String.Format("Word splitter {0} successfully registered.", wordSplitterConfiguration.FullClassName));
				}
				catch(Exception ex)
				{
					logger.Error(String.Format("Word splitter {0} cannot be registered: {1}", wordSplitterConfiguration.FullClassName, ex.Message));
				}
			}
			logger.Info("Reading word splitter extension point configuration finished");
		}

		private static void FindAndRegisterValidResultsReordererExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			logger.Info("Reading results reorderer extension point configuration started");
			BaseExtensionPointConfiguration resultsReordererConfiguration = extensionPointsConfiguration.ResultsReordererConfiguration;
			if(resultsReordererConfiguration != null)
			{
				try
				{
					logger.Info(String.Format("Results reorderer found: {0}, from assembly: {1}", resultsReordererConfiguration.FullClassName, resultsReordererConfiguration.LibraryFileRelativePath));
					IResultsReorderer resultsReorderer = CreateInstance<IResultsReorderer>(extensionPointsConfiguration.PluginDirectoryPath, resultsReordererConfiguration.LibraryFileRelativePath, resultsReordererConfiguration.FullClassName);
					ExtensionPointsRepository.GetInstance().RegisterResultsReordererImplementation(resultsReorderer);
					logger.Info(String.Format("Results reorderer {0} successfully registered.", resultsReordererConfiguration.FullClassName));
				}
				catch(Exception ex)
				{
					logger.Error(String.Format("Results reorderer {0} cannot be registered: {1}", resultsReordererConfiguration.FullClassName, ex.Message));
				}
			}
			logger.Info("Reading results reorderer extension point configuration finished");
		}

		private static void FindAndRegisterValidQueryWeightsSupplierExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			logger.Info("Reading query weights supplier extension point configuration started");
			BaseExtensionPointConfiguration queryWeightsSupplierConfiguration = extensionPointsConfiguration.QueryWeightsSupplierConfiguration;
			if(queryWeightsSupplierConfiguration != null)
			{
				try
				{
					logger.Info(String.Format("Query weights supplier found: {0}, from assembly: {1}", queryWeightsSupplierConfiguration.FullClassName, queryWeightsSupplierConfiguration.LibraryFileRelativePath));
					IQueryWeightsSupplier queryWeightsSupplier = CreateInstance<IQueryWeightsSupplier>(extensionPointsConfiguration.PluginDirectoryPath, queryWeightsSupplierConfiguration.LibraryFileRelativePath, queryWeightsSupplierConfiguration.FullClassName);
					ExtensionPointsRepository.GetInstance().RegisterQueryWeightsSupplierImplementation(queryWeightsSupplier);
					logger.Info(String.Format("Query weights supplier {0} successfully registered.", queryWeightsSupplierConfiguration.FullClassName));
				}
				catch(Exception ex)
				{
					logger.Error(String.Format("Query weights supplier {0} cannot be registered: {1}", queryWeightsSupplierConfiguration.FullClassName, ex.Message));
				}
			}
			logger.Info("Reading query weights supplier extension point configuration finished");
		}

		private static void FindAndRegisterValidQueryRewriterExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			logger.Info("Reading query rewriter extension point configuration started");
			BaseExtensionPointConfiguration queryRewriterConfiguration = extensionPointsConfiguration.QueryRewriterConfiguration;
			if(queryRewriterConfiguration != null)
			{
				try
				{
					logger.Info(String.Format("Query rewriter found: {0}, from assembly: {1}", queryRewriterConfiguration.FullClassName, queryRewriterConfiguration.LibraryFileRelativePath));
					IQueryRewriter queryRewriter = CreateInstance<IQueryRewriter>(extensionPointsConfiguration.PluginDirectoryPath, queryRewriterConfiguration.LibraryFileRelativePath, queryRewriterConfiguration.FullClassName);
					ExtensionPointsRepository.GetInstance().RegisterQueryRewriterImplementation(queryRewriter);
					logger.Info(String.Format("Query rewriter {0} successfully registered.", queryRewriterConfiguration.FullClassName));
				}
				catch(Exception ex)
				{
					logger.Error(String.Format("Query rewriter {0} cannot be registered: {1}", queryRewriterConfiguration.FullClassName, ex.Message));
				}
			}
			logger.Info("Reading query rewriter extension point configuration finished");
		}

		private static Assembly LoadAssembly(string pluginDirectoryPath, string libraryFileRelativePath)
		{
			string assemblyPath = Path.Combine(pluginDirectoryPath, libraryFileRelativePath);
			return Assembly.LoadFrom(assemblyPath);
		}

		private static T CreateInstance<T>(string pluginDirectoryPath, string libraryFileRelativePath, string fullClassName)
		{
			Assembly assembly = LoadAssembly(pluginDirectoryPath, libraryFileRelativePath);
			Type type = assembly.GetType(fullClassName);
			if(type == null)
				throw new ArgumentNullException(String.Format("Type cannot be found: {0}", fullClassName));
			return (T)Activator.CreateInstance(type);
		}
	}
}
