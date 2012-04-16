using System.Collections.Generic;

namespace Sando.Core.Extensions.Configuration
{
	public class ParserExtensionPointsConfiguration : BaseExtensionPointConfiguration
	{
		public ParserExtensionPointsConfiguration()
		{
			SupportedFileExtensions = new List<string>();
			ProgramElementsConfiguration = new List<BaseExtensionPointConfiguration>();
		}

		public List<string> SupportedFileExtensions { get; set; }
		public List<BaseExtensionPointConfiguration> ProgramElementsConfiguration { get; set; }
	}
}
