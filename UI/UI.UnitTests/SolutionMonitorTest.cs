using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sando.UI.Monitoring;
using Sando.Core;
using Sando.Indexer;

namespace Sando.UI.UnitTests
{
    [TestFixture]
    public class SolutionMonitorTest
    {
        private const string _luceneTempIndexesDirectory = "C:/Windows/Temp";

        [Test]
        public void SolutionMonitor_BasicSetupTest()
        {
            Directory.CreateDirectory(_luceneTempIndexesDirectory+"/basic/");
            var key = new SolutionKey(new Guid(), ".\\TestFiles\\FourCSFiles", _luceneTempIndexesDirectory+"/basic/");
            var indexer = DocumentIndexerFactory.CreateIndexer(key, AnalyzerType.Snowball);
            var monitor = new SolutionMonitor(new SolutionWrapper(), key, indexer);
            string[] files = Directory.GetFiles(".\\TestFiles\\FourCSFiles");
            foreach (var file in files)
            {
                monitor.ProcessFileForTesting(file);
            }
            monitor.UpdateAfterAdditions();
            monitor.StopMonitoring();
        }
    }
}
