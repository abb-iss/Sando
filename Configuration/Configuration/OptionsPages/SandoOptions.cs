using System.IO;

namespace Configuration.OptionsPages
{
	public class SandoOptions
	{
		public SandoOptions(string extensionPointsPluginDirectoryPath, int numberOfSearchResultsReturned)
		{
			ExtensionPointsPluginDirectoryPath = extensionPointsPluginDirectoryPath;
            ExtensionPointsConfigurationFilePath = Path.Combine(extensionPointsPluginDirectoryPath, "ExtensionPointsConfiguration.xml");
            NumberOfSearchResultsReturned = numberOfSearchResultsReturned;
		}

		public string ExtensionPointsPluginDirectoryPath { get; protected set; }
        public string ExtensionPointsConfigurationFilePath { get; protected set; }
		public int NumberOfSearchResultsReturned { get; protected set; }
	}
}
