using log4net;
using Sando.Core.Logging.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Sando.Core.Logging.Upload;
using Sando.DependencyInjection;
using Configuration.OptionsPages;

namespace Sando.Core.Logging.Events
{
    public static class DataCollectionLogEventHandlers
    {
        static DataCollectionLogEventHandlers()
        {
            _initialized = false;
        }

        public static void InitializeLogFile(string logPath)
        {
			if (!_initialized)
			{
				_initialized = true;
			}

			var machineDomain = GetMachineDomain();
			var dataFileName = Path.Combine(logPath, "SandoData-" + Environment.MachineName.GetHashCode() + "-" + machineDomain.GetHashCode() + DateTime.Now.ToString("-yyyy-MM-dd-HH.mm") + ".log");
			Logger = FileLogger.CreateFileLogger("DataCollectionLogger", dataFileName);
			CurrentLogFile = dataFileName;
			LogPath = logPath;
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
			if (!_initialized) return;

			var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();
			if (!sandoOptions.AllowDataCollectionLogging) return;

			//randomly with p=0.33
			Random random = new Random();
			int rand = random.Next(0, 3);
			if (rand == 0)
			{
				DoS3Upload();
			}
			else
			{
				Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
				LogEvents.NoS3UploadDueToChance(t, rand);
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
				if (fileName.StartsWith("SandoData-") && fileInfo.Length > 0)
				{
					bool success = AmazonS3LogUploader.WriteLogFile(fullFilePath, s3CredsPath);
					if (success == true)
					{
						System.IO.File.Delete(fullFilePath);
						if (fullFilePath == CurrentLogFile)
						{
							InitializeLogFile(LogPath);
						}
					}
				}
			}
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
