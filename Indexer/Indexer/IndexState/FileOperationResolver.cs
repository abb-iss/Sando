using System.Diagnostics.Contracts;

namespace Sando.Indexer.IndexState
{
	public class FileOperationResolver
	{
		public IndexOperation ResolveRequiredOperation(PhysicalFileState physicalFileState, IndexFileState indexFileState)
		{
            // Code changed by JZ on 10/30: To complete the Delete case: physical file state CAN be null
            //Contract.Requires(physicalFileState != null, "FileOperationResolver:ResolveRequiredOperation - physical file state cannot be null!");
            // End of code changes
			
			if(indexFileState == null)
				return IndexOperation.Add;
			else
			{
                // Code changed by JZ on 10/29: To complete the Delete case
				//return indexFileState.LastIndexingDate != physicalFileState.LastModificationDate ? IndexOperation.Update : IndexOperation.DoNothing;
                if (physicalFileState == null)  // If the source file does not exist, but the index file exists
                {
                    return IndexOperation.Delete;
                }
                else
                {
                    if (indexFileState.LastIndexingDate != physicalFileState.LastModificationDate)  // Need to change != to < ???
                    {
                        return IndexOperation.Update;
                    }
                    else
                    {
                        return IndexOperation.DoNothing;
                    }
                }
                // End of code changes
			}
		}
	}
}
