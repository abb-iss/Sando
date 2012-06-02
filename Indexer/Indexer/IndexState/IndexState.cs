using System;
using System.Collections.Generic;
using System.Linq;

namespace Sando.Indexer.IndexState
{
	public class IndexState
	{
		public IndexState()
		{
			RelevantFilesInfo = new List<RelevantFileInfo>();
		}

		public List<RelevantFileInfo> RelevantFilesInfo { get; set; }

		public override bool Equals(object obj)
		{
			IndexState indexState = obj as IndexState;
			if(indexState == null)
				return false;
			
			if(RelevantFilesInfo.Count != indexState.RelevantFilesInfo.Count)
				return false;
			if(RelevantFilesInfo.Count(f => !indexState.RelevantFilesInfo.Exists(f2 => f.FullName == f2.FullName && f.LastWriteTime == f2.LastWriteTime)) > 0)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public class RelevantFileInfo
	{
		public string FullName { get; set; }
		public DateTime LastWriteTime { get; set; }
	}
}
