using System;
namespace Configuration.OptionsPages
{
	public class SandoOptions
	{
		public SandoOptions(string extensionPointsPluginDirectoryPath, string numberOfSearchResultsReturned)
		{
			this.ExtensionPointsPluginDirectoryPath = extensionPointsPluginDirectoryPath;
			int numberOfSearchResults = 20;
			int.TryParse(numberOfSearchResultsReturned, out numberOfSearchResults);
			this.NumberOfSearchResultsReturned = numberOfSearchResults <= 0 ? 20 : numberOfSearchResults;
		}

		public string ExtensionPointsPluginDirectoryPath { get; protected set; }
		public int NumberOfSearchResultsReturned { get; protected set; }
	}
}
