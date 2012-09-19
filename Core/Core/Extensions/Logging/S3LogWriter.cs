using System;
using Amazon.S3;
using Amazon.S3.Model;

namespace Sando.Core.Extensions.Logging
{
	public class S3LogWriter
	{
		private static string AccessKeyID;
		private static string SecretAccessKey;
		private static string BucketName;

		public static readonly string S3CredentialFileLocation = Environment.CurrentDirectory + "\\..\\..\\Core\\Core\\Extensions\\Logging\\S3Credentials.txt";

		public static bool WriteLogFile(string filePath)
		{
			try
			{
				if(ReadS3Credentials() == false)
				{
					FileLogger.DefaultLogger.Debug("S3LogWriter -- Cannot load S3 credentials. Log collecting is aborted.");
					return false;
				}
				AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(AccessKeyID, SecretAccessKey);
				PutObjectRequest request = new PutObjectRequest();
				string fileName = System.IO.Path.GetFileName(filePath);
				request.WithFilePath(filePath).WithBucketName(BucketName).WithKey(fileName);
				S3Response responseWithMetadata = client.PutObject(request);
				return true;
			}
			catch (AmazonS3Exception amazonS3Exception)
			{
				if (amazonS3Exception.ErrorCode != null &&
				    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
				     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
				{
					FileLogger.DefaultLogger.Debug("S3LogWriter -- Check the provided AWS Credentials.");
				}
				else
				{
					FileLogger.DefaultLogger.Debug("S3LogWriter -- AWS Error occurred. Message:'{0}' when writing an object; " + amazonS3Exception.Message);
				}
				return false;
			}
		}

		private static bool ReadS3Credentials()
		{
			if (! System.IO.File.Exists(S3CredentialFileLocation))
			{
				FileLogger.DefaultLogger.Debug("S3LogWriter -- Cannot find S3 credential file");
				return false;				
			}
			string[] lines = System.IO.File.ReadAllLines(S3CredentialFileLocation);
			if (lines.Length < 3)
			{
				FileLogger.DefaultLogger.Debug("S3LogWriter -- Corrupt S3 credential file");
				return false;
			}
			AccessKeyID = lines[0];
			SecretAccessKey = lines[1];
			BucketName = lines[2];
			return true;
		}
	}
	
}
