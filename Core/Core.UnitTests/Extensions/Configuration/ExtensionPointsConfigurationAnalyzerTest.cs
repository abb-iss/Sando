using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sando.ExtensionContracts.IndexerContracts;
using log4net;
using NUnit.Framework;
using Sando.Core.Extensions;
using Sando.Core.Extensions.Configuration;
using Sando.Core.Extensions.Logging;
using Sando.ExtensionContracts.ParserContracts;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.QueryContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.ExtensionContracts.SplitterContracts;
using Sando.UnitTestHelpers;
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

            Assert.IsNotNull(parser, "Parser for '.cs' extension should be registered!");
            List<ProgramElement> programElements = null;
            Assert.DoesNotThrow(() => programElements = parser.Parse("filename"));
            Assert.IsTrue(programElements != null && programElements.Count == 1, "Invalid results from Parse method!");
            Assert.AreEqual(programElements[0].Name, "TestCSharpName", "Name differs!");
            Assert.AreEqual(programElements[0].GetType().FullName, "Sando.TestExtensionPoints.TestElement", "Type differs!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RegistersUsableCustomWordSplitter()
        {
            CreateExtensionPointsConfiguration(addValidWordSplitterConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IWordSplitter wordSplitter = ExtensionPointsRepository.Instance.GetWordSplitterImplementation();
            Assert.IsNotNull(wordSplitter, "Word splitter should be registered!");
            Assert.AreEqual(wordSplitter.GetType().FullName, "Sando.TestExtensionPoints.TestWordSplitter", "Invalid word splitter returned!");

            string[] splittedWords = null;
            Assert.DoesNotThrow(() => splittedWords = wordSplitter.ExtractWords("FileName"));
            Assert.IsTrue(splittedWords != null && splittedWords.Length == 2, "Invalid results from ExtractWords method!");
            Assert.AreEqual(splittedWords[0], "File", "First splitted word is invalid!");
            Assert.AreEqual(splittedWords[1], "Name", "Second splitted word is invalid!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RegistersUsableCustomResultsReorderer()
        {
            CreateExtensionPointsConfiguration(addValidResultsReordererConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IResultsReorderer resultsReorderer = ExtensionPointsRepository.Instance.GetResultsReordererImplementation();
            Assert.IsNotNull(resultsReorderer, "Results reorderer should be registered!");
            Assert.AreEqual(resultsReorderer.GetType().FullName, "Sando.TestExtensionPoints.TestResultsReorderer", "Invalid results reorderer returned!");

            List<CodeSearchResult> results = new List<CodeSearchResult>() 
												{
													new CodeSearchResult(SampleProgramElementFactory.GetSampleClassElement(), 1),
													new CodeSearchResult(SampleProgramElementFactory.GetSampleMethodElement(), 3),
												};
            Assert.DoesNotThrow(() => results = resultsReorderer.ReorderSearchResults(results.AsQueryable()).ToList());
            Assert.IsTrue(results != null && results.Count() == 2, "Invalid results from ReorderSearchResults method!");
            Assert.IsTrue(results.ElementAt(0).Score == 3 && results.ElementAt(0).ProgramElement.ProgramElementType == ProgramElementType.Method, "First result is invalid!");
            Assert.IsTrue(results.ElementAt(1).Score == 1 && results.ElementAt(1).ProgramElement.ProgramElementType == ProgramElementType.Class, "Second result is invalid!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RegistersUsableCustomQueryWeightsSupplier()
        {
            CreateExtensionPointsConfiguration(addValidQueryWeightsSupplierConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IQueryWeightsSupplier queryWeightsSupplier = ExtensionPointsRepository.Instance.GetQueryWeightsSupplierImplementation();
            Assert.IsNotNull(queryWeightsSupplier, "Query weights supplier should be registered!");
            Assert.AreEqual(queryWeightsSupplier.GetType().FullName, "Sando.TestExtensionPoints.TestQueryWeightsSupplier", "Invalid query weights supplier returned!");

            Dictionary<string, float> weights = null;
            Assert.DoesNotThrow(() => weights = queryWeightsSupplier.GetQueryWeightsValues());
            Assert.IsTrue(weights != null && weights.Count() == 2, "Invalid results from SetQueryWeightsValues method!");
            Assert.AreEqual(weights["field1"], 2, "First weight is invalid!");
            Assert.AreEqual(weights["field2"], 3, "Second weight is invalid!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RegistersUsableCustomQueryRewriter()
        {
            CreateExtensionPointsConfiguration(addValidQueryRewriterConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IQueryRewriter queryRewriter = ExtensionPointsRepository.Instance.GetQueryRewriterImplementation();
            Assert.IsNotNull(queryRewriter, "Query rewriter should be registered!");
            Assert.AreEqual(queryRewriter.GetType().FullName, "Sando.TestExtensionPoints.TestQueryRewriter", "Invalid query rewriter returned!");

            string query = null;
            Assert.DoesNotThrow(() => query = queryRewriter.RewriteQuery("Two Keywords"));
            Assert.IsFalse(String.IsNullOrWhiteSpace(query), "Invalid results from RewriteQuery method!");
            Assert.AreEqual(query, "two keywords", "Query is invalid!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RegistersUsableCustomIndexFilterManager()
        {
            CreateExtensionPointsConfiguration(addValidIndexFilterManagerConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IIndexFilterManager indexFilterManager = ExtensionPointsRepository.Instance.GetIndexFilterManagerImplementation();
            Assert.IsNotNull(indexFilterManager, "Index filter manager should be registered!");
            Assert.AreEqual(indexFilterManager.GetType().FullName, "Sando.TestExtensionPoints.TestIndexFilterManager", "Invalid index filter manager returned!");

            bool shouldBeIndexed = false;
            Assert.DoesNotThrow(() => shouldBeIndexed = indexFilterManager.ShouldFileBeIndexed("C:\\\\index.xml"));
            Assert.IsTrue(shouldBeIndexed, "Invalid results from ShouldFileBeIndexed method!");
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
        public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomQueryWeightsSupplierConfiguration()
        {
            CreateExtensionPointsConfiguration(addInvalidQueryWeightsSupplierConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IQueryWeightsSupplier queryWeightsSupplier = ExtensionPointsRepository.Instance.GetQueryWeightsSupplierImplementation();
            Assert.IsNotNull(queryWeightsSupplier, "Default query weights supplier should be used!");
            Assert.AreEqual(queryWeightsSupplier.GetType().FullName, "Sando.Indexer.Searching.QueryWeightsSupplier", "Invalid query weights supplier returned!");

            string logFileContent = File.ReadAllText(logFilePath);
            Assert.IsTrue(logFileContent.Contains("Invalid query weights supplier configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid word splitter configuration!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomQueryRewriterConfiguration()
        {
            CreateExtensionPointsConfiguration(addInvalidQueryRewriterConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IQueryRewriter queryRewriter = ExtensionPointsRepository.Instance.GetQueryRewriterImplementation();
            Assert.IsNotNull(queryRewriter, "Default query rewriter should be used!");
            Assert.AreEqual(queryRewriter.GetType().FullName, "Sando.Indexer.Searching.DefaultQueryRewriter", "Invalid query rewriter returned!");

            string logFileContent = File.ReadAllText(logFilePath);
            Assert.IsTrue(logFileContent.Contains("Invalid query rewriter configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid query rewriter configuration!");
        }

        [Test]
        public void FindAndRegisterValidExtensionPoints_RemovesInvalidCustomIndexFilterManagerConfiguration()
        {
            CreateExtensionPointsConfiguration(addInvalidIndexFilterManagerConfiguration: true);
            ExtensionPointsConfigurationAnalyzer.FindAndRegisterValidExtensionPoints(extensionPointsConfiguration, logger);

            IIndexFilterManager indexFilterManager = ExtensionPointsRepository.Instance.GetIndexFilterManagerImplementation();
            Assert.IsNotNull(indexFilterManager, "Default index filter manager should be used!");
            Assert.AreEqual(indexFilterManager.GetType().FullName, "Sando.Indexer.IndexFiltering.IndexFilterManager", "Invalid index filter manager returned!");

            string logFileContent = File.ReadAllText(logFilePath);
            Assert.IsTrue(logFileContent.Contains("Invalid index filter manager configuration found - it will be omitted during registration process."), "Log file should contain information about removed invalid index filter manager configuration!");
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
            Assert.IsTrue(logFileContent.Contains("Type cannot be found: Sando.TestExtensionPoints.NonExistingQueryWeightsSupplier"), "Log file should contain information about errors occurred during the assembly loading!");
            Assert.IsTrue(logFileContent.Contains("Type cannot be found: Sando.TestExtensionPoints.NonExistingQueryRewriter"), "Log file should contain information about errors occurred during the assembly loading!");
            Assert.IsTrue(logFileContent.Contains("Type cannot be found: Sando.TestExtensionPoints.NonExistingIndexFilterManager"), "Log file should contain information about errors occurred during the assembly loading!");
        }

        [SetUp]
        public void SetUp()
        {
            TestUtils.InitializeDefaultExtensionPoints();
        }

        [TestFixtureSetUp]
        public void TextFixtureSetUp()
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
            logger = FileLogger.CreateFileLogger("ExtensionPointsLogger", logFilePath);
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
            bool addValidQueryWeightsSupplierConfiguration = false,
            bool addInvalidQueryWeightsSupplierConfiguration = false,
            bool addValidQueryRewriterConfiguration = false,
            bool addInvalidQueryRewriterConfiguration = false,
            bool addValidIndexFilterManagerConfiguration = false,
            bool addInvalidIndexFilterManagerConfiguration = false,
            bool addInvalidExtensionPoints = false)
        {
            extensionPointsConfiguration = new ExtensionPointsConfiguration();
            extensionPointsConfiguration.PluginDirectoryPath = pluginDirectory;
            extensionPointsConfiguration.ParsersConfiguration = new List<ParserExtensionPointsConfiguration>();

            if (addValidParserConfigurations)
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
            if (addInvalidParserConfigurations)
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

            if (addValidWordSplitterConfiguration)
            {
                extensionPointsConfiguration.WordSplitterConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.TestWordSplitter",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addInvalidWordSplitterConfiguration)
            {
                extensionPointsConfiguration.WordSplitterConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.TestWordSplitter",
                        LibraryFileRelativePath = ""
                    };
            }

            if (addValidResultsReordererConfiguration)
            {
                extensionPointsConfiguration.ResultsReordererConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.TestResultsReorderer",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addInvalidResultsReordererConfiguration)
            {
                extensionPointsConfiguration.ResultsReordererConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addValidQueryWeightsSupplierConfiguration)
            {
                extensionPointsConfiguration.QueryWeightsSupplierConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.TestQueryWeightsSupplier",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addInvalidQueryWeightsSupplierConfiguration)
            {
                extensionPointsConfiguration.QueryWeightsSupplierConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addValidQueryRewriterConfiguration)
            {
                extensionPointsConfiguration.QueryRewriterConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.TestQueryRewriter",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addInvalidQueryRewriterConfiguration)
            {
                extensionPointsConfiguration.QueryRewriterConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addValidIndexFilterManagerConfiguration)
            {
                extensionPointsConfiguration.IndexFilterManagerConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.TestIndexFilterManager",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addInvalidIndexFilterManagerConfiguration)
            {
                extensionPointsConfiguration.IndexFilterManagerConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };
            }

            if (addInvalidExtensionPoints)
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

                extensionPointsConfiguration.QueryWeightsSupplierConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.NonExistingQueryWeightsSupplier",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };

                extensionPointsConfiguration.QueryRewriterConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.NonExistingQueryRewriter",
                        LibraryFileRelativePath = "TestExtensionPoints.dll"
                    };

                extensionPointsConfiguration.IndexFilterManagerConfiguration =
                    new BaseExtensionPointConfiguration()
                    {
                        FullClassName = "Sando.TestExtensionPoints.NonExistingIndexFilterManager",
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
