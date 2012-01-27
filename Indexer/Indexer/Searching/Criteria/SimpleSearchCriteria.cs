using System.Collections.Generic;
using Sando.Core;

namespace Sando.Indexer.Searching.Criteria
{
	public class SimpleSearchCriteria : SearchCriteria
	{
		public SimpleSearchCriteria()
		{
			SearchTerms = new List<string>();
			MatchCase = false;
			MatchWholeWord = false;
			ExactMode = true;
			SearchWithinComments = false;
			SearchByAccessType = false;
			AccessLevels = new List<AccessLevel>();
			SearchByProgramElementType = false;
			ProgramElementTypes = new List<ProgramElementType>();
			SearchByUsageType = false;
			UsageTypes = new List<UsageType>();
			SearchInLocations = new List<string>();
		}

		public virtual List<string> SearchTerms { get; set; }
		public virtual bool MatchCase { get; set; }
		public virtual bool MatchWholeWord { get; set; }
		public virtual bool ExactMode { get; set; }
		public virtual bool SearchWithinComments { get; set; }
		public virtual bool SearchByAccessType { get; set; }
		public virtual List<AccessLevel> AccessLevels { get; set; }
		public virtual bool SearchByProgramElementType { get; set; }
		public virtual List<ProgramElementType> ProgramElementTypes { get; set; }
		public virtual bool SearchByUsageType { get; set; }
		public virtual List<UsageType> UsageTypes { get; set; }
		public virtual List<string> SearchInLocations { get; set; }
	}
}
