using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Sando.Indexer.IndexState
{
	public class PhysicalFilesStatesManager
	{
		public PhysicalFileState GetPhysicalFileState(string fullFilePath)
		{
            // Code changed by JZ on 10/30: To complete the Delete case: physical file CAN be not found
            //Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "PhysicalFilesStatesManager:GetPhysicalFileState - full file path cannot be null or an empty string!");
			//Contract.Requires(File.Exists(fullFilePath), "PhysicalFilesStatesManager:GetPhysicalFileState - full file path does not point to a valid file!");
            // End of code changes

            // Code changed by JZ on 10/29: To complete the Delete case: If there is not such a source file, then delete the index and/or srcML file.
            if (!File.Exists(fullFilePath))
            {
                return null;
            }
            // End of code changes

            FileInfo fileInfo = new FileInfo(fullFilePath);
			PhysicalFileState physicalFileState = new PhysicalFileState(fullFilePath, fileInfo.LastWriteTimeUtc);
			return physicalFileState;
		}
	}
}
