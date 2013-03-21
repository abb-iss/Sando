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

		public static string S3CredentialDirectory; 

		public static bool WriteLogFile(string filePath)
		{
			LogEvents.S3UploadStarted(null, filePath);
			try
			{
				if(ReadS3Credentials() == false)
				{
					LogEvents.S3NoCredentials(null);
					return false;
				}
				AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(_accessKeyId, _secretAccessKey);
				PutObjectRequest request = new PutObjectRequest();
				string fileName = System.IO.Path.GetFileName(filePath);
				request.WithFilePath(filePath).WithBucketName(_bucketName).WithKey(fileName);
				S3Response responseWithMetadata = client.PutObject(request);
				return true;
			}
			catch (AmazonS3Exception amazonS3Exception)
			{
				LogEvents.S3Error(null, amazonS3Exception);				
				return false;
			}
		}

		private static bool ReadS3Credentials()
		{
			string s3CredentialFileLocation = S3CredentialDirectory + "\\S3Credentials.txt";

			if (! System.IO.File.Exists(s3CredentialFileLocation))
			{
				return false;				
			}
			string[] lines = System.IO.File.ReadAllLines(s3CredentialFileLocation);
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

