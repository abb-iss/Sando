using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
	public class LogEvents
	{
        public static void TestLogging(Object sender)
        {
            DefaultLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Message from the logger");
        }

        #region ParserEvents

        public static void ParserFileNotFoundInArchiveError(Object sender, string fileName)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "File not found in archive: " + fileName);
        }

        public static void ParserGenericFileError(Object sender, string fileName)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "The file could not be read: " + fileName);
        }

        #endregion

        #region UIEvents

        public static void SolutionOpened(Object sender, string solutionName)
        {
            DataCollectionLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Message from the logger");
        }

        public static void UIGenericError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UISandoSearchingError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UIRespondToSolutionOpeningError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UISolutionOpeningError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UISolutionClosingError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UISandoWindowActivationError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UISandoInitializationError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

		public static void UISandoBeginInitialization(Object sender)
        {
            DefaultLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Sando initialization started");
        }

        public static void UIIndexUpdateError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

        public static void UIOpenFileError(Object sender, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ex);
        }

		public static void UIMonitoringStopped(Object sender)
		{
            DefaultLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Monitoring stopped");
		}

        #endregion

        #region IndexerEvents

        public static void IndexLockObtainFailed(Object sender, Exception lockFailedEx)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", lockFailedEx);
        }

        public static void IndexCorruptError(Object sender, Exception corruptIndexEx)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", corruptIndexEx);
        }

        public static void IndexIOError(Object sender, Exception ioEx)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "", ioEx);
        }

        #endregion

        #region S3Events

        public static void S3Error(Object sender, Exception awsException)
		{
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "AWS Error occurred when writing an object", awsException);
		}

		public static void S3NoCredentials(Object sender)
		{
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "Cannot load S3 credentials. Log collecting is aborted.");
		}

		public static void S3UploadStarted(Object sender, string filePath)
		{
            DefaultLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Beginning to put file=" + filePath);
		}

        #endregion

        #region SwumEvents

        public static void SwumErrorCreatingSwum(Object sender, string sourcePath, Exception ex)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "Error creating SWUM on file: " + sourcePath, ex);
        }

        public static void SwumErrorGeneratingSrcML(Object sender, string sourcePath)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "Error converting file to SrcML, no file unit found: " + sourcePath);
        }

        public static void SwumFileNotFoundInArchive(Object sender, string sourcePath)
        {
            DefaultLogEventHandlers.WriteErrorLogMessage(sender.GetType().ToString(), "File not found in archive: " + sourcePath);
        }

        public static void SwumCacheFileNotExist(Object sender, string cachePath)
        {
            DefaultLogEventHandlers.WriteInfoLogMessage(sender.GetType().ToString(), "Cache file does not exist: " + cachePath);
        }

		#endregion
	}
}
