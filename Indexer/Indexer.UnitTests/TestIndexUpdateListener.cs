namespace Sando.Indexer.UnitTests
{
	public class TestIndexUpdateListener : IIndexUpdateListener
	{
		public void NotifyAboutIndexUpdate()
		{
			NotifyCalled = true;
		}

		public bool NotifyCalled { get; set; }
	}
}
