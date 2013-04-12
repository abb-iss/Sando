using Sando.Core.Logging.Events;
using Sando.Core.Logging.Persistence;
using Sando.Core.Logging.Upload;
using Sando.Core.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging
{
    public static class SandoLogManager
    {
        static SandoLogManager()
        {
            DefaultLoggingOn = false;
            DataCollectionOn = false;
        }

        public static void StartDefaultLogging(string logPath)
        {
            FileLogger.SetupDefaultFileLogger(logPath);
            DefaultLoggingOn = true;
        }

        public static void StartDataCollectionLogging(string logPath)
        {
            //Upload old data to S3 (randomly with p=0.33)
            Random random = new Random();
            int rand = random.Next(0, 3);
            if (rand == 0)
            {
                var s3UploadWorker = new BackgroundWorker();
                s3UploadWorker.DoWork += new DoWorkEventHandler(s3UploadWorker_DoWork);
                s3UploadWorker.RunWorkerAsync(logPath);
            }
            else
            {
                Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
                LogEvents.NoS3UploadDueToChance(t, rand);
            }

            var dataFileName = Path.Combine(logPath, "SandoData-" + Environment.MachineName + "-" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".log");
            var logger = FileLogger.CreateFileLogger("DataCollectionLogger", dataFileName);
            DataCollectionLogEventHandlers.InitializeLogFile(logger);
            DataCollectionOn = true;
        }

        public static void StopAllLogging()
        {
            DefaultLoggingOn = false;
            DataCollectionOn = false;
        }

        private static void s3UploadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string logPath = (string) e.Argument;
            string s3CredsPath = logPath + "//S3Credentials";
            string[] files = Directory.GetFiles(logPath);
            foreach (var file in files)
            {
                string fullPath = Path.GetFullPath(file);
                string fileName = Path.GetFileName(fullPath);

                if (fileName.StartsWith("SandoData-") && fileName.EndsWith("log"))
                {
                    bool success = AmazonS3LogUploader.WriteLogFile(fullPath, s3CredsPath);
                    if (success == true)
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
        }

        public static bool DefaultLoggingOn { get; private set; }
        public static bool DataCollectionOn { get; private set; }
    }
}
