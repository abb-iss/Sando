using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Logging.Persistence;

namespace Sando.Core.Logging.Events
{
	public class SimpleLogEventHandlers
	{
		public static void RegisterLogEventHandlers()
		{
			LogEvents.Event_MonitoringStopped += new EventHandler(Handler_MonitoringStopped);
			LogEvents.Event_TestLogging += new EventHandler(Handler_TestLogging);

			LogEvents.Event_S3UploadStarted += new EventHandler<EventArgs<string>>(Handler_S3UploadStarted);
			LogEvents.Event_S3NoCredentials += new EventHandler(Handler_S3NoCredentials);
			LogEvents.Event_S3Error += new EventHandler<EventArgs<Exception>>(Handler_S3Error);
		}

		public static void UnregisterLogEventHandlers()
		{
			LogEvents.Event_MonitoringStopped -= new EventHandler(Handler_MonitoringStopped);
			LogEvents.Event_TestLogging -= new EventHandler(Handler_TestLogging);

			LogEvents.Event_S3UploadStarted -= new EventHandler<EventArgs<string>>(Handler_S3UploadStarted);
			LogEvents.Event_S3NoCredentials -= new EventHandler(Handler_S3NoCredentials);
			LogEvents.Event_S3Error -= new EventHandler<EventArgs<Exception>>(Handler_S3Error);
		}

		private static void Handler_S3Error(Object sender, EventArgs<Exception> exceptionArgs)
		{
			FileLogger.DefaultLogger.Error("S3LogWriter -- AWS Error occurred. Message:'{0}' when writing an object", exceptionArgs.Value);
		}

		private static void Handler_S3NoCredentials(Object sender, EventArgs e)
		{
			FileLogger.DefaultLogger.Error("S3LogWriter -- Cannot load S3 credentials. Log collecting is aborted.");
		}

		private static void Handler_S3UploadStarted(Object sender, EventArgs<string> filePathArgs)
		{
			FileLogger.DefaultLogger.Info("S3LogWriter - beginning to put file=" + filePathArgs.Value);
		}

		private static void Handler_MonitoringStopped(object sender, EventArgs e)
		{
			FileLogger.DefaultLogger.Info("Sando: MonitoringStopped()");
		}

		private static void Handler_TestLogging(object sender, EventArgs e)
		{
			FileLogger.DefaultLogger.Info("Message from the logger");
		}

	}
}
