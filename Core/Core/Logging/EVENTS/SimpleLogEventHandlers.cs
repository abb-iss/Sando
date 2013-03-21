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

            LogEvents.Event_FileNotFoundInArchiveError += new EventHandler<EventArgs<string>>(Handler_FileNotFoundInArchiveError);
            LogEvents.Event_ParsingFileGenericError += new EventHandler<EventArgs<string>>(Handler_ParsingFileGenericError);

            LogEvents.Event_CorruptIndexError += new EventHandler<EventArgs<Exception>>(Handler_CorruptIndexError);
            LogEvents.Event_LockObtainFailedError += new EventHandler<EventArgs<Exception>>(Handler_LockObtainFailedError);
            LogEvents.Event_IndexerIOError += new EventHandler<EventArgs<Exception>>(Handler_IndexerIOError);

			LogEvents.Event_S3UploadStarted += new EventHandler<EventArgs<string>>(Handler_S3UploadStarted);
			LogEvents.Event_S3NoCredentials += new EventHandler(Handler_S3NoCredentials);
			LogEvents.Event_S3Error += new EventHandler<EventArgs<Exception>>(Handler_S3Error);
		}

		public static void UnregisterLogEventHandlers()
		{
			LogEvents.Event_MonitoringStopped -= new EventHandler(Handler_MonitoringStopped);
			LogEvents.Event_TestLogging -= new EventHandler(Handler_TestLogging);

            LogEvents.Event_FileNotFoundInArchiveError -= new EventHandler<EventArgs<string>>(Handler_FileNotFoundInArchiveError);
            LogEvents.Event_ParsingFileGenericError -= new EventHandler<EventArgs<string>>(Handler_ParsingFileGenericError);

            LogEvents.Event_CorruptIndexError -= new EventHandler<EventArgs<Exception>>(Handler_CorruptIndexError);
            LogEvents.Event_LockObtainFailedError -= new EventHandler<EventArgs<Exception>>(Handler_LockObtainFailedError);
            LogEvents.Event_IndexerIOError -= new EventHandler<EventArgs<Exception>>(Handler_IndexerIOError);

			LogEvents.Event_S3UploadStarted -= new EventHandler<EventArgs<string>>(Handler_S3UploadStarted);
			LogEvents.Event_S3NoCredentials -= new EventHandler(Handler_S3NoCredentials);
			LogEvents.Event_S3Error -= new EventHandler<EventArgs<Exception>>(Handler_S3Error);
		}

        private static void Handler_ParsingFileGenericError(object sender, EventArgs<string> fileNameArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "The file could not be read: " + fileNameArg.Value);
        }

        private static void Handler_FileNotFoundInArchiveError(object sender, EventArgs<string> fileNameArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "File not found in archive: " + fileNameArg.Value);
        }

        private static void Handler_IndexerIOError(object sender, EventArgs<Exception> ioExArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", ioExArg.Value);
        }

        private static void Handler_LockObtainFailedError(object sender, EventArgs<Exception> lockObtainFailedExArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", lockObtainFailedExArg.Value);
        }

        private static void Handler_CorruptIndexError(Object sender, EventArgs<Exception> corruptIndexExArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", corruptIndexExArg.Value);
        }

		private static void Handler_S3Error(Object sender, EventArgs<Exception> exceptionArgs)
		{
            WriteErrorLogMessage(sender.GetType().ToString(), "AWS Error occurred. Message:'{0}' when writing an object", exceptionArgs.Value);
		}

		private static void Handler_S3NoCredentials(Object sender, EventArgs e)
		{
            WriteErrorLogMessage(sender.GetType().ToString(), "Cannot load S3 credentials. Log collecting is aborted.");
		}

		private static void Handler_S3UploadStarted(Object sender, EventArgs<string> filePathArgs)
		{
            WriteInfoLogMessage(sender.GetType().ToString(), "Beginning to put file=" + filePathArgs.Value);
		}

		private static void Handler_MonitoringStopped(object sender, EventArgs e)
		{
            WriteInfoLogMessage(sender.GetType().ToString(), "Monitoring stopped");
		}

		private static void Handler_TestLogging(object sender, EventArgs e)
		{
			WriteInfoLogMessage(sender.GetType().ToString(), "Message from the logger");
        }

        private static void WriteErrorLogMessage(string sendingType, string message, Exception e = null)
        {
            if (e != null)
            {
                FileLogger.DefaultLogger.Error(sendingType + ": " + ExceptionFormatter.CreateMessage(e,message));
            }
            else
            {
                FileLogger.DefaultLogger.Error(sendingType + ": " + message);
            }
        }

        private static void WriteInfoLogMessage(string sendingType, string message)
        {
            FileLogger.DefaultLogger.Info(sendingType + ": " + message);
        }
    }
}
