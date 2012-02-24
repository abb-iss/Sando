using System;

namespace Sando.Indexer.IndexState
{
	public class PhysicalFileState
	{
		public PhysicalFileState(string filePath, DateTime? lastModificationDate)
		{
			FilePath = filePath;
			LastModificationDate = lastModificationDate;
		}

		public string FilePath { get; set; }
		public DateTime? LastModificationDate { get; set; }
	}
}
