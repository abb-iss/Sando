using log4net;
using Sando.Core.Logging.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Sando.Core.Logging.Upload;

namespace Sando.Core.Logging.Events
{
    public static class DataCollectionLogEventHandlers
    {
        static DataCollectionLogEventHandlers()
        {
            _initialized = false;
        }

        public static void InitializeDataCollection(string logPath)
        {
            if (!_initialized)
            {
                var machineDomain = GetMachineDomain();
                var dataFileName = Path.Combine(logPath, "SandoData_v" + GetSandoVersion() + "_" + Environment.MachineName.GetHashCode() + "_" + machineDomain.GetHashCode() + DateTime.Now.ToString("_yyyy-MM-dd-HH.mm") + ".log");
                Logger = FileLogger.CreateFileLogger("DataCollectionLogger", dataFileName);
                CurrentLogFile = dataFileName;
                LogPath = logPath;
                _initialized = true;
            }

        }

        public static void CloseDataCollection()
        {
            if (_initialized)
            {
                FileLogger.CloseLogger("DataCollectionLogger");
                _initialized = false;
            }
        }

        public static void WriteInfoLogMessage(string sendingType, string message)
        {
            if (SandoLogManager.DataCollectionOn && _initialized)
            {
                Logger.Info(sendingType + ": " + message);
            }
        }

		public static void UploadLogFiles()
		{
            if (SandoLogManager.DataCollectionOn && _initialized)
            {
				DoS3Upload();
            }
		}

		private static void DoS3Upload()
		{
			string s3CredsPath = LogPath + "//S3Credentials";
			string[] files = Directory.GetFiles(LogPath, "*.log");
			foreach (var file in files)
			{
				string fullFilePath = Path.GetFullPath(file);
				FileInfo fileInfo = new FileInfo(fullFilePath);
				string fileName = Path.GetFileName(fullFilePath);
				if (fileName.StartsWith("SandoData"))
				{
                    if (fileInfo.Length > 400)
                    {
                        bool success = AmazonS3LogUploader.WriteLogFile(fullFilePath, s3CredsPath);
                        if (success == true)
                        {
                            if (fullFilePath == CurrentLogFile)
                            {
                                CloseDataCollection();
                                InitializeDataCollection(LogPath);
                            }
                            System.IO.File.Delete(fullFilePath);
                        }
                    }
                    else
                    {
                        if (fullFilePath != CurrentLogFile)
                        {
                            System.IO.File.Delete(fullFilePath);
                        }
                    }
				}
			}
		}

        private static string GetSandoVersion()
        {
            //TODO: Need to get the version dynamically somehow, instead of hardcoding it
            return "ExFdbck";
        }

		private static string GetMachineDomain()
		{
			var machineDomain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
			var md = System.Net.Dns.GetHostName();
			//if domain has the form x.y.z.com then return the last 2 parts: z.com 
			string[] domainSplit = machineDomain.Split('.');
			if(domainSplit.Count() >= 2)
			{
				return domainSplit[domainSplit.Count() - 2] + "." + domainSplit[domainSplit.Count() - 1];
			}
			return machineDomain;
		}

        private static ILog Logger;
		private static string CurrentLogFile;
		private static string LogPath;

		private static bool _initialized;
    }
}
