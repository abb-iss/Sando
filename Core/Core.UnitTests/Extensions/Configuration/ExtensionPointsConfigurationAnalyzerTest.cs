using log4net;
using NUnit.Framework;
using Sando.Core.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Logging;

namespace Sando.Core.UnitTests.Extensions.Configuration
{
	[TestFixture]
	public class ExtensionPointsConfigurationAnalyzerTest
	{
		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomParsers()
		{
			CreateExtensionPointsConfiguration(true, false, false, false);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cs"), "Parser for '.cs' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".h"), "Parser for '.h' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cpp"), "Parser for '.cpp' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cxx"), "Parser for '.cxx' extension should be registered!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomParserConfigurations()
		{
			CreateExtensionPointsConfiguration(true, true, false, false);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cs"), "Parser for '.cs' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".h"), "Parser for '.h' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cpp"), "Parser for '.cpp' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cxx"), "Parser for '.cxx' extension should be registered!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("3 invalid parser configurations found - they will be omitted during registration process."), "Log file should contain information about removed invalid parser configurations!");
		}
		
		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomWordSplitter()
		{
			CreateExtensionPointsConfiguration(false, false, true, false);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetWordSplitterImplementation(), "Word splitter should be registered!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomWordSplitterConfiguration()
		{
			CreateExtensionPointsConfiguration(false, false, false, true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNull(ExtensionPointsRepository.Instance.GetWordSplitterImplementation(), "Word splitter shouldn't be registered!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("Invalid word splitter configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid word splitter configuration!");
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			pluginDirectory = Path.GetTempPath();
			try
			{
				File.Copy("Parser.dll", Path.Combine(pluginDirectory, "Parser.dll"), true);
			}
			catch
			{
			}
			try
			{
				File.Copy("Core.dll", Path.Combine(pluginDirectory, "Core.dll"), true);
			}
			catch
			{
			}
			
			logFilePath = Path.Combine(pluginDirectory, "ExtensionAnalyzer.log");
			logger = new FileLogger(logFilePath).Logger;
		}

		[TearDown]
		public void TearDown()
		{
			ExtensionPointsRepository.Instance.ClearRepository();
		}

		private void CreateExtensionPointsConfiguration(
			bool addValidParserConfigurations, 
			bool addInvalidParserConfigurations, 
			bool addValidWordSplitterConfiguration,
			bool addInvalidWordSplitterConfiguration)
		{
			extensionPointsConfiguration = new ExtensionPointsConfiguration();
			extensionPointsConfiguration.PluginDirectoryPath = pluginDirectory;
			extensionPointsConfiguration.ParserExtensionPointsConfiguration = new List<ParserExtensionPointsConfiguration>();

			if(addValidParserConfigurations)
			{
				extensionPointsConfiguration.ParserExtensionPointsConfiguration.AddRange(
					new List<ParserExtensionPointsConfiguration>()
					{
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.Parser.SrcMLCSharpParser",
							LibraryFileRelativePath = "Parser.dll",
							SupportedFileExtensions = new List<string>() { ".cs" }
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.Parser.SrcMLCppParser",
							LibraryFileRelativePath = "Parser.dll",
							SupportedFileExtensions = new List<string>() { ".h", ".cpp", ".cxx" }
						}
					});
			}
			if(addInvalidParserConfigurations)
			{
				extensionPointsConfiguration.ParserExtensionPointsConfiguration.AddRange(
					new List<ParserExtensionPointsConfiguration>()
					{
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "",
							LibraryFileRelativePath = "Parser.dll",
							SupportedFileExtensions = new List<string>() { ".cs" }
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.Parser.SrcMLCppParser",
							LibraryFileRelativePath = "",
							SupportedFileExtensions = new List<string>() { ".h", ".cpp", ".cxx" }
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.Parser.SrcMLCppParser",
							LibraryFileRelativePath = "Parser.dll",
							SupportedFileExtensions = new List<string>()
						}
					});
			}

			if(addValidWordSplitterConfiguration)
			{
				extensionPointsConfiguration.WordSplitterExtensionPointConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.Core.Tools.WordSplitter",
						LibraryFileRelativePath = "Core.dll"
					};
			}

			if(addInvalidWordSplitterConfiguration)
			{
				extensionPointsConfiguration.WordSplitterExtensionPointConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.Core.Tools.WordSplitter",
						LibraryFileRelativePath = ""
					};
			}
		}

		private ExtensionPointsConfiguration extensionPointsConfiguration;
		private ILog logger;
		private string pluginDirectory;
		private string logFilePath;
	}
}
