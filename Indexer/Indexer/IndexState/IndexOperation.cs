namespace Sando.Indexer.IndexState
{
	public enum IndexOperation
	{
		Add,		//file has not been indexed
		Update,		//file has changed since last indexing
        // Code changed by JZ on 10/29: Added the Delete case
        Delete,		//file has been deleted since last indexing 
        // End of code changes
        DoNothing	//file is up to date
	}
}
