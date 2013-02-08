using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Exceptions;
using Sando.Translation;

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
            LuceneQueryStringBuilder builder = new LuceneQueryStringBuilder(this);
            return builder.Build();

		}



		public virtual SortedSet<string> SearchTerms { get; set; }
		public virtual bool MatchCase { get; set; }
		public virtual bool ExactMode { get; set; }
		public virtual bool SearchByAccessLevel { get; set; }
		public virtual SortedSet<AccessLevel> AccessLevels { get; set; }
		public virtual bool SearchByProgramElementType { get; set; }
		public virtual SortedSet<ProgramElementType> ProgramElementTypes { get; set; }
		public virtual bool SearchByUsageType { get; set; }
		public virtual SortedSet<UsageType> UsageTypes { get; set; }
		public virtual bool SearchByLocation { get; set; }
		public virtual SortedSet<string> Locations { get; set; }
        public virtual bool SearchByFileExtension { get; set; }
        public virtual SortedSet<string> FileExtensions { get; set; }

		private Dictionary<string, float> queryWeights;
	}
}
