using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using NUnit.Framework;
using Sando.ExtensionContracts.IndexerContracts;
using Sando.Indexer.IndexFiltering;
using log4net;
using log4net.Config;

namespace Sando.Indexer.UnitTests.IndexFiltering
{
    [TestFixture]
    public class IndexFilterManagerTest
    {
        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsFalseIfExtensionRuleMatches()
        {
            var testFileName = "index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                    {
                        IgnoredExtensions = new List<string> {"tmp", ".xml"}
                    };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = false;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsTrueIfNoExtensionRuleMatches()
        {
            var testFileName = "index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredExtensions = new List<string> { ".xaml", ".tmp" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = true;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsFalseIfFileNameRuleMatches()
        {
            var testFileName = "index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredFileNames = new List<string> { "index.tmp", "index.xml" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = false;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsTrueIfNoFileNameRuleMatches()
        {
            var testFileName = "index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredFileNames = new List<string> { "index.xaml", "debug.tmp" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = true;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsFalseIfDirectoryRuleMatches()
        {
            var testDirectoryName = "bin";
            var testFileName = "bin\\index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredDirectories = new List<string> { "bin", "obj" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                CreateTestDirectory(testDirectoryName);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = false;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
                DeleteTestDirectory(testDirectoryName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsTrueIfNoDirectoryRuleMatches()
        {
            var testDirectoryName = "project";
            var testFileName = "project\\index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredDirectories = new List<string> { "bin", "obj" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                CreateTestDirectory(testDirectoryName);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = true;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
                DeleteTestDirectory(testDirectoryName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsFalseIfPathExpressionsRuleMatches()
        {
            var testDirectoryName = "bin";
            var testFileName = "bin\\index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredPathExpressions = new List<string> { "bin\\*", "obj/*" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                CreateTestDirectory(testDirectoryName);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = false;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
                DeleteTestDirectory(testDirectoryName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsTrueIfNoPathExpressionsRuleMatches()
        {
            var testDirectoryName = "project";
            var testFileName = "project\\index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredPathExpressions = new List<string> { "bin\\*", "obj\\*" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                CreateTestDirectory(testDirectoryName);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = true;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
                DeleteTestDirectory(testDirectoryName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsFalseIfPathRegularExpressionsRuleMatches()
        {
            var testDirectoryName = "bin";
            var testFileName = "bin\\index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredPathRegularExpressions = new List<string> { @"bin\\.*\.xml" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                CreateTestDirectory(testDirectoryName);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = false;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
                DeleteTestDirectory(testDirectoryName);
            }
        }

        [Test]
        public void IndexFilterManager_ShouldFileBeIndexedReturnsTrueIfNoPathRegularExpressionsRuleMatches()
        {
            var testDirectoryName = "project";
            var testFileName = "project\\index.xml";
            try
            {
                var indexFilterSettings = new IndexFilterSettings
                {
                    IgnoredPathRegularExpressions = new List<string> { @"bin\\.*\.xml" }
                };
                var logger = CreateLog();
                var indexFilterManager = new IndexFilterManager(indexFilterSettings, logger);
                CreateTestDirectory(testDirectoryName);
                var fullFilePath = CreateTestFile(testFileName);
                var expected = true;
                var actual = indexFilterManager.ShouldFileBeIndexed(fullFilePath);
                Assert.AreEqual(expected, actual, "Invalid result from ShouldFileBeIndexed method!");
            }
            finally
            {
                DeleteTestFile(testFileName);
                DeleteTestDirectory(testDirectoryName);
            }
        }

        [SetUp]
        public void ResetContract()
        {
            contractFailed = false;
            Contract.ContractFailed += (sender, e) =>
            {
                e.SetHandled();
                e.SetUnwind();
                contractFailed = true;
            };
        }

        private static ILog CreateLog()
        {
            string configurationContent =
                @"<?xml version='1.0'?>
				<log4net>
					<appender name='MemoryAppender' type='log4net.Appender.MemoryAppender'>
					</appender>
					<root>
						<level value='DEBUG' />
						<appender-ref ref='MemoryAppender' />
					</root>
				</log4net>";
            XmlConfigurator.Configure(new MemoryStream(ASCIIEncoding.Default.GetBytes(configurationContent)));
            return LogManager.GetLogger("TestLogger");
        }

        private string CreateTestFile(string fileName)
        {
            var tmpDir = Path.GetTempPath();
            var fullFilePath = Path.Combine(tmpDir, fileName);
            if (!File.Exists(fullFilePath))
                File.WriteAllText(fullFilePath, String.Empty);
            return fullFilePath;
        }

        private void DeleteTestFile(string fileName)
        {
            var tmpDir = Path.GetTempPath();
            var fullFilePath = Path.Combine(tmpDir, fileName);
            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);
        }

        private string CreateTestDirectory(string directoryName)
        {
            var tmpDir = Path.GetTempPath();
            var fullDirectoryPath = Path.Combine(tmpDir, directoryName);
            if (!Directory.Exists(fullDirectoryPath))
                Directory.CreateDirectory(fullDirectoryPath);
            return fullDirectoryPath;
        }

        private void DeleteTestDirectory(string directoryName)
        {
            var tmpDir = Path.GetTempPath();
            var fullDirectoryPath = Path.Combine(tmpDir, directoryName);
            if (Directory.Exists(fullDirectoryPath))
                Directory.Delete(fullDirectoryPath);
        }

        private bool contractFailed;
    }
}