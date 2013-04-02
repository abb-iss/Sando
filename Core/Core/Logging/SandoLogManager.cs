using Sando.Core.Logging.Events;
using Sando.Core.Logging.Persistence;
using Sando.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging
{
    public static class SandoLogManager
    {
        private static bool baseLoggingOn = false;

        public static void StartBaseLogging(string logPath)
        {
            FileLogger.SetupDefaultFileLogger(PathManager.Instance.GetExtensionRoot());
            DefaultLogEventHandler.RegisterLogEventHandlers();
            baseLoggingOn = true;
        }

        public static void StopAllLogging()
        {
            if (baseLoggingOn)
            {
                DefaultLogEventHandler.UnregisterLogEventHandlers();
            }
        }

    }
}
