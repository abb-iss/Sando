using System;
using System.Collections.Generic;
using System.IO;
using Sando.Core;
using Sando.Core.Extensions;
using Sando.Core.Tools;
using Sando.DependencyInjection;
using Sando.Indexer.IndexFiltering;
using Sando.Indexer.Searching;
using Sando.Parser;
using Sando.SearchEngine;
using Sando.Core.Logging;
using Sando.Core.Logging.Persistence;

namespace UnitTestHelpers
{
    public class TestUtils
    {
        static TestUtils() {
            SolutionDirectory = GetSolutionDirectory();
            SrcMLDirectory = Path.Combine(SolutionDirectory, "LIBS", "SrcML");
        }

        public static string SolutionDirectory { get; private set; }

        public static string SrcMLDirectory { get; private set; }
        public static void ClearDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }

		public static void InitializeDefaultExtensionPoints()
		{
            FileLogger.SetupDefaultFileLogger(Path.GetTempPath());
			ExtensionPointsRepository extensionPointsRepository = ExtensionPointsRepository.Instance;
            PathManager.Create(Path.GetTempPath());            
            var generator = new ABB.SrcML.SrcMLGenerator(SrcMLDirectory);

            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".cs" }, new SrcMLCSharpParser(generator));
            extensionPointsRepository.RegisterParserImplementation(new List<string>() { ".h", ".cpp", ".cxx" }, new SrcMLCppParser(generator));

			extensionPointsRepository.RegisterWordSplitterImplementation(new WordSplitter());

			extensionPointsRepository.RegisterResultsReordererImplementation(new SortByScoreResultsReorderer());

			extensionPointsRepository.RegisterQueryWeightsSupplierImplementation(new QueryWeightsSupplier());

			extensionPointsRepository.RegisterQueryRewriterImplementation(new DefaultQueryRewriter());

		    var solutionKey = new SolutionKey(Guid.NewGuid(), Path.GetTempPath());
            ServiceLocator.RegisterInstance(solutionKey);

            extensionPointsRepository.RegisterIndexFilterManagerImplementation(new IndexFilterManager());
		}

        internal static string GetSolutionDirectory() {
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            while(currentDirectory != null && !File.Exists(Path.Combine(currentDirectory.FullName, "Sando", "Sando.sln"))) {
                currentDirectory = currentDirectory.Parent;
            }
            return currentDirectory.FullName;
        }
    }
}
