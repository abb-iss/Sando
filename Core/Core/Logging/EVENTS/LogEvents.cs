using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
	public class LogEvents
	{
		public static event EventHandler Event_TestLogging;

        public static void TestLogging(Object sender)
        {
            if (Event_TestLogging != null) Event_TestLogging(sender, EventArgs.Empty);
        }

        #region ParserEvents

        public static event EventHandler<EventArgs<string>> Event_ParserFileNotFoundInArchiveError;
        public static event EventHandler<EventArgs<string>> Event_ParserGenericFileError;

        public static void ParserFileNotFoundInArchiveError(Object sender, string fileName)
        {
            if (Event_ParserFileNotFoundInArchiveError != null) Event_ParserFileNotFoundInArchiveError(sender, new EventArgs<string>(fileName));
        }

        public static void ParserGenericFileError(Object sender, string fileName)
        {
            if (Event_ParserGenericFileError != null) Event_ParserGenericFileError(sender, new EventArgs<string>(fileName));
        }

        #endregion

        #region UIEvents

        public static event EventHandler Event_UIMonitoringStopped;
        public static event EventHandler<EventArgs<Exception>> Event_UIOpenFileError;
        public static event EventHandler<EventArgs<Exception>> Event_UIIndexUpdateError;
        public static event EventHandler Event_UISandoBeginInitialization;
        public static event EventHandler<EventArgs<Exception>> Event_UISandoInitializationError;
        public static event EventHandler<EventArgs<Exception>> Event_UISandoWindowActivationError;
        public static event EventHandler<EventArgs<Exception>> Event_UISolutionClosingError;
        public static event EventHandler<EventArgs<Exception>> Event_UISolutionOpeningError;
        public static event EventHandler<EventArgs<Exception>> Event_UIGenericError;
        public static event EventHandler<EventArgs<Exception>> Event_UIRespondToSolutionOpeningError;
        public static event EventHandler<EventArgs<Exception>> Event_UISandoSearchingError;

        public static void UIGenericError(Object sender, Exception ex)
        {
            if (Event_UIGenericError != null) Event_UIGenericError(sender, new EventArgs<Exception>(ex));
        }

        public static void UISandoSearchingError(Object sender, Exception ex)
        {
            if (Event_UISandoSearchingError != null) Event_UISandoSearchingError(sender, new EventArgs<Exception>(ex));
        }

        public static void UIRespondToSolutionOpeningError(Object sender, Exception ex)
        {
            if (Event_UIRespondToSolutionOpeningError != null) Event_UIRespondToSolutionOpeningError(sender, new EventArgs<Exception>(ex));
        }

        public static void UISolutionOpeningError(Object sender, Exception ex)
        {
            if (Event_UISolutionOpeningError != null) Event_UISolutionOpeningError(sender, new EventArgs<Exception>(ex));
        }

        public static void UISolutionClosingError(Object sender, Exception ex)
        {
            if (Event_UISolutionClosingError != null) Event_UISolutionClosingError(sender, new EventArgs<Exception>(ex));
        }

        public static void UISandoWindowActivationError(Object sender, Exception ex)
        {
            if (Event_UISandoWindowActivationError != null) Event_UISandoWindowActivationError(sender, new EventArgs<Exception>(ex));
        }

        public static void UISandoInitializationError(Object sender, Exception ex)
        {
            if (Event_UISandoInitializationError != null) Event_UISandoInitializationError(sender, new EventArgs<Exception>(ex));
        }

		public static void UISandoBeginInitialization(Object sender)
        {
            if (Event_UISandoBeginInitialization != null) Event_UISandoBeginInitialization(sender, EventArgs.Empty);
        }

        public static void UIIndexUpdateError(Object sender, Exception ex)
        {
            if (Event_UIIndexUpdateError != null) Event_UIIndexUpdateError(sender, new EventArgs<Exception>(ex));
        }

        public static void UIOpenFileError(Object sender, Exception ex)
        {
            if (Event_UIOpenFileError != null) Event_UIOpenFileError(sender, new EventArgs<Exception>(ex));
        }

		public static void UIMonitoringStopped(Object sender)
		{
			if(Event_UIMonitoringStopped != null) Event_UIMonitoringStopped(sender, EventArgs.Empty);
		}

        #endregion

        #region IndexerEvents

        public static event EventHandler<EventArgs<Exception>> Event_IndexCorruptError;
        public static event EventHandler<EventArgs<Exception>> Event_IndexLockObtainFailed;
        public static event EventHandler<EventArgs<Exception>> Event_IndexIOError;

        public static void IndexLockObtainFailed(Object sender, Exception lockFailedEx)
        {
            if (Event_IndexLockObtainFailed != null) Event_IndexLockObtainFailed(sender, new EventArgs<Exception>(lockFailedEx));
        }

        public static void IndexCorruptError(Object sender, Exception corruptIndexEx)
        {
            if (Event_IndexCorruptError != null) Event_IndexCorruptError(sender, new EventArgs<Exception>(corruptIndexEx));
        }

        public static void IndexIOError(Object sender, Exception ioEx)
        {
            if (Event_IndexIOError != null) Event_IndexIOError(sender, new EventArgs<Exception>(ioEx));
        }

        #endregion

        #region S3Events

        public static event EventHandler<EventArgs<string>> Event_S3UploadStarted;
		public static event EventHandler Event_S3NoCredentials;
		public static event EventHandler<EventArgs<Exception>> Event_S3Error;

		public static void S3Error(Object sender, Exception awsException)
		{
			if(Event_S3Error != null) Event_S3Error(sender, new EventArgs<Exception>(awsException));
		}

		public static void S3NoCredentials(Object sender)
		{
			if(Event_S3NoCredentials != null) Event_S3NoCredentials(sender, null);
		}

		public static void S3UploadStarted(Object sender, string filePath)
		{
			if(Event_S3UploadStarted != null) Event_S3UploadStarted(sender, new EventArgs<string>(filePath));
		}

		#endregion

		#region RecommenderEvents

		public static event EventHandler<EventArgs<string>> Event_SwumCacheFileNotExist;
		public static event EventHandler<EventArgs<string>> Event_SwumFileNotFoundInArchive;
        public static event EventHandler<EventArgs<string>> Event_SwumErrorGeneratingSrcML;
        public static event EventHandler<EventArgs<string, Exception>> Event_SwumErrorCreatingSwum;

        public static void SwumErrorCreatingSwum(Object sender, string sourcePath, Exception ex)
        {
            if (Event_SwumErrorCreatingSwum != null) Event_SwumErrorCreatingSwum(sender, new EventArgs<string, Exception>(sourcePath, ex));
        }

        public static void SwumErrorGeneratingSrcML(Object sender, string sourcePath)
        {
            if (Event_SwumErrorGeneratingSrcML != null) Event_SwumErrorGeneratingSrcML(sender, new EventArgs<string>(sourcePath));
        }

        public static void SwumFileNotFoundInArchive(Object sender, string sourcePath)
        {
            if (Event_SwumFileNotFoundInArchive != null) Event_SwumFileNotFoundInArchive(sender, new EventArgs<string>(sourcePath));
        }

        public static void SwumCacheFileNotExist(Object sender, string cachePath)
        {
			if(Event_SwumCacheFileNotExist != null) Event_SwumCacheFileNotExist(sender, new EventArgs<string>(cachePath));
        }



		#endregion
	}
}
