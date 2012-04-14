using System.Collections.Generic;
namespace Sando.Core.Extensions.Configuration
{
	public class ExtensionPointsConfiguration
	{
		public string PluginDirectoryPath { get; set; }
		public List<ParserExtensionPointsConfiguration> ParserExtensionPointsConfiguration { get; set; }
		public BaseExtensionPointConfiguration WordSplitterExtensionPointConfiguration { get; set; }
	}
}
