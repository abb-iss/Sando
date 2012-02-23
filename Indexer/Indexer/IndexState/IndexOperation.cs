namespace Sando.Indexer.IndexState
{
	public enum IndexOperation
	{
		Add,		//file has not been indexed
		Update,		//file has changed since last indexing
		DoNothing	//file is up to date
	}
}
