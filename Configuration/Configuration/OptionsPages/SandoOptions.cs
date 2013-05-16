using System.IO;

namespace Configuration.OptionsPages
{
	public class SandoOptions
	{
		public SandoOptions(string extensionPointsPluginDirectoryPath, int numberOfSearchResultsReturned, bool allowLogCollection)
		{
			ExtensionPointsPluginDirectoryPath = extensionPointsPluginDirectoryPath;
            ExtensionPointsConfigurationFilePath = Path.Combine(extensionPointsPluginDirectoryPath, "ExtensionPointsConfiguration.xml");
            NumberOfSearchResultsReturned = numberOfSearchResultsReturned;
			AllowLogCollection = allowLogCollection;
		}

		public string ExtensionPointsPluginDirectoryPath { get; protected set; }
        public string ExtensionPointsConfigurationFilePath { get; protected set; }
		public int NumberOfSearchResultsReturned { get; protected set; }
		public bool AllowLogCollection { get; protected set; }
	}
}
