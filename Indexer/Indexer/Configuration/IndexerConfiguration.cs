using System.Configuration;

namespace Sando.Indexer.Configuration
{
	public class IndexerConfiguration
	{
		public static string GetValue(string settingName)
		{
			return ConfigurationManager.AppSettings[settingName];
		}
	}
}
