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
using ABB.SrcML.VisualStudio.SolutionMonitor;

namespace UnitTestHelpers
{
    public class TestUtils
    {
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
			ExtensionPointsRepository extensionPointsRepository = ExtensionPointsRepository.Instance;
            PathManager.Create(Path.GetTempPath());
            var generator = new ABB.SrcML.SrcMLGenerator(@"LIBS\SrcML");
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
    }
}
