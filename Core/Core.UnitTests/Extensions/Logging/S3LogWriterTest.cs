using System;
using NUnit.Framework;
using Sando.Core.Extensions.Logging;

namespace Sando.Core.UnitTests.Extensions.Logging
{
	[TestFixture]
	class S3LogWriterTest
	{
		[Test]
		public void TestWriteLog()
		{
			if(!System.IO.File.Exists(S3LogWriter.S3CredentialFileLocation))
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
