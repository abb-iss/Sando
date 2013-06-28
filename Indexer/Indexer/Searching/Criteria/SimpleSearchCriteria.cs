using System.Collections.Generic;
using System.Linq;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.Indexer.Searching.Criteria
{
	public class SimpleSearchCriteria : SearchCriteria
	{
		public SimpleSearchCriteria()
		{
            AccessLevels = new SortedSet<AccessLevel>();
            ExactMode = true;
            FileExtensions = new SortedSet<string>();
            Locations = new SortedSet<string>();
            MatchCase = false;
            ProgramElementTypes = new SortedSet<ProgramElementType>();
            SearchByAccessLevel = false;
            SearchByProgramElementType = false;
            SearchTerms = new SortedSet<string>();									
			SearchByUsageType = false;
			UsageTypes = new SortedSet<UsageType>();			
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

	    public override IQueryable<string> GetRecommendedQueries()
	    {
	        return RecommendedQueries;
	    }

	    public IQueryable<string> RecommendedQueries { set; get; }
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

        public bool IsLiteralSearch()
        {
            foreach (var term in SearchTerms)
            {                
                if (term.StartsWith("\""))             
                    return true;
            }
            return false;
        }
    }
}
