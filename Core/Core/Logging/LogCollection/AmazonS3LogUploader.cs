using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Sando.Core.Logging;

namespace Sando.Core.Logging.LogCollection
{
	public class AmazonS3LogUploader
	{
		private static string _accessKeyId;
		private static string _secretAccessKey;
		private static string _bucketName;

		public static string S3CredentialDirectory; 

		public static bool WriteLogFile(string filePath)
		{
			FileLogger.DefaultLogger.Debug("S3LogWriter - beginning to put file=" + filePath);
			try
			{
				if(ReadS3Credentials() == false)
				{
					FileLogger.DefaultLogger.Debug("S3LogWriter -- Cannot load S3 credentials. Log collecting is aborted.");
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
			string s3CredentialFileLocation = S3CredentialDirectory + "\\S3Credentials.txt";

			if (! System.IO.File.Exists(s3CredentialFileLocation))
			{
				FileLogger.DefaultLogger.Debug("S3LogWriter -- Cannot find S3 credential file");
				return false;				
			}
			string[] lines = System.IO.File.ReadAllLines(s3CredentialFileLocation);
			if (lines.Length < 3)
			{
				FileLogger.DefaultLogger.Debug("S3LogWriter -- Corrupt S3 credential file");
				return false;
			}
			_accessKeyId = lines[0];
			_secretAccessKey = lines[1];
			_bucketName = lines[2];
			return true;
		}
	}
	
}

