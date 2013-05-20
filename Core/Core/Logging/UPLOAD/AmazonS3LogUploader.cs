using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Sando.Core.Logging.Events;
using Sando.Core.Logging.Persistence;

namespace Sando.Core.Logging.Upload
{
	public class AmazonS3LogUploader
	{
		private static string _accessKeyId;
		private static string _secretAccessKey;
		private static string _bucketName;

		public static bool WriteLogFile(string uploadFilePath, string credentialFilePath)
		{
            Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
			LogEvents.S3UploadStarted(t, uploadFilePath);
			try
			{
				if(ReadS3Credentials(credentialFilePath) == false)
				{
                    LogEvents.S3NoCredentials(t);
					return false;
				}
				AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(_accessKeyId, _secretAccessKey);
				PutObjectRequest request = new PutObjectRequest();
				string fileName = System.IO.Path.GetFileName(uploadFilePath);
				request.WithFilePath(uploadFilePath).WithBucketName(_bucketName).WithKey(fileName);
				S3Response responseWithMetadata = client.PutObject(request);
				return true;
			}
			catch (AmazonS3Exception amazonS3Exception)
			{
				LogEvents.S3Error(t, amazonS3Exception);				
				return false;
			}
		}

        public static bool ReadLogFile(string s3FileName, string credentialFilePath)
        {
            Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            try
            {
                if (ReadS3Credentials(credentialFilePath) == false)
                {
                    LogEvents.S3NoCredentials(t);
                    return false;
                }
                AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(_accessKeyId, _secretAccessKey);
                GetObjectRequest request = new GetObjectRequest();
                request.WithBucketName(_bucketName).WithKey(s3FileName);
                S3Response responseWithMetadata = client.GetObject(request);
                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                LogEvents.S3Error(t, amazonS3Exception);
                return false;
            }
        }

		private static bool ReadS3Credentials(string s3CredentialFile)
		{
			if (! System.IO.File.Exists(s3CredentialFile))
			{
				return false;				
			}
			string[] lines = System.IO.File.ReadAllLines(s3CredentialFile);
			if (lines.Length < 3)
			{
				return false;
			}
			_accessKeyId = lines[0];
			_secretAccessKey = lines[1];
			_bucketName = lines[2];
			return true;
		}
	}
	
}

