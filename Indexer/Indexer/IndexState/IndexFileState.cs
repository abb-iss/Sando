using System;

namespace Sando.Indexer.IndexState
{
	public class IndexFileState
	{
		public IndexFileState(DateTime? lastIndexingDate)
		{
			LastIndexingDate = lastIndexingDate;
		}

		public DateTime? LastIndexingDate { get; set; }
	}
}
