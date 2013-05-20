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
            DataCollectionLogEventHandlers.InitializeDataCollection(logPath);
            DataCollectionOn = true;
        }

		public static void StopDataCollectionLogging()
		{
            DataCollectionLogEventHandlers.CloseDataCollection();
			DataCollectionOn = false;
		}

        public static void StopAllLogging()
        {
            DefaultLoggingOn = false;
            DataCollectionOn = false;
        }

        public static bool DefaultLoggingOn { get; private set; }
        public static bool DataCollectionOn { get; private set; }
    }
}
