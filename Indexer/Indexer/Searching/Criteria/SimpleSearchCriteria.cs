using System.Collections.Generic;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Searching.Criteria
{
	public class SimpleSearchCriteria : SearchCriteria
	{
		public SimpleSearchCriteria()
		{
			SearchTerms = new SortedSet<string>();
			MatchCase = false;
			ExactMode = true;
			SearchByAccessLevel = false;
			AccessLevels = new SortedSet<AccessLevel>();
			SearchByProgramElementType = false;
			ProgramElementTypes = new SortedSet<ProgramElementType>();
			SearchByUsageType = false;
			UsageTypes = new SortedSet<UsageType>();
			Locations = new SortedSet<string>();
		}


	    public override string ToQueryString()
		{
            var builder = new LuceneQueryStringBuilder(this);
            return builder.Build();
		}

        public bool Reformed { set; get; }
        public string Explanation { set; get; }

	    public override bool IsQueryReformed()
	    {
	        return Reformed;
	    }

	    public override string GetQueryReformExplanation()
	    {
	        return Explanation;
	    }

	    public SortedSet<string> SearchTerms { get; set; }
		public bool MatchCase { get; set; }
		public bool ExactMode { get; set; }
		public bool SearchByAccessLevel { get; set; }
		public SortedSet<AccessLevel> AccessLevels { get; set; }
		public bool SearchByProgramElementType { get; set; }
		public SortedSet<ProgramElementType> ProgramElementTypes { get; set; }
		public bool SearchByUsageType { get; set; }
		public SortedSet<UsageType> UsageTypes { get; set; }
		public bool SearchByLocation { get; set; }
		public SortedSet<string> Locations { get; set; }
        public bool SearchByFileExtension { get; set; }
        public SortedSet<string> FileExtensions { get; set; }

        public bool RequiresWildcards()
        {
            foreach (var term in SearchTerms)
            {
                var temp = LuceneQueryStringBuilder.GetTransformed(term);
                if (!temp.Equals(term))                
                    return true;
            }
            return false;
        }
    }
}
