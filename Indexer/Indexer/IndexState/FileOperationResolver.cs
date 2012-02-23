using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Sando.Indexer.IndexState
{
	public class FileOperationResolver
	{
		public IndexOperation ResolveRequiredOperation(string fullFilePath, IndexFileState indexFileState)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "FileOperationResolver:ResolveRequiredOperation - full file path cannot be null or an empty string!");
			Contract.Requires(File.Exists(fullFilePath), "FileOperationResolver:ResolveRequiredOperation - full file path does not point to a valid file!");

			if(indexFileState == null)
				return IndexOperation.Add;
			else
			{
				FileInfo fileInfo = new FileInfo(fullFilePath);
				return indexFileState.LastIndexingDate != fileInfo.LastWriteTimeUtc ? IndexOperation.Update : IndexOperation.DoNothing;
			}
		}

		public DateTime GetDateOfLastModification(string fullFilePath)
		{
			Contract.Requires(!String.IsNullOrWhiteSpace(fullFilePath), "FileOperationResolver:GetDateOfLastModification - full file path cannot be null or an empty string!");
			Contract.Requires(File.Exists(fullFilePath), "FileOperationResolver:GetDateOfLastModification - full file path does not point to a valid file!");
			
			FileInfo fileInfo = new FileInfo(fullFilePath);
			return fileInfo.LastWriteTimeUtc;
		}
	}
}
