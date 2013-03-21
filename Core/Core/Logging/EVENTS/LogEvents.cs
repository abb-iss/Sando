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

        public static event EventHandler<EventArgs<string>> Event_FileNotFoundInArchiveError;
        public static event EventHandler<EventArgs<string>> Event_ParsingFileGenericError;

        public static void FileNotFoundInArchiveError(Object sender, string fileName)
        {
            if (Event_FileNotFoundInArchiveError != null) Event_FileNotFoundInArchiveError(sender, new EventArgs<string>(fileName));
        }

        public static void ParsingFileGenericError(Object sender, string fileName)
        {
            if (Event_ParsingFileGenericError != null) Event_ParsingFileGenericError(sender, new EventArgs<string>(fileName));
        }

        #endregion

        #region UIEvents

        public static event EventHandler Event_MonitoringStopped;

		public static void MonitoringStopped(Object sender)
		{
			if(Event_MonitoringStopped != null) Event_MonitoringStopped(sender, EventArgs.Empty);
		}

        #endregion

        #region IndexerEvents

        public static event EventHandler<EventArgs<Exception>> Event_CorruptIndexError;
        public static event EventHandler<EventArgs<Exception>> Event_LockObtainFailedError;
        public static event EventHandler<EventArgs<Exception>> Event_IndexerIOError;

        public static void LockObtainFailedError(Object sender, Exception lockFailedEx)
        {
            if (Event_LockObtainFailedError != null) Event_LockObtainFailedError(sender, new EventArgs<Exception>(lockFailedEx));
        }

        public static void CorruptIndexError(Object sender, Exception corruptIndexEx)
        {
            if (Event_CorruptIndexError != null) Event_CorruptIndexError(sender, new EventArgs<Exception>(corruptIndexEx));
        }

        public static void IndexerIOError(Object sender, Exception ioEx)
        {
            if (Event_IndexerIOError != null) Event_IndexerIOError(sender, new EventArgs<Exception>(ioEx));
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


	}
}
