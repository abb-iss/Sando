using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Serialization;
using log4net;

namespace Sando.Core.Extensions.Configuration
{
	public static class ExtensionPointsConfigurationFileReader
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

        public static void WriteConfiguration(string extensionPointsConfigurationFilePath, ExtensionPointsConfiguration configuration)
        {
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(extensionPointsConfigurationFilePath);
                new XmlSerializer(typeof(ExtensionPointsConfiguration)).Serialize(writer,configuration);
            }
            catch (Exception ex)
            {
                throw new Exception("Saving extension points configuration file failed! - " + ex.StackTrace);
            }
            finally
            {
                writer.Close();
            }

        }

		private static ExtensionPointsConfiguration TryRetrieveConfigurationObject(string extensionPointsConfigurationFilePath)
		{
			if(String.IsNullOrWhiteSpace(extensionPointsConfigurationFilePath))
				throw new Exception("Extension points configuration file path cannot be null or an empty string!");
			if(!File.Exists(extensionPointsConfigurationFilePath))
				throw new Exception("Extension points configuration file wasn't found!");

			ExtensionPointsConfiguration extensionPointsConfiguration = null;
		    TextReader textReader = null;
			try
			{
				textReader = new StreamReader(extensionPointsConfigurationFilePath);
				extensionPointsConfiguration = (ExtensionPointsConfiguration)new XmlSerializer(typeof(ExtensionPointsConfiguration)).Deserialize(textReader);
			}
			catch(Exception ex)
			{
				throw new Exception("Reading extension points configuration file failed! - " + ex.StackTrace);
			}finally
			{
			    textReader.Close();
			}

			if(String.IsNullOrWhiteSpace(extensionPointsConfiguration.PluginDirectoryPath))
				throw new Exception("Plugin directory path must be set!");

			if(!Directory.Exists(extensionPointsConfiguration.PluginDirectoryPath))
				throw new Exception("Plugin directory wasn't found!");

			return extensionPointsConfiguration;
		}
	}
}
