using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Config;

namespace Sando.Core.Extensions.Logging
{
	public class FileLogger
	{
        public static ILog CreateCustomLogger(string logFilePath)
        {
            if(!logs.ContainsKey(logFilePath))
                logs.Add(logFilePath, CreateLog(logFilePath));
            return logs[logFilePath];
        }

        static FileLogger()
        {
            FileInfo fileInfo = new FileInfo(Assembly.GetCallingAssembly().Location);
            defaultLogPath = Path.Combine(fileInfo.DirectoryName, "Sando" + Guid.NewGuid() + ".log");
            defaultInstance = CreateLog(defaultLogPath);
            logs = new Dictionary<string, ILog>();
        }
        
        public static ILog DefaultLogger
        {
            get
            {
                return defaultInstance;
            }
        }

        private static ILog CreateLog(string loggerLogFile)
		{
			string configurationContent =
				@"<?xml version='1.0'?>
				<log4net>
					<appender name='FileAppender' type='log4net.Appender.FileAppender'>
						<file value='" + loggerLogFile + @"' />
						<appendToFile value='false' />
						<lockingModel type='log4net.Appender.FileAppender+MinimalLock' />
						<maximumFileSize value='100KB' />
						<layout type='log4net.Layout.PatternLayout'>
							<conversionPattern value='%date %-5level %logger - %message%newline' />
						</layout>
					</appender>
    
					<root>
						<level value='DEBUG' />
						<appender-ref ref='FileAppender' />
					</root>
				</log4net>";
			XmlConfigurator.Configure(new MemoryStream(ASCIIEncoding.Default.GetBytes(configurationContent)));
			return LogManager.GetLogger("ExtensionLogger");
		}

        private static ILog defaultInstance;
        private static Dictionary<string, ILog> logs;

        private static string defaultLogPath;
	}
}
