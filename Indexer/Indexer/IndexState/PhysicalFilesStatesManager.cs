using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Sando.Indexer.IndexState
{
	public class PhysicalFilesStatesManager
	{
		public PhysicalFileState GetPhysicalFileState(string fullFilePath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "PhysicalFilesStatesManager:GetPhysicalFileState - full file path cannot be null or an empty string!");
			Contract.Requires(File.Exists(fullFilePath), "PhysicalFilesStatesManager:GetPhysicalFileState - full file path does not point to a valid file!");

			FileInfo fileInfo = new FileInfo(fullFilePath);
			PhysicalFileState physicalFileState = new PhysicalFileState(fullFilePath, fileInfo.LastWriteTimeUtc);
			return physicalFileState;
		}
	}
}
