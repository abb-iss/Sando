using System.IO;
using log4net;
using log4net.Config;
using System.Xml;
using System.Text;

namespace Sando.Core.Extensions.Logging
{
	public class FileLogger
	{
		public FileLogger(string loggerLogFile)
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
			Logger = LogManager.GetLogger("ExtensionLogger");
		}

		public ILog Logger { get; set; }
	}
}
