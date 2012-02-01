using System.Collections.Generic;
using Sando.Core;
using System.Text;
using Sando.Indexer.Exceptions;
using Sando.Translation;
using System.Diagnostics.Contracts;

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
			Locations = new List<string>();
		}

		public override string ToQueryString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			if(SearchByAccessType)
			{
				stringBuilder.Append(AccessLevelCriteriaToString());
				stringBuilder.Append(" AND ");
			}
			if(SearchByProgramElementType)
			{
				stringBuilder.Append(ProgramElementTypeCriteriaToString());
				stringBuilder.Append(" AND ");
			}
			if(SearchByLocation)
			{
				stringBuilder.Append(LocationCriteriaToString());
				stringBuilder.Append(" AND ");
			}
			if(SearchByUsageType)
			{
				//TODO search for the terms within the chosen usage types
				stringBuilder.Append(UsageTypeCriteriaToString());
				stringBuilder.Append(" AND ");
			}
			else
			{
				//TODO search for the terms without the chosen usage types
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected string AccessLevelCriteriaToString()
		{
			Contract.Requires(AccessLevels != null, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be null!");

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			for(int i = 0; i < AccessLevels.Count; ++i)
			{
				stringBuilder.Append("AccessLevel:");
				stringBuilder.Append(AccessLevels[i].ToString());
				if(i < AccessLevels.Count - 1)
				{
					stringBuilder.Append(" OR ");
				}
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected string ProgramElementTypeCriteriaToString()
		{
			Contract.Requires(ProgramElementTypes != null, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be null!");

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			for(int i = 0; i < ProgramElementTypes.Count; ++i)
			{
				stringBuilder.Append("ProgramElementType:");
				stringBuilder.Append(ProgramElementTypes[i].ToString());
				if(i < ProgramElementTypes.Count - 1)
				{
					stringBuilder.Append(" OR ");
				}
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected string LocationCriteriaToString()
		{
			Contract.Requires(Locations != null, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be null!");

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			for(int i = 0; i < Locations.Count; ++i)
			{
				stringBuilder.Append("FullFilePath:");
				stringBuilder.Append(Locations[i].ToString());
				if(i < Locations.Count - 1)
				{
					stringBuilder.Append(" OR ");
				}
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected string UsageTypeCriteriaToString()
		{
			Contract.Requires(UsageTypes != null, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be null!");

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			for(int i = 0; i < UsageTypes.Count; ++i)
			{
				switch(UsageTypes[i])
				{
					case UsageType.Definition:
						break;
					case UsageType.MethodArgument:
						break;
					case UsageType.MethodBody:
						break;
					case UsageType.MethodReturnType:
						break;
					case UsageType.PropertyOrFieldType:
						break;
					default:
						throw new IndexerException(TranslationCode.Exception_General_UnrecognizedEnumValue, null, "UsageType");
				}
				if(i < UsageTypes.Count - 1)
				{
					stringBuilder.Append(" OR ");
				}
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
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
		public virtual bool SearchByLocation { get; set; }
		public virtual List<string> Locations { get; set; }
	}
}
