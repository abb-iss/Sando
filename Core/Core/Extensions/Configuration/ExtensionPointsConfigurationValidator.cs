using System;
using System.IO;
using System.Reflection;
using log4net;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;
using System.Linq;
using System.Runtime.Remoting;
using Sando.ExtensionContracts.ParserContracts;

namespace Sando.Core.Extensions.Configuration
{
	public static class ExtensionPointsConfigurationValidator
	{
		public static ExtensionPointsConfiguration ReadAndValidate(string extensionPointsConfigurationFilePath, ILog logger)
		{
			Contract.Requires(logger != null, "ExtensionPointsConfigurationValidator:Validate - logger cannot be null!");

			logger.Info("Validating configuration started");
			ExtensionPointsConfiguration extensionPointsConfiguration = null;
			try
			{
				extensionPointsConfiguration = TryRetrieveConfigurationObject(extensionPointsConfigurationFilePath);
			}
			catch(Exception ex)
			{
				logger.Fatal("Validation failed: " + ex.Message);
				return null;
			}
			logger.Info("Validating configuration finished");
			return extensionPointsConfiguration;
		}

		private static ExtensionPointsConfiguration TryRetrieveConfigurationObject(string extensionPointsConfigurationFilePath)
		{
			if(String.IsNullOrWhiteSpace(extensionPointsConfigurationFilePath))
				throw new Exception("Extension points configuration file path cannot be null or an empty string!");
			if(!File.Exists(extensionPointsConfigurationFilePath))
				throw new Exception("Extension points configuration file wasn't found!");

			ExtensionPointsConfiguration extensionPointsConfiguration = null;
			try
			{
				TextReader textReader = new StreamReader(extensionPointsConfigurationFilePath);
				extensionPointsConfiguration = (ExtensionPointsConfiguration)new XmlSerializer(typeof(ExtensionPointsConfiguration)).Deserialize(textReader);
			}
			catch(Exception ex)
			{
				throw new Exception("Reading extension points configuration file failed! - " + ex.StackTrace);
			}

			if(String.IsNullOrWhiteSpace(extensionPointsConfiguration.PluginDirectoryPath))
				throw new Exception("Plugin directory path must be set!");

			if(!Directory.Exists(extensionPointsConfiguration.PluginDirectoryPath))
				throw new Exception("Plugin directory wasn't found!");

			return extensionPointsConfiguration;
		}
	}
}
