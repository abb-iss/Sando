using System.Configuration;

namespace Sando.Configuration
{
	public class Configuration
	{
		public static string GetValue(string settingName)
		{
			return ConfigurationManager.AppSettings[settingName];
		}
	}
}
