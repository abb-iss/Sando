using System;

namespace Sando.Indexer.IndexState
{
	public class IndexFileState
	{
		public IndexFileState()
		{
		}

		public IndexFileState(string filePath, DateTime? lastIndexingDate)
		{
			FilePath = filePath;
			LastIndexingDate = lastIndexingDate;
		}

		public string FilePath { get; set; }
		public DateTime? LastIndexingDate { get; set; }
	}
}
