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

        public static void SwumCacheFileNotExist(Object sender, string cachePath)
        {
			if(Event_SwumCacheFileNotExist != null) Event_SwumCacheFileNotExist(sender, new EventArgs<string>(cachePath));
        }



		#endregion
	}
}
