using System.Collections.Generic;
using System.Linq;

namespace Sando.Indexer.IndexState
{
	public class IndexState
	{
		public IndexState()
		{
			UsedFieldNames = new List<string>();
			UsedParsers = new Dictionary<string, string>();
		}

		public List<string> UsedFieldNames { get; set; }
		public Dictionary<string, string> UsedParsers { get; set; }
		public string UsedSplitter { get; set; }
		public string UsedAnalyzer { get; set; }

		public override bool Equals(object obj)
		{
			IndexState indexState = obj as IndexState;
			if(indexState == null)
				return false;
			
			if(UsedFieldNames.Count != indexState.UsedFieldNames.Count)
				return false;
			if(UsedFieldNames.Count(f => !indexState.UsedFieldNames.Contains(f)) > 0)
				return false;

			if(UsedParsers.Count != indexState.UsedParsers.Count)
				return false;
			if(UsedParsers.Count(f => !indexState.UsedParsers.Contains(f)) > 0)
				return false;

			if(UsedSplitter != indexState.UsedSplitter)
				return false;

			if(UsedAnalyzer != indexState.UsedAnalyzer)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
