using log4net;
using Sando.Core.Logging.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
    public static class DataCollectionLogEventHandlers
    {
        static DataCollectionLogEventHandlers()
        {
            _initialized = false;
        }

        public static void InitializeLogFile(ILog logger)
        {
            Logger = logger;
            _initialized = true;
        }

        public static void WriteInfoLogMessage(string sendingType, string message)
        {
            if (SandoLogManager.DataCollectionOn && _initialized)
            {
                Logger.Info(sendingType + ": " + message);
            }
        }

        private static ILog Logger;
        private static bool _initialized;
    }
}
