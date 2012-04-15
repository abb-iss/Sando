using System.Collections.Generic;
using System.IO;
using log4net;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Configuration;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;
using UnitTestHelpers;

namespace Sando.Core.UnitTests.Extensions.Configuration
{
	[TestFixture]
	public class ExtensionPointsConfigurationAnalyzerTest
	{
		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersUsableCustomParser()
		{
			CreateExtensionPointsConfiguration(addValidParserConfigurations: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IParser parser = ExtensionPointsRepository.Instance.GetParserImplementation(".cs");

			Assert.IsNotNull(ExtensionPointsRepository.Instance.GetParserImplementation(".cs"), "Parser for '.cs' extension should be registered!");
			List<ProgramElement> programElements = null;
			Assert.DoesNotThrow(() => programElements = ExtensionPointsRepository.Instance.GetParserImplementation(".cs").Parse("filename"));
			Assert.IsTrue(programElements != null && programElements.Count == 1, "Invalid results from parse method!");
			Assert.AreEqual(programElements[0].Name, "TestCSharpName", "Name differs!");
			Assert.AreEqual(programElements[0].GetType().FullName, "Sando.TestExtensionPoints.TestElement", "Type differs!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomParsers()
		{
			CreateExtensionPointsConfiguration(addValidParserConfigurations: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);
			
			IParser parser = ExtensionPointsRepository.Instance.GetParserImplementation(".cs");
			Assert.IsNotNull(parser, "Parser for '.cs' extension should be registered!");
			Assert.AreEqual(parser.GetType().FullName, "Sando.TestExtensionPoints.TestCSharpParser", "Invalid parser returned for '.cs' extension!");

			parser = ExtensionPointsRepository.Instance.GetParserImplementation(".h");
			Assert.IsNotNull(parser, "Parser for '.h' extension should be registered!");
			Assert.AreEqual(parser.GetType().FullName, "Sando.TestExtensionPoints.TestCppParser", "Invalid parser returned for '.h' extension!");

			parser = ExtensionPointsRepository.Instance.GetParserImplementation(".cpp");
			Assert.IsNotNull(parser, "Parser for '.cpp' extension should be registered!");
			Assert.AreEqual(parser.GetType().FullName, "Sando.TestExtensionPoints.TestCppParser", "Invalid parser returned for '.cpp' extension!");

			parser = ExtensionPointsRepository.Instance.GetParserImplementation(".cxx");
			Assert.IsNotNull(parser, "Parser for '.cxx' extension should be registered!");
			Assert.AreEqual(parser.GetType().FullName, "Sando.TestExtensionPoints.TestCppParser", "Invalid parser returned for '.cxx' extension!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomParserConfigurations()
		{
			CreateExtensionPointsConfiguration(addInvalidParserConfigurations: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IParser parser = ExtensionPointsRepository.Instance.GetParserImplementation(".cs");
			Assert.IsNotNull(parser, "Default parser for '.cs' extension should be used!");
			Assert.AreEqual(parser.GetType().FullName, "Sando.Parser.SrcMLCSharpParser", "Invalid parser returned for '.cs' extension!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("3 invalid parser configurations found - they will be omitted during registration process."), "Log file should contain information about removed invalid parser configurations!");
		}
		
		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomWordSplitter()
		{
			CreateExtensionPointsConfiguration(addValidWordSplitterConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IWordSplitter wordSplitter = ExtensionPointsRepository.Instance.GetWordSplitterImplementation();
			Assert.IsNotNull(wordSplitter, "Word splitter should be registered!");
			Assert.AreEqual(wordSplitter.GetType().FullName, "Sando.TestExtensionPoints.TestWordSplitter", "Invalid word splitter returned!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomWordSplitterConfiguration()
		{
			CreateExtensionPointsConfiguration(addInvalidWordSplitterConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IWordSplitter wordSplitter = ExtensionPointsRepository.Instance.GetWordSplitterImplementation();
			Assert.IsNotNull(wordSplitter, "Default word splitter should be used!");
			Assert.AreEqual(wordSplitter.GetType().FullName, "Sando.Core.Tools.WordSplitter", "Invalid word splitter returned!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("Invalid word splitter configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid word splitter configuration!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RegistersCustomResultsReorderer()
		{
			CreateExtensionPointsConfiguration(addValidResultsReordererConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IResultsReorderer resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
			Assert.IsNotNull(resultsReorderer, "Results reorderer should be registered!");
			Assert.AreEqual(resultsReorderer.GetType().FullName, "Sando.TestExtensionPoints.TestResultsReorderer", "Invalid results reorderer returned!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomResultsReordererConfiguration()
		{
			CreateExtensionPointsConfiguration(addInvalidResultsReordererConfiguration: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IResultsReorderer resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
			Assert.IsNotNull(resultsReorderer, "Default results reorderer should be used!");
			Assert.AreEqual(resultsReorderer.GetType().FullName, "Sando.SearchEngine.SortByScoreResultsReorderer", "Invalid results reorderer returned!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("Invalid results reorderer configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid word splitter configuration!");
		}

		[Test]
		public void FindAndRegisterValidExtensionPoints_DoesNotRegisterInvalidExtensionPoints()
		{
			CreateExtensionPointsConfiguration(addInvalidExtensionPoints: true);
			ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

			IParser parser = ExtensionPointsRepository.Instance.GetParserImplementation(".cs");
			Assert.IsNotNull(parser, "Default parser for '.cs' extension should be used!");
			Assert.AreEqual(parser.GetType().FullName, "Sando.Parser.SrcMLCSharpParser", "Invalid parser returned for '.cs' extension!");

			IWordSplitter wordSplitter = ExtensionPointsRepository.Instance.GetWordSplitterImplementation();
			Assert.IsNotNull(wordSplitter, "Default word splitter should be used!");
			Assert.AreEqual(wordSplitter.GetType().FullName, "Sando.Core.Tools.WordSplitter", "Invalid word splitter returned!");

			IResultsReorderer resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
			Assert.IsNotNull(resultsReorderer, "Default results reorderer should be used!");
			Assert.AreEqual(resultsReorderer.GetType().FullName, "Sando.SearchEngine.SortByScoreResultsReorderer", "Invalid results reorderer returned!");

			string logFileContent = File.ReadAllText(logFilePath);
			Assert.IsTrue(logFileContent.Contains("Type cannot be found: Sando.NonExistingParser"), "Log file should contain information about errors occurred during the assembly loading!");
			Assert.IsTrue(logFileContent.Contains("Could not load file or assembly 'file:///" + pluginDirectory + "NonExistingParser.dll' or one of its dependencies"), "Log file should contain information about errors occurred during the assembly loading!");
			Assert.IsTrue(logFileContent.Contains("Could not load file or assembly 'file:///" + pluginDirectory + "NonExistingTestElement.dll' or one of its dependencies"), "Log file should contain information about errors occurred during the assembly loading!");
			Assert.IsTrue(logFileContent.Contains("Could not load file or assembly 'file:///" + pluginDirectory + "NonExistingWordSplitter.dll' or one of its dependencies"), "Log file should contain information about errors occurred during the assembly loading!");
			Assert.IsTrue(logFileContent.Contains("Type cannot be found: Sando.TestExtensionPoints.NonExistingResultsReorderer"), "Log file should contain information about errors occurred during the assembly loading!");
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			pluginDirectory = Path.GetTempPath();
			TestUtils.InitializeDefaultExtensionPoints();
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
			bool addInvalidResultsReordererConfiguration = false,
			bool addInvalidExtensionPoints = false)
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

			if(addInvalidExtensionPoints)
			{
				extensionPointsConfiguration.ParsersConfiguration.AddRange(
					new List<ParserExtensionPointsConfiguration>()
					{
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.NonExistingParser",
							LibraryFileRelativePath = "TestExtensionPoints.dll",
							SupportedFileExtensions = new List<string>() { ".cs" }
						},
						new ParserExtensionPointsConfiguration()
						{
							FullClassName = "Sando.TestExtensionPoints.TestCppParser",
							LibraryFileRelativePath = "NonExistingParser.dll",
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
									LibraryFileRelativePath = "NonExistingTestElement.dll"
								}
							}
						}
					});

				extensionPointsConfiguration.WordSplitterConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.TestExtensionPoints.TestWordSplitter",
						LibraryFileRelativePath = "NonExistingWordSplitter.dll"
					};

				extensionPointsConfiguration.ResultsReordererConfiguration =
					new BaseExtensionPointConfiguration()
					{
						FullClassName = "Sando.TestExtensionPoints.NonExistingResultsReorderer",
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
