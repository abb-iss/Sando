using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Upload
{
    [TestFixture]
    class AmazonS3LogUploaderTest
    {        	
		[Test]
		public void SimpleAmazonS3UploadTest()
		{
            string credentialFilePath = Environment.CurrentDirectory + "\\..\\..\\UI\\UI\\S3Credentials";
            string uploadFilePath = Environment.CurrentDirectory + "\\..\\..\\UI\\UI\\epl-1.0.txt";
            Assert.IsTrue(AmazonS3LogUploader.WriteLogFile(uploadFilePath, credentialFilePath));
		}

        [Test]
        public void EnsureS3DownloadNotPossibleTest()
        {
            string credentialFilePath = Environment.CurrentDirectory + "\\..\\..\\UI\\UI\\S3Credentials";
            string downloadFileName = "epl-1.0.txt";
            Assert.IsFalse(AmazonS3LogUploader.ReadLogFile(downloadFileName, credentialFilePath));
        }
	}
}
