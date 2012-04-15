using System.Collections.Generic;
using System.IO;
using log4net;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Configuration;
using Sando.Core.Extensions.Logging;

namespace Sando.Core.UnitTests.Extensions.Configuration
{
	[TestFixture]
	public class ExtensionPointsConfigurationAnalyzerTest
	{
		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomParsers()
		{
			CreateExtensionPointsConfiguration(addValidParserConfigurations: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cs"), "Parser for '.cs' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".h"), "Parser for '.h' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cpp"), "Parser for '.cpp' extension should be registered!");
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cxx"), "Parser for '.cxx' extension should be registered!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomParserConfigurations()
		{
			CreateExtensionPointsConfiguration(addValidParserConfigurations: true, addInvalidParserConfigurations: true);
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
			CreateExtensionPointsConfiguration(addValidWordSplitterConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetWordSplitterImplementation(), "Word splitter should be registered!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomWordSplitterConfiguration()
		{
			CreateExtensionPointsConfiguration(addInvalidWordSplitterConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNull(ExtensionPointsRepository.Instance.GetWordSplitterImplementation(), "Word splitter shouldn't be registered!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("Invalid word splitter configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid word splitter configuration!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomResultsReorderer()
		{
			CreateExtensionPointsConfiguration(addValidResultsReordererConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetResultsReordererImplementation(), "Results reorderer should be registered!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomResultsReordererConfiguration()
		{
			CreateExtensionPointsConfiguration(addInvalidResultsReordererConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			Assert.IsNull(ExtensionPointsRepository.Instance.GetResultsReordererImplementation(), "Results reorderer shouldn't be registered!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("Invalid results reorderer configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid word splitter configuration!");
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			pluginDirectory = Path.GetTempPath();
			try
			{
				File.Copy("TestExtensionPoints.dll", Path.Combine(pluginDirectory, "TestExtensionPoints.dll"), true);
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
			bool addValidParserConfigurations = false, 
			bool addInvalidParserConfigurations = false, 
			bool addValidWordSplitterConfiguration = false,
			bool addInvalidWordSplitterConfiguration = false,
			bool addValidResultsReordererConfiguration = false,
			bool addInvalidResultsReordererConfiguration = false)
		{
			extensionPointsConfiguration = new ExtensionPointsConfiguration();
			extensionPointsConfiguration.PluginDirectoryPath = pluginDirectory;
			extensionPointsConfiguration.ParsersConfiguration = new List<ParserExtensionPointsConfiguration>();

			if(addValidParserConfigurations)
			{
				extensionPointsConfiguration.ParsersConfiguration.AddRange(
					new List<ParserExtensionPointsConfiguration>()
					{
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.TestExtensionPoints.TestCSharpParser",
							LibraryFileRelativePath = "TestExtensionPoints.dll",
							SupportedFileExtensions = new List<string>() { ".cs" },
							ProgramElementsConfiguration = new List<BaseExtensionPointConfiguration>()
							{
								new BaseExtensionPointConfiguration()
								{
									FullClassName = "Sando.TestExtensionPoints.TestElement",
									LibraryFileRelativePath = "TestExtensionPoints.dll"
								}
							}
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.TestExtensionPoints.TestCppParser",
							LibraryFileRelativePath = "TestExtensionPoints.dll",
							SupportedFileExtensions = new List<string>() { ".h", ".cpp", ".cxx" }
						}
					});
			}
			if(addInvalidParserConfigurations)
			{
				extensionPointsConfiguration.ParsersConfiguration.AddRange(
					new List<ParserExtensionPointsConfiguration>()
					{
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "",
							LibraryFileRelativePath = "TestExtensionPoints.dll",
							SupportedFileExtensions = new List<string>() { ".cs" }
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.TestExtensionPoints.TestCppParser",
							LibraryFileRelativePath = "",
							SupportedFileExtensions = new List<string>() { ".h", ".cpp", ".cxx" }
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.TestExtensionPoints.TestCppParser",
							LibraryFileRelativePath = "TestExtensionPoints.dll",
							SupportedFileExtensions = new List<string>(){".cs"},
							ProgramElementsConfiguration = new List<BaseExtensionPointConfiguration>()
							{
								new BaseExtensionPointConfiguration()
								{
									FullClassName = "Sando.TestExtensionPoints.TestElement",
									LibraryFileRelativePath = ""
								}
							}
						}
					});
			}

			if(addValidWordSplitterConfiguration)
			{
				extensionPointsConfiguration.WordSplitterConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.TestExtensionPoints.TestWordSplitter",
						LibraryFileRelativePath = "TestExtensionPoints.dll"
					};
			}

			if(addInvalidWordSplitterConfiguration)
			{
				extensionPointsConfiguration.WordSplitterConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.TestExtensionPoints.TestWordSplitter",
						LibraryFileRelativePath = ""
					};
			}

			if(addValidResultsReordererConfiguration)
			{
				extensionPointsConfiguration.ResultsReordererConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.TestExtensionPoints.TestResultsReorderer",
						LibraryFileRelativePath = "TestExtensionPoints.dll"
					};
			}

			if(addInvalidResultsReordererConfiguration)
			{
				extensionPointsConfiguration.ResultsReordererConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "",
						LibraryFileRelativePath = "TestExtensionPoints.dll"
					};
			}
		}

		private ExtensionPointsConfiguration extensionPointsConfiguration;
		private ILog logger;
		private string pluginDirectory;
		private string logFilePath;
	}
}
