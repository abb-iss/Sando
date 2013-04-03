using Sando.Core.Logging.Events;
using Sando.Core.Logging.Persistence;
using Sando.Core.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging
{
    public static class SandoLogManager
    {
        private static bool defaultLoggingOn = false;
        private static bool dataCollectionOn = false;

        public static void StartDefaultLogging(string logPath)
        {
            FileLogger.SetupDefaultFileLogger(PathManager.Instance.GetExtensionRoot());
            DefaultLogEventHandlers.RegisterLogEventHandlers();
            defaultLoggingOn = true;
        }

        public static void StartDataCollectionLogging(string logPath)
        {
            var dataFileName = Path.Combine(PathManager.Instance.GetExtensionRoot(), "SandoData-" + Environment.MachineName + "-" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".log");
            var logger = FileLogger.CreateFileLogger("DataCollectionLogger", dataFileName);
            DataCollectionLogEventHandlers.InitializeLogFile(logger);
            DataCollectionLogEventHandlers.RegisterLogEventHandlers();
            dataCollectionOn = true;
        }

        public static void StopAllLogging()
        {
            if (defaultLoggingOn)
            {
                DefaultLogEventHandlers.UnregisterLogEventHandlers();
            }
            if (dataCollectionOn)
            {
                DataCollectionLogEventHandlers.UnregisterLogEventHandlers();
            }
        }

    }
}
