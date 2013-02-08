using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Serialization;
using Configuration.OptionsPages;
using Sando.DependencyInjection;
using log4net;

namespace Sando.Core.Extensions.Configuration
{
	public static class ExtensionPointsConfigurationFileReader
	{
		public static ExtensionPointsConfiguration ReadAndValidate(ILog logger)
		{
			Contract.Requires(logger != null, "ExtensionPointsConfigurationValidator:Validate - logger cannot be null!");

			logger.Info("Validating configuration started");
			ExtensionPointsConfiguration extensionPointsConfiguration = null;
			try
			{
			    extensionPointsConfiguration = TryRetrieveConfigurationObject();
			}
			catch(Exception ex)
			{
				logger.Fatal("Validation failed: " + ex.Message);
				return null;
			}
			logger.Info("Validating configuration finished");
			return extensionPointsConfiguration;
		}

        public static void WriteConfiguration(ExtensionPointsConfiguration configuration)
        {
            StreamWriter writer = null;
            try
            {
                var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions(); ;
			    writer = new StreamWriter(sandoOptions.ExtensionPointsConfigurationFilePath);
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

		private static ExtensionPointsConfiguration TryRetrieveConfigurationObject()
		{
            var sandoOptions = ServiceLocator.Resolve<ISandoOptionsProvider>().GetSandoOptions();
			if(String.IsNullOrWhiteSpace(sandoOptions.ExtensionPointsConfigurationFilePath))
				throw new Exception("Extension points configuration file path cannot be null or an empty string!");
			if(!File.Exists(sandoOptions.ExtensionPointsConfigurationFilePath))
				throw new Exception("Extension points configuration file wasn't found!");

			ExtensionPointsConfiguration extensionPointsConfiguration = null;
		    TextReader textReader = null;
			try
			{
				textReader = new StreamReader(sandoOptions.ExtensionPointsConfigurationFilePath);
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
