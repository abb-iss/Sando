using System.IO;
using NUnit.Framework;
using Sando.Core.Logging;
using System.Linq;
using Sando.Core.Logging.Persistence;
using Sando.Core.Logging.Events;

namespace Sando.Core.UnitTests.Logging
{
    [TestFixture]
    public class FileLoggerTest
    {
        [Test]
        public void GIVEN_FileLoggerNotInitialize_WHEN_SetupDefautlFileLoggerMethodIsCalled_AND_DefaultLoggerIsUsed_THEN_LogFileShouldBeCreatedAndContainLoggedMessage()
        {
            FileLogger.SetupDefaultFileLogger(_directoryPath);
			SimpleLogEventHandlers.RegisterLogEventHandlers();
			LogEvents.TestLogging(this);
            var logFiles = Directory.GetFiles(_directoryPath).AsEnumerable().Where(f => f.Contains("Sando") && f.EndsWith(".log")).ToList();
            Assert.IsTrue(logFiles.Any(), "There should be log file created!");
            var content = File.ReadAllText(logFiles.First());
            Assert.IsTrue(content.Contains("Message from the logger"), "Invalid log file content");
        }

        [SetUp]
        public void SetUp()
        {
            _directoryPath = Path.Combine(Path.GetTempPath(), "LogTest");
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, true);
                Directory.CreateDirectory(_directoryPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directoryPath))
                Directory.Delete(_directoryPath, true);
        }

        private string _directoryPath;
    }
}