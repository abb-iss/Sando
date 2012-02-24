using System.Diagnostics.Contracts;

namespace Sando.Indexer.IndexState
{
	public class FileOperationResolver
	{
		public IndexOperation ResolveRequiredOperation(PhysicalFileState physicalFileState, IndexFileState indexFileState)
		{
			Contract.Requires(physicalFileState != null, "FileOperationResolver:ResolveRequiredOperation - physical file state cannot be null!");
			
			if(indexFileState == null)
				return IndexOperation.Add;
			else
			{
				return indexFileState.LastIndexingDate != physicalFileState.LastModificationDate ? IndexOperation.Update : IndexOperation.DoNothing;
			}
		}
	}
}
