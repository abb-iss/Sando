using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sando.ExtensionContracts.ParserContracts;

namespace Sando.Core.Extensions
{
	public class ExtensionPointsRepository
	{
		public IParser GetParserImplementation(string fileExtension)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fileExtension), "ExtensionPointsManager:GetParserImplementation - fileExtension cannot be null or an empty string!");
			
			if(parsers.ContainsKey(fileExtension))
				return parsers[fileExtension];
			else
				return null;
		}

		public void RegisterParserImplementation(List<string> supportedFileExtensions, IParser parserImplementation)
		{
			Contract.Requires(supportedFileExtensions != null, "ExtensionPointsManager:RegisterParserImplementation - supportedFileExtensions cannot be null!");
			Contract.Requires(supportedFileExtensions.Count > 0, "ExtensionPointsManager:RegisterParserImplementation - supportedFileExtensions must contain at least one item!");
			Contract.Requires(supportedFileExtensions.FindAll(sfe => String.IsNullOrWhiteSpace(sfe)).Count == 0, "ExtensionPointsManager:RegisterParserImplementation - supportedFileExtensions cannot contain empty items!");
			Contract.Requires(parserImplementation != null, "ExtensionPointsManager:RegisterParserImplementation - parserImplementation cannot be null!");
			
			foreach(string supportedFileExtension in supportedFileExtensions)
				parsers[supportedFileExtension] = parserImplementation;
		}

		public static ExtensionPointsRepository Instance
		{
			get
			{
				if(extensionManager == null)
					extensionManager = new ExtensionPointsRepository();
				return extensionManager;
			}
		}

		private ExtensionPointsRepository()
		{
			parsers = new Dictionary<string, IParser>();
		}

		private static ExtensionPointsRepository extensionManager;

		private Dictionary<string, IParser> parsers;
	}
}
