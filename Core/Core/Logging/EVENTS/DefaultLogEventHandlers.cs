using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Logging.Persistence;
using log4net;

namespace Sando.Core.Logging.Events
{
	public class DefaultLogEventHandlers
	{
        public static void WriteErrorLogMessage(string sendingType, string message, Exception e = null)
        {
            if (SandoLogManager.DefaultLoggingOn)
            {
                if (e != null)
                {
                    FileLogger.DefaultLogger.Error(sendingType + ": " + ExceptionFormatter.CreateMessage(e, message));
                }
                else
                {
                    FileLogger.DefaultLogger.Error(sendingType + ": " + message);
                }
            }
        }

        public static void WriteInfoLogMessage(string sendingType, string message)
        {
            if (SandoLogManager.DefaultLoggingOn)
            {
                FileLogger.DefaultLogger.Info(sendingType + ": " + message);
            }
        }
    }
}
