using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
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
					ExtensionPointsRepository.Instance.RegisterParserImplementation(parserConfiguration.SupportedFileExtensions, parser);
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
					ExtensionPointsRepository.Instance.RegisterWordSplitterImplementation(wordSplitter);
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
					ExtensionPointsRepository.Instance.RegisterResultsReordererImplementation(resultsReorderer);
					logger.Info(String.Format("Results reorderer {0} successfully registered.", resultsReordererConfiguration.FullClassName));
				}
				catch(Exception ex)
				{
					logger.Error(String.Format("Results reorderer {0} cannot be registered: {1}", resultsReordererConfiguration.FullClassName, ex.Message));
				}
			}
			logger.Info("Reading results reorderer extension point configuration finished");
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
