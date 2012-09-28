using System;
using NUnit.Framework;
using Sando.UI.InterleavingExperiment.Logging;

namespace Sando.UI.UnitTests
{
	[TestFixture]
	class S3LogWriterTest
	{
		[Test]
		public void TestWriteLog()
		{
			S3LogWriter.S3CredentialDirectory = Environment.CurrentDirectory + "\\..\\..\\Core\\Core\\Extensions\\Logging";
			if(!System.IO.File.Exists(S3LogWriter.S3CredentialDirectory + "\\S3Credentials.txt "))
			{
				//logging is off due to unavailabily of S3 credentials
				return;
			}
			else 
			{
				string logFilePath = Environment.CurrentDirectory + "\\..\\..\\Core\\Core.UnitTests\\TestFiles\\TestLog.txt";
				bool success = S3LogWriter.WriteLogFile(logFilePath);
				Assert.IsTrue(success);
			}
		}
	}
}
