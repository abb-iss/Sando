using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Sando.Core;
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
			MatchWholeWord = false;
			ExactMode = true;
			SearchByAccessType = false;
			AccessLevels = new SortedSet<AccessLevel>();
			SearchByProgramElementType = false;
			ProgramElementTypes = new SortedSet<ProgramElementType>();
			SearchByUsageType = false;
			UsageTypes = new SortedSet<UsageType>();
			Locations = new SortedSet<string>();
		}

		public override string ToQueryString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			if(SearchByAccessType)
			{
				AccessLevelCriteriaToString(stringBuilder);
				stringBuilder.Append(" AND ");
			}
			if(SearchByProgramElementType)
			{
				ProgramElementTypeCriteriaToString(stringBuilder);
				stringBuilder.Append(" AND ");
			}
			if(SearchByLocation)
			{
				LocationCriteriaToString(stringBuilder);
				stringBuilder.Append(" AND ");
			}
			UsageTypeCriteriaToString(stringBuilder, SearchByUsageType);
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		private void AccessLevelCriteriaToString(StringBuilder stringBuilder)
		{
			Contract.Requires(AccessLevels != null, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be null!");
			Contract.Requires(AccessLevels.Count > 0, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be empty!");

			stringBuilder.Append("(");
			int collectionSize = AccessLevels.Count;
			foreach(AccessLevel accessLevel in AccessLevels)
			{
				stringBuilder.Append("AccessLevel:");
				stringBuilder.Append(accessLevel.ToString());
				if(collectionSize > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		private void ProgramElementTypeCriteriaToString(StringBuilder stringBuilder)
		{
			Contract.Requires(ProgramElementTypes != null, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be null!");
			Contract.Requires(ProgramElementTypes.Count > 0, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be empty!");

			stringBuilder.Append("(");
			int collectionSize = ProgramElementTypes.Count;
			foreach(ProgramElementType programElementType in ProgramElementTypes)
			{
				stringBuilder.Append("ProgramElementType:");
				stringBuilder.Append(programElementType.ToString());
				if(collectionSize > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		private void LocationCriteriaToString(StringBuilder stringBuilder)
		{
			Contract.Requires(Locations != null, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be null!");
			Contract.Requires(Locations.Count > 0, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be empty!");

			stringBuilder.Append("(");
			int collectionSize = Locations.Count;
			foreach(string location in Locations)
			{
				stringBuilder.Append("FullFilePath:");
				stringBuilder.Append(location);
				if(collectionSize > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		private void UsageTypeCriteriaToString(StringBuilder stringBuilder, bool searchByUsageType)
		{
			Contract.Requires(UsageTypes != null, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be null!");
			Contract.Requires(!SearchByUsageType || UsageTypes.Count > 0, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be empty!");

			stringBuilder.Append("(");
			if(SearchByUsageType)
			{
				int collectionSize = UsageTypes.Count;
				foreach(UsageType usageType in UsageTypes)
				{
					SingleUsageTypeCriteriaToString(stringBuilder, usageType);
					if(collectionSize > 1)
					{
						stringBuilder.Append(" OR ");
					}
					--collectionSize;
				}
			}
			else //all usage types are used
			{
				int collectionSize = Enum.GetValues(typeof(UsageType)).Length;
				foreach(UsageType usageType in Enum.GetValues(typeof(UsageType)))
				{
					SingleUsageTypeCriteriaToString(stringBuilder, usageType);
					if(collectionSize > 1)
					{
						stringBuilder.Append(" OR ");
					}
					--collectionSize;
				}
			}
			stringBuilder.Append(")");
		}

		private void SingleUsageTypeCriteriaToString(StringBuilder stringBuilder, UsageType usageType)
		{
			stringBuilder.Append("(");
			int collectionSize = SearchTerms.Count;
			foreach(string searchTerm in SearchTerms)
			{
				switch(usageType)
				{
					case UsageType.Bodies:
						stringBuilder.Append("Body:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.Definitions:
						stringBuilder.Append("Name:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.EnumValues:
						stringBuilder.Append("Values:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.ExtendedClasses:
						stringBuilder.Append("ExtendedClasses:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.ImplementedInterfaces:
						stringBuilder.Append("ImplementedInterfaces:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.MethodArguments:
						stringBuilder.Append("Arguments:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.MethodReturnTypes:
						stringBuilder.Append("ReturnType:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.NamespaceNames:
						stringBuilder.Append("Namespace:");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.PropertyOrFieldTypes:
						stringBuilder.Append("DataType:");
						stringBuilder.Append(searchTerm);
						break;
					default:
						throw new IndexerException(TranslationCode.Exception_General_UnrecognizedEnumValue, null, "UsageType");
				}
				if(collectionSize > 1)
				{
					stringBuilder.Append(" AND "); //every term must be present in the results
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		public virtual SortedSet<string> SearchTerms { get; set; }
		public virtual bool MatchCase { get; set; }
		public virtual bool MatchWholeWord { get; set; }
		public virtual bool ExactMode { get; set; }
		public virtual bool SearchByAccessType { get; set; }
		public virtual SortedSet<AccessLevel> AccessLevels { get; set; }
		public virtual bool SearchByProgramElementType { get; set; }
		public virtual SortedSet<ProgramElementType> ProgramElementTypes { get; set; }
		public virtual bool SearchByUsageType { get; set; }
		public virtual SortedSet<UsageType> UsageTypes { get; set; }
		public virtual bool SearchByLocation { get; set; }
		public virtual SortedSet<string> Locations { get; set; }
	}
}
