using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using log4net;
using Sando.ExtensionContracts.ParserContracts;
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
			logger.Info("-=#|#=- Analyzing configuration finished -=#|#=-");
		}

		private static void RemoveInvalidConfigurations(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			if(extensionPointsConfiguration.ParserExtensionPointsConfiguration != null)
				RemoveInvalidParserConfigurations(extensionPointsConfiguration, logger);
			if(extensionPointsConfiguration.WordSplitterExtensionPointConfiguration != null)
				RemoveInvalidWordSplitterConfiguration(extensionPointsConfiguration, logger);
		}

		private static void RemoveInvalidParserConfigurations(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			int invalidParserConfigurationCount = extensionPointsConfiguration.ParserExtensionPointsConfiguration.RemoveAll(
				p =>
					String.IsNullOrWhiteSpace(p.FullClassName) ||
					String.IsNullOrWhiteSpace(p.LibraryFileRelativePath) ||
					p.SupportedFileExtensions == null ||
					p.SupportedFileExtensions.Count == 0);
			if(invalidParserConfigurationCount > 0)
				logger.Info(String.Format("{0} invalid parser configurations found - they will be omitted during registration process.", invalidParserConfigurationCount));
		}

		private static void RemoveInvalidWordSplitterConfiguration(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			BaseExtensionPointConfiguration wordSplitterConfiguration = extensionPointsConfiguration.WordSplitterExtensionPointConfiguration;
			if(String.IsNullOrWhiteSpace(wordSplitterConfiguration.FullClassName) ||
				String.IsNullOrWhiteSpace(wordSplitterConfiguration.LibraryFileRelativePath))
			{
				extensionPointsConfiguration.WordSplitterExtensionPointConfiguration = null;
				logger.Info(String.Format("Invalid word splitter configuration found - it will be omitted during registration process."));
			}
		}

		private static void FindAndRegisterValidParserExtensionPoints(ExtensionPointsConfiguration extensionPointsConfiguration, ILog logger)
		{
			logger.Info("Reading parser extension points configuration started");
			foreach(ParserExtensionPointsConfiguration parserConfiguration in extensionPointsConfiguration.ParserExtensionPointsConfiguration)
			{
				try
				{
					logger.Info(String.Format("Parser found: {0}, from assembly: {1}", parserConfiguration.FullClassName, parserConfiguration.LibraryFileRelativePath));
					string assemblyPath = Path.Combine(extensionPointsConfiguration.PluginDirectoryPath, parserConfiguration.LibraryFileRelativePath);
					Assembly parserAssembly = Assembly.LoadFile(assemblyPath);
					Type parserType = parserAssembly.GetType(parserConfiguration.FullClassName);
					IParser parser = (IParser)Activator.CreateInstance(parserType);
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
			BaseExtensionPointConfiguration wordSplitterConfiguration = extensionPointsConfiguration.WordSplitterExtensionPointConfiguration;
			if(extensionPointsConfiguration.WordSplitterExtensionPointConfiguration != null)
			{
				try
				{
					logger.Info(String.Format("Word splitter found: {0}, from assembly: {1}", wordSplitterConfiguration.FullClassName, wordSplitterConfiguration.LibraryFileRelativePath));
					string assemblyPath = Path.Combine(extensionPointsConfiguration.PluginDirectoryPath, wordSplitterConfiguration.LibraryFileRelativePath);
					Assembly parserAssembly = Assembly.LoadFile(assemblyPath);
					Type wordSplitterType = parserAssembly.GetType(wordSplitterConfiguration.FullClassName);
					IWordSplitter wordSplitter = (IWordSplitter)Activator.CreateInstance(wordSplitterType);
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
	}
}
