using System.Collections.Generic;

namespace Sando.Core.Extensions.Configuration
{
	public class ParserExtensionPointsConfiguration : BaseExtensionPointConfiguration
	{
		public List<string> SupportedFileExtensions { get; set; }
	}
}
