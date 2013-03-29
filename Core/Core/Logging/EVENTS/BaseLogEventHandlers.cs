using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sando.Core.Logging.Persistence;

namespace Sando.Core.Logging.Events
{
	public class BaseLogEventHandlers
	{
		public static void RegisterLogEventHandlers()
		{
            LogEvents.Event_TestLogging += new EventHandler(Handler_TestLogging);

			LogEvents.Event_UIMonitoringStopped += new EventHandler(Handler_UIMonitoringStopped);
            LogEvents.Event_UIOpenFileError += new EventHandler<EventArgs<Exception>>(Handler_UIOpenFileError);
            LogEvents.Event_UIIndexUpdateError += new EventHandler<EventArgs<Exception>>(Handler_UIIndexUpdateError);
			LogEvents.Event_UISandoBeginInitialization += new EventHandler(Handler_UISandoBeginInitialization);
            LogEvents.Event_UISandoInitializationError += new EventHandler<EventArgs<Exception>>(Handler_UISandoInitializationError);
            LogEvents.Event_UISandoWindowActivationError += new EventHandler<EventArgs<Exception>>(Handler_UISandoWindowActivationError);
            LogEvents.Event_UISolutionClosingError += new EventHandler<EventArgs<Exception>>(Handler_UISolutionClosingError);
            LogEvents.Event_UIRespondToSolutionOpeningError += new EventHandler<EventArgs<Exception>>(Handler_UIRespondToSolutionOpeningError);

            LogEvents.Event_ParserFileNotFoundInArchiveError += new EventHandler<EventArgs<string>>(Handler_ParserFileNotFoundInArchiveError);
            LogEvents.Event_ParserGenericFileError += new EventHandler<EventArgs<string>>(Handler_ParserGenericFileError);

            LogEvents.Event_IndexCorruptError += new EventHandler<EventArgs<Exception>>(Handler_IndexCorruptError);
            LogEvents.Event_IndexLockObtainFailed += new EventHandler<EventArgs<Exception>>(Handler_IndexLockObtainFailed);
            LogEvents.Event_IndexIOError += new EventHandler<EventArgs<Exception>>(Handler_IndexIOError);

			LogEvents.Event_S3UploadStarted += new EventHandler<EventArgs<string>>(Handler_S3UploadStarted);
			LogEvents.Event_S3NoCredentials += new EventHandler(Handler_S3NoCredentials);
			LogEvents.Event_S3Error += new EventHandler<EventArgs<Exception>>(Handler_S3Error);

			LogEvents.Event_SwumCacheFileNotExist += new EventHandler<EventArgs<string>>(Handler_SwumCacheFileNotExist);
            LogEvents.Event_SwumFileNotFoundInArchive += new EventHandler<EventArgs<string>>(Handler_SwumFileNotFoundInArchive);
            LogEvents.Event_SwumErrorGeneratingSrcML += new EventHandler<EventArgs<string>>(Handler_SwumErrorGeneratingSrcML);
            LogEvents.Event_SwumErrorCreatingSwum += new EventHandler<EventArgs<string, Exception>>(Handler_SwumErrorCreatingSwum);
		}

		public static void UnregisterLogEventHandlers()
		{
            LogEvents.Event_TestLogging -= new EventHandler(Handler_TestLogging);

            LogEvents.Event_UIMonitoringStopped -= new EventHandler(Handler_UIMonitoringStopped);
            LogEvents.Event_UIOpenFileError -= new EventHandler<EventArgs<Exception>>(Handler_UIOpenFileError);
            LogEvents.Event_UIIndexUpdateError -= new EventHandler<EventArgs<Exception>>(Handler_UIIndexUpdateError);
			LogEvents.Event_UISandoBeginInitialization -= new EventHandler(Handler_UISandoBeginInitialization);
            LogEvents.Event_UISandoInitializationError -= new EventHandler<EventArgs<Exception>>(Handler_UISandoInitializationError);
            LogEvents.Event_UISandoWindowActivationError -= new EventHandler<EventArgs<Exception>>(Handler_UISandoWindowActivationError);
            LogEvents.Event_UISolutionClosingError -= new EventHandler<EventArgs<Exception>>(Handler_UISolutionClosingError);
            LogEvents.Event_UIRespondToSolutionOpeningError -= new EventHandler<EventArgs<Exception>>(Handler_UIRespondToSolutionOpeningError);

            LogEvents.Event_ParserFileNotFoundInArchiveError -= new EventHandler<EventArgs<string>>(Handler_ParserFileNotFoundInArchiveError);
            LogEvents.Event_ParserGenericFileError -= new EventHandler<EventArgs<string>>(Handler_ParserGenericFileError);

            LogEvents.Event_IndexCorruptError -= new EventHandler<EventArgs<Exception>>(Handler_IndexCorruptError);
            LogEvents.Event_IndexLockObtainFailed -= new EventHandler<EventArgs<Exception>>(Handler_IndexLockObtainFailed);
            LogEvents.Event_IndexIOError -= new EventHandler<EventArgs<Exception>>(Handler_IndexIOError);

			LogEvents.Event_S3UploadStarted -= new EventHandler<EventArgs<string>>(Handler_S3UploadStarted);
			LogEvents.Event_S3NoCredentials -= new EventHandler(Handler_S3NoCredentials);
			LogEvents.Event_S3Error -= new EventHandler<EventArgs<Exception>>(Handler_S3Error);

			LogEvents.Event_SwumCacheFileNotExist -= new EventHandler<EventArgs<string>>(Handler_SwumCacheFileNotExist);
            LogEvents.Event_SwumFileNotFoundInArchive -= new EventHandler<EventArgs<string>>(Handler_SwumFileNotFoundInArchive);
            LogEvents.Event_SwumErrorGeneratingSrcML -= new EventHandler<EventArgs<string>>(Handler_SwumErrorGeneratingSrcML);
            LogEvents.Event_SwumErrorCreatingSwum += new EventHandler<EventArgs<string, Exception>>(Handler_SwumErrorCreatingSwum);
		}

        private static void Handler_UIRespondToSolutionOpeningError(object sender, EventArgs<Exception> exArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", exArg.Value);
        }

        private static void Handler_UISolutionClosingError(object sender, EventArgs<Exception> exArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", exArg.Value);
        }

        private static void Handler_UISandoWindowActivationError(object sender, EventArgs<Exception> exArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", exArg.Value);
        }

        private static void Handler_UISandoInitializationError(object sender, EventArgs<Exception> exArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", exArg.Value);
        }

		private static void Handler_UISandoBeginInitialization(object sender, EventArgs e)
        {
            WriteInfoLogMessage(sender.GetType().ToString(), "Sando initialization started");
        }

        private static void Handler_UIIndexUpdateError(object sender, EventArgs<Exception> exArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", exArg.Value);
        }

        private static void Handler_UIOpenFileError(object sender, EventArgs<Exception> exArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", exArg.Value);
        }

        private static void Handler_UIMonitoringStopped(object sender, EventArgs e)
        {
            WriteInfoLogMessage(sender.GetType().ToString(), "Monitoring stopped");
        }

        private static void Handler_SwumErrorCreatingSwum(Object sender, EventArgs<string, Exception> errorArgs)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "Error creating SWUM on file: " + errorArgs.FirstValue, errorArgs.SecondValue);
        }

        private static void Handler_SwumErrorGeneratingSrcML(Object sender, EventArgs<string> sourcePathArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "Error converting file to SrcML, no file unit found: " + sourcePathArg.Value);
        }

        private static void Handler_SwumFileNotFoundInArchive(Object sender, EventArgs<string> sourcePathArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "File not found in archive: " + sourcePathArg.Value);
        }

		private static void Handler_SwumCacheFileNotExist(Object sender, EventArgs<string> cachePathArg)
		{
            WriteInfoLogMessage(sender.GetType().ToString(), "Cache file does not exist: " + cachePathArg.Value);
		}

        private static void Handler_ParserGenericFileError(object sender, EventArgs<string> fileNameArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "The file could not be read: " + fileNameArg.Value);
        }

        private static void Handler_ParserFileNotFoundInArchiveError(object sender, EventArgs<string> fileNameArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "File not found in archive: " + fileNameArg.Value);
        }

        private static void Handler_IndexIOError(object sender, EventArgs<Exception> ioExArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", ioExArg.Value);
        }

        private static void Handler_IndexLockObtainFailed(object sender, EventArgs<Exception> lockObtainFailedExArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", lockObtainFailedExArg.Value);
        }

        private static void Handler_IndexCorruptError(Object sender, EventArgs<Exception> corruptIndexExArg)
        {
            WriteErrorLogMessage(sender.GetType().ToString(), "", corruptIndexExArg.Value);
        }

		private static void Handler_S3Error(Object sender, EventArgs<Exception> exceptionArgs)
		{
            WriteErrorLogMessage(sender.GetType().ToString(), "AWS Error occurred when writing an object", exceptionArgs.Value);
		}

		private static void Handler_S3NoCredentials(Object sender, EventArgs e)
		{
            WriteErrorLogMessage(sender.GetType().ToString(), "Cannot load S3 credentials. Log collecting is aborted.");
		}

		private static void Handler_S3UploadStarted(Object sender, EventArgs<string> filePathArgs)
		{
            WriteInfoLogMessage(sender.GetType().ToString(), "Beginning to put file=" + filePathArgs.Value);
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
