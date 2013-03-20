using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.Core.Logging.Events
{
	public class LogEvents
	{
		/*
		public class StringEventArgs : EventArgs
		{
			StringEventArgs(string str) { this.str = str; }
			public string str { get; private set; }
		}
		*/

		public static event EventHandler Event_MonitoringStopped;
		public static event EventHandler Event_TestLogging;

		public static void MonitoringStopped()
		{
			if(Event_MonitoringStopped != null) Event_MonitoringStopped(null, EventArgs.Empty);
		}

		public static void TestLogging()
		{
			if(Event_TestLogging != null) Event_TestLogging(null, EventArgs.Empty);
		}

	#region S3events

		public static event EventHandler<EventArgs<string>> Event_S3UploadStarted;
		public static event EventHandler Event_S3NoCredentials;
		public static event EventHandler<EventArgs<Exception>> Event_S3Error;

		public static void S3Error(Exception awsException)
		{
			if(Event_S3Error != null) Event_S3Error(null, new EventArgs<Exception>(awsException));
		}

		public static void S3NoCredentials()
		{
			if(Event_S3NoCredentials != null) Event_S3NoCredentials(null, null);
		}

		public static void S3UploadStarted(string filePath)
		{
			if(Event_S3UploadStarted != null) Event_S3UploadStarted(null, new EventArgs<string>(filePath));
		}

	#endregion


	}
}
